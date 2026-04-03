using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoans.Data.Repositories;

public class PublicHomepageRepository(AppDbContext dbContext) : IPublicHomepageRepository
{
    public async Task<IReadOnlyList<BorrowerCheckoutGroup>> GetAsync(CancellationToken ct)
    {
        List<BookLoanEntity> activeCheckouts = await dbContext.BookLoans
            .AsNoTracking()
            .Include(loan => loan.BorrowerEntity)
            .Include(loan => loan.BookEntity)
            .ThenInclude(book => book.Photos)
            .Include(loan => loan.BookEntity)
            .ThenInclude(book => book.BookAuthors)
            .ThenInclude(bookAuthor => bookAuthor.AuthorEntity)
            .Where(loan => loan.ReturnedAtUtc == null)
            .OrderBy(loan => loan.CheckedOutAtUtc)
            .ToListAsync(ct);

        List<BorrowerCheckoutGroup> groups = activeCheckouts
            .GroupBy(loan => new { loan.BorrowerEntity.FirstName, loan.BorrowerEntity.LastName })
            .Select(group => new BorrowerCheckoutGroup
            {
                BorrowerFullName = group.Key.FirstName + " " + group.Key.LastName,
                Books = group
                    .OrderBy(loan => loan.CheckedOutAtUtc)
                    .Select(loan => loan.ToBookLoanDto())
                    .ToList()
            })
            .OrderBy(group => group.Books.First().CheckedOutAtUtc)
            .ToList();

        return groups;
    }
}
