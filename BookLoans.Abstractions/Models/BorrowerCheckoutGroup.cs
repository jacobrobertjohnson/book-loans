using BookLoans.Abstractions.Models;

namespace BookLoans.Abstractions.Models;

public class BorrowerCheckoutGroup
{
    public string BorrowerFullName { get; init; } = string.Empty;

    public IReadOnlyList<BookLoan> Books { get; init; } = [];
}
