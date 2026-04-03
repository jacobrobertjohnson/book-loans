using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BookPhotoItemViewModel
{
    public int Id { get; init; }

    public bool IsDefault { get; init; }

    public string? Caption { get; init; }

    public DateTime UploadedAtUtc { get; init; }

    public static BookPhotoItemViewModel FromDto(BookPhoto dto)
        => dto.Adapt<BookPhotoItemViewModel>();

    public BookPhoto ToDto()
        => this.Adapt<BookPhoto>();
}
