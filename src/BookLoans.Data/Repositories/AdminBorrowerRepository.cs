using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Extensions;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoans.Data.Repositories;

public class AdminBorrowerRepository(AppDbContext dbContext) : IAdminBorrowerRepository
{
    public async Task<IReadOnlyList<Borrower>> GetBorrowersAsync(CancellationToken ct)
    {
        List<BorrowerEntity> borrowers = await dbContext.Borrowers
            .AsNoTracking()
            .Include(borrower => borrower.CurrentBooks)
            .Include(borrower => borrower.Loans)
            .OrderBy(borrower => borrower.LastName)
            .ThenBy(borrower => borrower.FirstName)
            .ToListAsync(ct);

        return borrowers
            .Select(borrower => borrower.ToAdminBorrowerListItemDto())
            .ToList();
    }

    public async Task<Borrower?> GetEditFormAsync(int id, CancellationToken ct)
    {
        BorrowerEntity? borrower = await dbContext.Borrowers
            .AsNoTracking()
            .Include(entity => entity.Loans)
            .ThenInclude(loan => loan.BookEntity)
            .ThenInclude(book => book.Photos)
            .Include(entity => entity.Loans)
            .ThenInclude(loan => loan.BookEntity)
            .ThenInclude(book => book.BookAuthors)
            .ThenInclude(bookAuthor => bookAuthor.AuthorEntity)
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (borrower is null)
        {
            return null;
        }

        return borrower.ToAdminBorrowerFormDto();
    }

    public async Task<string?> CreateAsync(Borrower model, CancellationToken ct)
    {
        string? normalizedFirstName = model.FirstName.NormalizeOrNull();
        string? normalizedLastName = model.LastName.NormalizeOrNull();
        if (normalizedFirstName is null || normalizedLastName is null)
        {
            return "First and last name are required.";
        }

        bool exists = await dbContext.Borrowers
            .AnyAsync(
                borrower => borrower.FirstName == normalizedFirstName && borrower.LastName == normalizedLastName,
                ct);

        if (exists)
        {
            return "A borrower with this name already exists.";
        }

        BorrowerEntity borrower = BorrowerEntity.FromFormDto(model);
        borrower.FirstName = normalizedFirstName;
        borrower.LastName = normalizedLastName;

        dbContext.Borrowers.Add(borrower);
        await dbContext.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> UpdateAsync(int id, Borrower model, CancellationToken ct)
    {
        string? normalizedFirstName = model.FirstName.NormalizeOrNull();
        string? normalizedLastName = model.LastName.NormalizeOrNull();
        if (normalizedFirstName is null || normalizedLastName is null)
        {
            return "First and last name are required.";
        }

        BorrowerEntity? borrower = await dbContext.Borrowers
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (borrower is null)
        {
            return "BorrowerEntity not found.";
        }

        bool exists = await dbContext.Borrowers
            .AnyAsync(
                entity => entity.Id != id && entity.FirstName == normalizedFirstName && entity.LastName == normalizedLastName,
                ct);

        if (exists)
        {
            return "A borrower with this name already exists.";
        }

        borrower.FirstName = normalizedFirstName;
        borrower.LastName = normalizedLastName;
        await dbContext.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> DeleteAsync(int id, CancellationToken ct)
    {
        BorrowerEntity? borrower = await dbContext.Borrowers
            .Include(entity => entity.CurrentBooks)
            .Include(entity => entity.Loans)
            .FirstOrDefaultAsync(entity => entity.Id == id, ct);

        if (borrower is null)
        {
            return "BorrowerEntity not found.";
        }

        if (borrower.CurrentBooks.Count > 0)
        {
            return "Cannot delete a borrower while they still have checked out books.";
        }

        if (borrower.Loans.Count > 0)
        {
            return "Cannot delete a borrower with checkout history.";
        }

        dbContext.Borrowers.Remove(borrower);
        await dbContext.SaveChangesAsync(ct);
        return null;
    }
}
