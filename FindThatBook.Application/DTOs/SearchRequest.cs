using System.ComponentModel.DataAnnotations;

namespace FindThatBook.Application.DTOs;

public class SearchRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Query cannot be empty")]
    public string Query { get; set; } = null!;

    [Range(1, 50)]
    public int PageSize { get; set; } = 5;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
}