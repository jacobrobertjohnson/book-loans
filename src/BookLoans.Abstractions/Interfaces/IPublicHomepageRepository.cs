using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IPublicHomepageRepository
{
    Task<HomepageData> GetAsync(CancellationToken ct);
}
