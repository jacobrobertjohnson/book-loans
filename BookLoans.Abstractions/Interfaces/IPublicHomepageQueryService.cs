using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IPublicHomepageQueryService
{
    Task<IReadOnlyList<BorrowerCheckoutGroup>> GetAsync(CancellationToken ct);
}
