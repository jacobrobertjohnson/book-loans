using BookLoans.Abstractions.Models;

namespace BookLoans.Data.Entities;

public class BookPhotoEntity
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public BookEntity BookEntity { get; set; } = null!;

    public byte[] Content { get; set; } = Array.Empty<byte>();

    public string? Caption { get; set; }

    public int SortOrder { get; set; }

    public DateTime UploadedAtUtc { get; set; }

    public string? MimeType { get; set; }

    public long ByteLength { get; set; }

    public BookPhoto ToAdminBookPhotoItemDto(bool isDefault)
        => new()
        {
            Id = Id,
            IsDefault = isDefault,
            Caption = Caption,
            UploadedAtUtc = UploadedAtUtc
        };

    public BookPhoto ToContentDto()
        => new()
        {
            Content = Content,
            MimeType = MimeType ?? "application/octet-stream"
        };
}
