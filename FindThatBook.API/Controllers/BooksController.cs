using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using FindThatBook.Domain;
using FindThatBook.Application.Interfaces;
using FindThatBook.Application.DTOs;
using FindThatBook.Application.Wrappers;
using FindThatBook.Application.Extensions;

namespace FindThatBook.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IAIParser _aiParser;
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IAIParser aiParser, IBookService bookService, ILogger<BooksController> logger)
    {
        _aiParser = aiParser;
        _bookService = bookService;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new ApiResponse<List<BookDTO>>(errors));
        }

        try
        {
            _logger.LogInformation("Processing search for query: {Query}", request.Query);
            var sanitizedQuery = request.Query.SanitizeForPrompt();
            var (cleanTitle, cleanAuthor, cleanKeywords) = await _aiParser.ParseQueryAsync(sanitizedQuery);
            var keywordList = cleanKeywords?.ToList() ?? new List<string>();
            if (string.IsNullOrWhiteSpace(cleanTitle) && 
                string.IsNullOrWhiteSpace(cleanAuthor) && 
                !keywordList.Any())
            {
                 return Ok(new ApiResponse<List<BookDTO>>(new List<BookDTO>(), "No books found."));
            }
            var searchResults = await _bookService.SearchBooksAsync(
                cleanTitle,
                cleanAuthor,
                keywordList
            );

            if (!searchResults.Any())
            {
                return Ok(new ApiResponse<List<BookDTO>>(new List<BookDTO>(), "No books found."));
            }
            var firstBook = searchResults.First();
            bool isClearWinner = IsStrongMatch(firstBook, cleanTitle, cleanAuthor);

            int actualTake = isClearWinner ? 1 : request.PageSize;

            var pagedBooks = searchResults
                .Skip((request.Page - 1) * request.PageSize)
                .Take(actualTake)
                .ToList();
            var processingTasks = pagedBooks.Select(async book =>
            {
                string openLibraryUrl = book.Key != null ? $"https://openlibrary.org{book.Key}" : null;
                var explanation = await _aiParser.GenerateExplanationAsync(request.Query, book.Title, book.Author);
                return new BookDTO
                {
                    Title = book.Title,
                    Author = book.Author,
                    CoverUrl = book.CoverUrl,
                    FirstPublishYear = book.FirstPublishYear,
                    OpenLibraryUrl = openLibraryUrl,
                    Explanation = explanation
                };
            });

            var resultDtos = await Task.WhenAll(processingTasks);

            return Ok(new ApiResponse<List<BookDTO>>(resultDtos.ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for {Query}", request.Query);
            return StatusCode(500, new ApiResponse<List<BookDTO>>("An internal error occurred."));
        }
    }

    private bool IsStrongMatch(Book book, string? targetTitle, string? targetAuthor)
    {
        if (string.IsNullOrWhiteSpace(targetTitle)) return false;
        var bookTitle = book.Title.ToNormalizedString();
        var searchTitle = targetTitle.ToNormalizedString();
        bool titleMatches = bookTitle.Contains(searchTitle);
        if (!titleMatches) return false;
        if (!string.IsNullOrWhiteSpace(targetAuthor) && !string.IsNullOrWhiteSpace(book.Author))
        {
            var bookAuthorNorm = book.Author.ToNormalizedString();
            var targetAuthorParts = targetAuthor.ToNormalizedString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool authorMatches = targetAuthorParts.All(part => bookAuthorNorm.Contains(part));
            
            return authorMatches;
        }
        return true;
    }
}