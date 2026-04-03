using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BookLoanViewModel
{
    public int Id { get; init; }

    public int BookId { get; init; }

    public string BookTitle { get; init; } = string.Empty;

    public bool BookHasDefaultPhoto { get; init; }

    public string BookAuthorNames { get; init; } = string.Empty;

    public int BorrowerId { get; init; }

    public string BorrowerFullName { get; init; } = string.Empty;

    public DateTime CheckedOutAtUtc { get; init; }

    public DateTime? ReturnedAtUtc { get; init; }

    public static BookLoanViewModel FromDto(BookLoan dto)
        => dto.Adapt<BookLoanViewModel>();

    public BookLoan ToDto()
        => this.Adapt<BookLoan>();
}
