namespace BookLoans.Abstractions.Models;

public class Book
{
    public int? Id { get; init; }

    public bool HasDefaultPhoto { get; init; }

    public string Title { get; init; } = string.Empty;

    public string AuthorNames { get; init; } = string.Empty;

    public string? Isbn { get; init; }

    public string ConditionName { get; init; } = string.Empty;

    public string? CurrentBorrowerName { get; init; }

    public DateTime? CurrentCheckedOutAtUtc { get; init; }

    public IReadOnlyList<int> SelectedAuthorIds { get; init; } = [];

    public string? NewAuthorNames { get; init; }

    public string? Edition { get; init; }

    public int YearFirstPublished { get; init; }

    public int? YearEditionPublished { get; init; }

    public DateOnly? DateOfPurchase { get; init; }

    public string? LocationOfPurchase { get; init; }

    public int ConditionId { get; init; }

    public bool IsCheckedOut { get; init; }

    public IReadOnlyList<Author> Authors { get; init; } = [];

    public IReadOnlyList<Condition> Conditions { get; init; } = [];

    public BookLoan? CurrentCheckout { get; init; }

    public IReadOnlyList<BookLoan> CheckoutHistory { get; init; } = [];

    public IReadOnlyList<BookPhoto> BookPhotos { get; init; } = [];
}
