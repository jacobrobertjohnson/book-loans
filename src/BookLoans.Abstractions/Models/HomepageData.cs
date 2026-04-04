namespace BookLoans.Abstractions.Models;

public class HomepageData
{
    public IReadOnlyList<BorrowerCheckoutGroup> BorrowerGroups { get; init; } = [];

    public IReadOnlyList<Book> AvailableBooks { get; init; } = [];
}
