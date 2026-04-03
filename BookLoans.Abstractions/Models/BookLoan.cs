namespace BookLoans.Abstractions.Models;

public class BookLoan
{
    public int Id { get; init; }

    public int BookId { get; init; }

    public string BookTitle { get; init; } = string.Empty;

    public bool BookHasDefaultPhoto { get; init; }

    public string BookAuthorNames { get; init; } = string.Empty;

    public int BorrowerId { get; init; }

    public string BorrowerFullName { get; init; } = string.Empty;

    public IReadOnlyList<Book> Books { get; init; } = [];

    public IReadOnlyList<Borrower> Borrowers { get; init; } = [];

    public DateTime CheckedOutAtUtc { get; init; }

    public DateTime? ReturnedAtUtc { get; init; }
}
