using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;

namespace BookLoans.Services;

public class PublicHomepageQueryService(IPublicHomepageRepository repository) : IPublicHomepageQueryService
{
    public Task<IReadOnlyList<BorrowerCheckoutGroup>> GetAsync(CancellationToken ct)
        => repository.GetAsync(ct);
}
