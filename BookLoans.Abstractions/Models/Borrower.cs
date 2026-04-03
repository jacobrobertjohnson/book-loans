using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Models;

public class Borrower
{
    public int? Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public int CurrentBookCount { get; init; }

    public int LoanCount { get; init; }

    public IReadOnlyList<BookLoan> CurrentCheckouts { get; init; } = [];

    public IReadOnlyList<BookLoan> CheckoutHistory { get; init; } = [];
}
