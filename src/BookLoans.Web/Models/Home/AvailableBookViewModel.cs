using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public sealed class AvailableBookViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? SeriesName { get; init; }

    public string AuthorNames { get; init; } = string.Empty;

    public static AvailableBookViewModel FromDto(Book dto)
        => dto.Adapt<AvailableBookViewModel>();

    public Book ToDto()
        => this.Adapt<Book>();
}
