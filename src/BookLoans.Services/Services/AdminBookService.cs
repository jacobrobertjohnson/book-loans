using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;

namespace BookLoans.Services;

public class AdminBookService(IAdminBookRepository repository) : IAdminBookService
{
    public Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct)
        => repository.GetBooksAsync(ct);

    public Task<Book> GetCreateFormAsync(CancellationToken ct)
        => repository.GetCreateFormAsync(ct);

    public Task<Book?> GetEditFormAsync(int id, CancellationToken ct)
        => repository.GetEditFormAsync(id, ct);

    public Task<string?> SetDefaultPhotoAsync(int bookId, int photoId, CancellationToken ct)
        => repository.SetDefaultPhotoAsync(bookId, photoId, ct);

    public Task<string?> UploadPhotoAsync(int bookId, BookPhoto? photo, CancellationToken ct)
        => repository.UploadPhotoAsync(bookId, photo, ct);

    public Task<string?> DeletePhotoAsync(int bookId, int photoId, CancellationToken ct)
        => repository.DeletePhotoAsync(bookId, photoId, ct);

    public Task<BookPhoto?> GetPhotoContentAsync(int photoId, CancellationToken ct)
        => repository.GetPhotoContentAsync(photoId, ct);

    public Task<BookPhoto?> GetDefaultPhotoContentAsync(int bookId, CancellationToken ct)
        => repository.GetDefaultPhotoContentAsync(bookId, ct);

    public Task<string?> CreateAsync(Book model, CancellationToken ct)
        => repository.CreateAsync(model, ct);

    public Task<string?> UpdateAsync(int id, Book model, CancellationToken ct)
        => repository.UpdateAsync(id, model, ct);

    public Task<string?> DeleteAsync(int id, CancellationToken ct)
        => repository.DeleteAsync(id, ct);

    public Task<BookImportResult> ImportBooksFromCsvAsync(Stream stream, CancellationToken ct)
        => repository.ImportBooksFromCsvAsync(stream, ct);

    public Task<string> ExportBooksToCsvAsync(CancellationToken ct)
        => repository.ExportBooksToCsvAsync(ct);
}
