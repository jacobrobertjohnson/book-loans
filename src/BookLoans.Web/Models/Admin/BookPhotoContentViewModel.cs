namespace BookLoans.Web.Models;

public class BookPhotoContentViewModel
{
    public byte[] Content { get; init; } = [];

    public string MimeType { get; init; } = "application/octet-stream";
}
