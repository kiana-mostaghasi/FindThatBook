using System.Text.Json;
using FindThatBook.Domain;

namespace FindThatBook.Infrastructure.Extensions;

public static class OpenLibraryExtensions
{
    public static Book ToBook(this JsonElement item)
    {
        string foundTitle = item.TryGetProperty("title", out var t) ? t.GetString() ?? "Unknown Title" : "Unknown Title";

        string foundAuthor = "Unknown";
        if (item.TryGetProperty("author_name", out var aList) && aList.GetArrayLength() > 0)
        {
            foundAuthor = aList[0].GetString() ?? "Unknown";
        }

        int year = 0;
        if (item.TryGetProperty("first_publish_year", out var y))
        {
            year = y.ValueKind == JsonValueKind.Number ? y.GetInt32() : 0;
        }

        string key = string.Empty;
        if (item.TryGetProperty("key", out var k))
        {
            key = k.GetString() ?? string.Empty;
        }

        string coverUrl = string.Empty;
        if (item.TryGetProperty("cover_i", out var c) &&
            c.ValueKind == JsonValueKind.Number)
        {
            coverUrl = $"https://covers.openlibrary.org/b/id/{c.GetInt32()}-M.jpg";
        }

        return new Book
        {
            Title = foundTitle,
            Author = foundAuthor,
            FirstPublishYear = year,
            Key = key,
            CoverUrl = coverUrl
        };
    }
}