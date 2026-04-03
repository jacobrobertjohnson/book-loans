using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Extensions;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoans.Data.Repositories;

public class AdminAuthorRepository(AppDbContext dbContext) : IAdminAuthorRepository
{
    public async Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct)
    {
        List<AuthorEntity> authors = await dbContext.Authors
            .AsNoTracking()
            .Include(author => author.BookAuthors)
            .OrderBy(author => author.Name)
            .ToListAsync(ct);

        return authors
            .Select(author => author.ToAuthorDto())
            .ToList();
    }

    public async Task<Author?> GetEditFormAsync(int id, CancellationToken ct)
    {
        AuthorEntity? author = await dbContext.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (author is null)
        {
            return null;
        }

        return author.ToAdminAuthorFormDto();
    }

    public async Task<string?> CreateAsync(Author model, CancellationToken ct)
    {
        string? normalizedName = model.Name.NormalizeOrNull();
        if (normalizedName is null)
        {
            return "AuthorEntity name is required.";
        }

        bool exists = await dbContext.Authors
            .AnyAsync(author => author.Name == normalizedName, ct);

        if (exists)
        {
            return "An author with this name already exists.";
        }

        AuthorEntity author = AuthorEntity.FromFormDto(model);
        author.Name = normalizedName;

        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> UpdateAsync(int id, Author model, CancellationToken ct)
    {
        string? normalizedName = model.Name.NormalizeOrNull();
        if (normalizedName is null)
        {
            return "AuthorEntity name is required.";
        }

        AuthorEntity? author = await dbContext.Authors
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (author is null)
        {
            return "AuthorEntity not found.";
        }

        bool exists = await dbContext.Authors
            .AnyAsync(entity => entity.Id != id && entity.Name == normalizedName, ct);

        if (exists)
        {
            return "An author with this name already exists.";
        }

        author.Name = normalizedName;
        await dbContext.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> DeleteAsync(int id, CancellationToken ct)
    {
        AuthorEntity? author = await dbContext.Authors
            .Include(entity => entity.BookAuthors)
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (author is null)
        {
            return "AuthorEntity not found.";
        }

        if (author.BookAuthors.Count > 0)
        {
            return "Cannot delete an author that is linked to existing books.";
        }

        dbContext.Authors.Remove(author);
        await dbContext.SaveChangesAsync(ct);
        return null;
    }
}
