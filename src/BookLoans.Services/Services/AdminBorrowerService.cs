using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;

namespace BookLoans.Services;

public class AdminBorrowerService(IAdminBorrowerRepository repository) : IAdminBorrowerService
{
    public Task<IReadOnlyList<Borrower>> GetBorrowersAsync(CancellationToken ct)
        => repository.GetBorrowersAsync(ct);

    public Task<Borrower?> GetEditFormAsync(int id, CancellationToken ct)
        => repository.GetEditFormAsync(id, ct);

    public Task<string?> CreateAsync(Borrower model, CancellationToken ct)
        => repository.CreateAsync(model, ct);

    public Task<string?> UpdateAsync(int id, Borrower model, CancellationToken ct)
        => repository.UpdateAsync(id, model, ct);

    public Task<string?> DeleteAsync(int id, CancellationToken ct)
        => repository.DeleteAsync(id, ct);
}
