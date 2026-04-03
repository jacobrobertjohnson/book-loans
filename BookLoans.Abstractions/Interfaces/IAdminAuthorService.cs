using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IAdminAuthorService
{
    Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct);

    Author GetCreateForm();

    Task<Author?> GetEditFormAsync(int id, CancellationToken ct);

    Task<string?> CreateAsync(Author model, CancellationToken ct);

    Task<string?> UpdateAsync(int id, Author model, CancellationToken ct);

    Task<string?> DeleteAsync(int id, CancellationToken ct);
}
