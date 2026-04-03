namespace BookLoans.Abstractions.Models;

public class BookPhoto
{
    public int Id { get; init; }

    public bool IsDefault { get; init; }

    public byte[] Content { get; init; } = [];

    public string MimeType { get; init; } = "application/octet-stream";

    public string? Caption { get; init; }

    public DateTime UploadedAtUtc { get; init; }
}
