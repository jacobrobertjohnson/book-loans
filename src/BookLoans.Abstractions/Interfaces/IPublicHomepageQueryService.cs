using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IPublicHomepageQueryService
{
    Task<HomepageData> GetAsync(CancellationToken ct);
}
