using BookLoans.Abstractions.Models;

namespace BookLoans.Data.Entities;

public class BookLoanEntity
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public BookEntity BookEntity { get; set; } = null!;

    public int BorrowerId { get; set; }

    public BorrowerEntity BorrowerEntity { get; set; } = null!;

    public DateTime CheckedOutAtUtc { get; set; }

    public DateTime? ReturnedAtUtc { get; set; }

    public BookLoan ToBookLoanDto()
        => new()
        {
            Id = Id,
            BookId = BookId,
            BookTitle = BookEntity.Title,
            BookHasDefaultPhoto = BookEntity.Photos.Count > 0,
            BookAuthorNames = BookEntity.BookAuthors.Count == 0
                ? "(unknown)"
                : string.Join(
                    ", ",
                    BookEntity.BookAuthors
                        .OrderBy(bookAuthor => bookAuthor.AuthorEntity.Name)
                        .Select(bookAuthor => bookAuthor.AuthorEntity.Name)),
            BorrowerId = BorrowerId,
            BorrowerFullName = BorrowerEntity.FirstName + " " + BorrowerEntity.LastName,
            CheckedOutAtUtc = CheckedOutAtUtc,
            ReturnedAtUtc = ReturnedAtUtc
        };

    public static BookLoanEntity FromFormDto(BookLoan dto)
        => new()
        {
            BookId = dto.BookId,
            BorrowerId = dto.BorrowerId,
            CheckedOutAtUtc = DateTime.UtcNow
        };
}
