using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FindThatBook.Application.Interfaces;
using FindThatBook.Application.Extensions;
using FindThatBook.Infrastructure.Prompts.Parsing;
using FindThatBook.Infrastructure.Prompts.Explanations;

namespace FindThatBook.Infrastructure;

public class GeminiParser : IAIParser
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<GeminiParser> _logger;

    public GeminiParser(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiParser> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var rawKey = configuration["Gemini:ApiKey"];
        _apiKey = rawKey?.Trim() ?? throw new ArgumentNullException(nameof(configuration), "Gemini API Key is missing.");

        _modelName = configuration["Gemini:Model"] ?? "gemini-pro";
    }

    public async Task<(string? Title, string? Author, List<string>? Keywords)> ParseQueryAsync(string originalQuery)
    {
        var prompt = string.Format(ParsingPromptsV1.SystemInstruction, originalQuery);

        try
        {
            var responseText = await CallGemini(prompt);
            var cleanJson = responseText.SanitizeJson();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ParsedResponse>(cleanJson, options);

            return (result?.Title, result?.Author, result?.Keywords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Parsing failed for query: {Query}", originalQuery);
            return (originalQuery, null, null);
        }
    }

    public async Task<string> GenerateExplanationAsync(string originalQuery, string bookTitle, string bookAuthor)
    {
        var prompt = string.Format(ExplanationPromptsV1.StandardReasoning, originalQuery, bookTitle, bookAuthor);

        try
        {
            return await CallGemini(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate explanation for book: {Title}", bookTitle);
            return "AI Explanation unavailable at this time.";
        }
    }

    private async Task<string> CallGemini(string promptText)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";

        _logger.LogDebug("Calling Gemini Model: {Model}", _modelName);

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = promptText } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API Error: {StatusCode} - {Body}", response.StatusCode, errorBody);
            throw new HttpRequestException($"Gemini API Failed with status {response.StatusCode}");
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        var root = doc.RootElement;

        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var contentElem) &&
                contentElem.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0)
            {
                return parts[0].GetProperty("text").GetString() ?? "";
            }
        }

        return "";
    }

    private class ParsedResponse
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }
    }
}