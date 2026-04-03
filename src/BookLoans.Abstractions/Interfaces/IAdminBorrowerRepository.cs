using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IAdminBorrowerRepository
{
    Task<IReadOnlyList<Borrower>> GetBorrowersAsync(CancellationToken ct);

    Task<Borrower?> GetEditFormAsync(int id, CancellationToken ct);

    Task<string?> CreateAsync(Borrower model, CancellationToken ct);

    Task<string?> UpdateAsync(int id, Borrower model, CancellationToken ct);

    Task<string?> DeleteAsync(int id, CancellationToken ct);
}
