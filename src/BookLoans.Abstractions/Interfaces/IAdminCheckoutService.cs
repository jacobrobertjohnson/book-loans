using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IAdminCheckoutService
{
    Task<IReadOnlyList<BookLoan>> GetCheckoutsAsync(CancellationToken ct);

    Task<BookLoan> GetCreateFormAsync(int? preselectedBookId, int? preselectedBorrowerId, CancellationToken ct);

    Task<BookLoan> RebuildFormAsync(BookLoan model, CancellationToken ct);

    Task<string?> CreateAsync(BookLoan model, CancellationToken ct);

    Task<string?> ReturnAsync(int loanId, CancellationToken ct);
}
