using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Extensions;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoans.Data.Repositories;

public class AdminConditionRepository(AppDbContext dbContext) : IAdminConditionRepository
{
    public async Task<IReadOnlyList<Condition>> GetConditionsAsync(CancellationToken ct)
    {
        List<ConditionEntity> conditions = await dbContext.Conditions
            .AsNoTracking()
            .Include(condition => condition.Books)
            .OrderBy(condition => condition.Name)
            .ToListAsync(ct);

        return conditions
            .Select(condition => condition.ToConditionDto())
            .ToList();
    }

    public async Task<Condition?> GetEditFormAsync(int id, CancellationToken ct)
    {
        ConditionEntity? condition = await dbContext.Conditions
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (condition is null)
        {
            return null;
        }

        return condition.ToAdminConditionFormDto();
    }

    public async Task<string?> CreateAsync(Condition model, CancellationToken ct)
    {
        string? normalizedName = model.Name.NormalizeOrNull();
        if (normalizedName is null)
        {
            return "ConditionEntity name is required.";
        }

        bool exists = await dbContext.Conditions
            .AnyAsync(condition => condition.Name == normalizedName, ct);

        if (exists)
        {
            return "A condition with this name already exists.";
        }

        ConditionEntity condition = ConditionEntity.FromFormDto(model);
        condition.Name = normalizedName;

        dbContext.Conditions.Add(condition);
        await dbContext.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> UpdateAsync(int id, Condition model, CancellationToken ct)
    {
        string? normalizedName = model.Name.NormalizeOrNull();
        if (normalizedName is null)
        {
            return "ConditionEntity name is required.";
        }

        ConditionEntity? condition = await dbContext.Conditions
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (condition is null)
        {
            return "ConditionEntity not found.";
        }

        bool exists = await dbContext.Conditions
            .AnyAsync(entity => entity.Id != id && entity.Name == normalizedName, ct);

        if (exists)
        {
            return "A condition with this name already exists.";
        }

        condition.Name = normalizedName;
        await dbContext.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> DeleteAsync(int id, CancellationToken ct)
    {
        ConditionEntity? condition = await dbContext.Conditions
            .Include(entity => entity.Books)
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (condition is null)
        {
            return "ConditionEntity not found.";
        }

        if (condition.Books.Count > 0)
        {
            return "Cannot delete a condition that is assigned to existing books.";
        }

        dbContext.Conditions.Remove(condition);
        await dbContext.SaveChangesAsync(ct);
        return null;
    }
}
