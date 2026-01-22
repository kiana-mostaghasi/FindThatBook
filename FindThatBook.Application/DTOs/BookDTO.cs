namespace FindThatBook.Application.DTOs;

public class BookDTO
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public int FirstPublishYear { get; set; }
    public string OpenLibraryUrl { get; set; }
    public string? Explanation { get; set; }
}