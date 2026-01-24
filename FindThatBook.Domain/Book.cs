namespace FindThatBook.Domain;

public class Book
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int FirstPublishYear { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? OpenLibraryKey { get; set; }
}
