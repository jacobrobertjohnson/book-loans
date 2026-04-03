using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IPublicHomepageRepository
{
    Task<IReadOnlyList<BorrowerCheckoutGroup>> GetAsync(CancellationToken ct);
}
