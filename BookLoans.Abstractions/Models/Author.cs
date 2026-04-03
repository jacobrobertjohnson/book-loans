namespace BookLoans.Abstractions.Models;

public class Author
{
    public int? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int BookCount { get; init; }
}
