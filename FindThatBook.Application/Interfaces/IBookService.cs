using FindThatBook.Domain;

namespace FindThatBook.Application.Interfaces;

public interface IBookService
{
    Task<List<Book>> SearchBooksAsync(string? title, string? author, List<string> keywords);
}
