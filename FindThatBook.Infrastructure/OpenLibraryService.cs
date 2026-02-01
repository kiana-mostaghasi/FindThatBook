using System.Text.Json;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain;
using FindThatBook.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FindThatBook.Infrastructure;

public class OpenLibraryService : IBookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenLibraryService> _logger;

    public OpenLibraryService(HttpClient httpClient, ILogger<OpenLibraryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FindThatBook/1.0 (kiana@example.com)");
    }

    public async Task<List<Book>> SearchBooksAsync(string? title, string? author, List<string> keywords)
    {
        if (title == null && author == null && !keywords.Any())
        {
            return new List<Book>();
        }
        if (!string.IsNullOrWhiteSpace(title))
        {
            _logger.LogInformation("Attempting Precise Search: Title='{Title}', Author='{Author}'", title, author);
            var queryParams = $"title={Uri.EscapeDataString(title)}";
            
            if (!string.IsNullOrWhiteSpace(author))
            {
                queryParams += $"&author={Uri.EscapeDataString(author)}";
            }

            var preciseResults = await FetchFromOpenLibrary(queryParams);

            if (preciseResults.Any())
            {
                return preciseResults;
            }

            _logger.LogWarning("Precise search yielded 0 results. Falling back to Broad Keyword Search.");
        }
        var searchTerms = new List<string>();
        if (!string.IsNullOrWhiteSpace(title)) searchTerms.Add(title);
        if (!string.IsNullOrWhiteSpace(author)) searchTerms.Add(author);
        if (keywords != null) searchTerms.AddRange(keywords);

        var combinedQuery = string.Join(" ", searchTerms);

        if (string.IsNullOrWhiteSpace(combinedQuery)) return new List<Book>();

        _logger.LogInformation("Attempting Broad Search: Query='{Query}'", combinedQuery);
        return await FetchFromOpenLibrary($"q={Uri.EscapeDataString(combinedQuery)}");
    }

    private async Task<List<Book>> FetchFromOpenLibrary(string queryParams)
    {
        try
        {
            var fields = "key,title,author_name,editions,cover_i,first_publish_year";
            var url = $"https://openlibrary.org/search.json?{queryParams}&fields={fields}&limit=5";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var results = new List<Book>();

            if (doc.RootElement.TryGetProperty("docs", out var docs))
            {
                foreach (var item in docs.EnumerateArray())
                {
                    results.Add(item.ToBook());
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenLibrary API call failed for query: {Params}", queryParams);
            return new List<Book>();
        }
    }
}
