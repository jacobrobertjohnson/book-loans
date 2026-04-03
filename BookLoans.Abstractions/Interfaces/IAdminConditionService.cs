using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Interfaces;

public interface IAdminConditionService
{
    Task<IReadOnlyList<Condition>> GetConditionsAsync(CancellationToken ct);

    Condition GetCreateForm();

    Task<Condition?> GetEditFormAsync(int id, CancellationToken ct);

    Task<string?> CreateAsync(Condition model, CancellationToken ct);

    Task<string?> UpdateAsync(int id, Condition model, CancellationToken ct);

    Task<string?> DeleteAsync(int id, CancellationToken ct);
}
