using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IAdminBookService
{
    Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct);

    Task<Book> GetCreateFormAsync(CancellationToken ct);

    Task<Book?> GetEditFormAsync(int id, CancellationToken ct);

    Task<string?> SetDefaultPhotoAsync(int bookId, int photoId, CancellationToken ct);

    Task<string?> UploadPhotoAsync(int bookId, BookPhoto? photo, CancellationToken ct);

    Task<string?> DeletePhotoAsync(int bookId, int photoId, CancellationToken ct);

    Task<BookPhoto?> GetPhotoContentAsync(int photoId, CancellationToken ct);

    Task<BookPhoto?> GetDefaultPhotoContentAsync(int bookId, CancellationToken ct);

    Task<string?> CreateAsync(Book model, CancellationToken ct);

    Task<string?> UpdateAsync(int id, Book model, CancellationToken ct);

    Task<string?> DeleteAsync(int id, CancellationToken ct);

    Task<BookImportResult> ImportBooksFromCsvAsync(Stream stream, CancellationToken ct);
}
