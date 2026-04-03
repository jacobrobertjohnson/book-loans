using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BookListItemViewModel
{
    public int Id { get; init; }

    public bool HasDefaultPhoto { get; init; }

    public string Title { get; init; } = string.Empty;

    public string AuthorNames { get; init; } = string.Empty;

    public string? Isbn { get; init; }

    public string ConditionName { get; init; } = string.Empty;

    public DateTime? CurrentCheckedOutAtUtc { get; init; }

    public string? CurrentBorrowerName { get; init; }

    public static BookListItemViewModel FromDto(Book dto)
        => dto.Adapt<BookListItemViewModel>();

    public Book ToDto()
        => this.Adapt<Book>();
}
