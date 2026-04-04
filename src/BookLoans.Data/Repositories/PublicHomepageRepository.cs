using BookLoans.Abstractions.Extensions;
using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoans.Data.Repositories;

public class PublicHomepageRepository(AppDbContext dbContext) : IPublicHomepageRepository
{
    public async Task<HomepageData> GetAsync(CancellationToken ct)
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

        List<Book> availableBooks = (await dbContext.Books
            .AsNoTracking()
            .Include(book => book.Loans)
            .Include(book => book.BookAuthors)
            .ThenInclude(bookAuthor => bookAuthor.AuthorEntity)
            .Include(book => book.Series)
            .Where(book => !book.Loans.Any(loan => loan.ReturnedAtUtc == null))
            .Select(book => new Book
            {
                Id = book.Id,
                Title = book.Title,
                SeriesName = book.Series != null ? book.Series.Name : null,
                AuthorNames = book.BookAuthors.Count == 0
                    ? "(unknown)"
                    : string.Join(
                        ", ",
                        book.BookAuthors
                            .OrderBy(bookAuthor => bookAuthor.AuthorEntity.Name)
                            .Select(bookAuthor => bookAuthor.AuthorEntity.Name))
            })
            .ToListAsync(ct))
            .OrderBy(book => (book.SeriesName ?? book.Title).NormalizeForSort())
            .ThenBy(book => book.Title.NormalizeForSort())
            .ToList();

        return new HomepageData
        {
            BorrowerGroups = groups,
            AvailableBooks = availableBooks
        };
    }
}
