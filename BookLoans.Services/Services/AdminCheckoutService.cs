using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;

namespace BookLoans.Services;

public class AdminCheckoutService(IAdminCheckoutRepository repository) : IAdminCheckoutService
{
    public Task<IReadOnlyList<BookLoan>> GetCheckoutsAsync(CancellationToken ct)
        => repository.GetCheckoutsAsync(ct);

    public Task<BookLoan> GetCreateFormAsync(int? preselectedBookId, int? preselectedBorrowerId, CancellationToken ct)
        => repository.GetCreateFormAsync(preselectedBookId, preselectedBorrowerId, ct);

    public Task<BookLoan> RebuildFormAsync(BookLoan model, CancellationToken ct)
        => repository.RebuildFormAsync(model, ct);

    public Task<string?> CreateAsync(BookLoan model, CancellationToken ct)
        => repository.CreateAsync(model, ct);

    public Task<string?> ReturnAsync(int loanId, CancellationToken ct)
        => repository.ReturnAsync(loanId, ct);
}
