namespace BookLoans.Abstractions.Models;

public record BookImportRowError(int RowNumber, string Title, string ErrorMessage);

public class BookImportResult
{
    public int SuccessCount { get; init; }

    public IReadOnlyList<BookImportRowError> Errors { get; init; } = [];
}
