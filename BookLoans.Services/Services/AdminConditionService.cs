using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;

namespace BookLoans.Services;

public class AdminConditionService(IAdminConditionRepository repository) : IAdminConditionService
{
    public Task<IReadOnlyList<Condition>> GetConditionsAsync(CancellationToken ct)
        => repository.GetConditionsAsync(ct);

    public Condition GetCreateForm()
    {
        return new Condition();
    }

    public Task<Condition?> GetEditFormAsync(int id, CancellationToken ct)
        => repository.GetEditFormAsync(id, ct);

    public Task<string?> CreateAsync(Condition model, CancellationToken ct)
        => repository.CreateAsync(model, ct);

    public Task<string?> UpdateAsync(int id, Condition model, CancellationToken ct)
        => repository.UpdateAsync(id, model, ct);

    public Task<string?> DeleteAsync(int id, CancellationToken ct)
        => repository.DeleteAsync(id, ct);
}
