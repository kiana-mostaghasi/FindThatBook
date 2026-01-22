namespace FindThatBook.Application.Interfaces;

public interface IAIParser
{
    Task<(string? Title, string? Author, List<string>? Keywords)> ParseQueryAsync(string originalQuery);
    Task<string> GenerateExplanationAsync(string originalQuery, string bookTitle, string bookAuthor);
}
