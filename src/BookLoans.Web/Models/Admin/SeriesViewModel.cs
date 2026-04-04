using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class SeriesViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int BookCount { get; init; }

    public static SeriesViewModel FromDto(Series dto)
        => dto.Adapt<SeriesViewModel>();

    public Series ToDto()
        => this.Adapt<Series>();
}
