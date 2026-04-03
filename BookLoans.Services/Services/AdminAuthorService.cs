using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;

namespace BookLoans.Services;

public class AdminAuthorService(IAdminAuthorRepository repository) : IAdminAuthorService
{
    public Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct)
        => repository.GetAuthorsAsync(ct);

    public Author GetCreateForm()
    {
        return new Author();
    }

    public Task<Author?> GetEditFormAsync(int id, CancellationToken ct)
        => repository.GetEditFormAsync(id, ct);

    public Task<string?> CreateAsync(Author model, CancellationToken ct)
        => repository.CreateAsync(model, ct);

    public Task<string?> UpdateAsync(int id, Author model, CancellationToken ct)
        => repository.UpdateAsync(id, model, ct);

    public Task<string?> DeleteAsync(int id, CancellationToken ct)
        => repository.DeleteAsync(id, ct);
}
