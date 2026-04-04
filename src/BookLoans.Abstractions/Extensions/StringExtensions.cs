namespace BookLoans.Abstractions.Extensions;

public static class StringExtensions
{
    public static string? NormalizeOrNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    public static string NormalizeForSort(this string value)
    {
        string trimmed = value.Trim();

        if (trimmed.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
            return trimmed[4..];
        if (trimmed.StartsWith("an ", StringComparison.OrdinalIgnoreCase))
            return trimmed[3..];
        if (trimmed.StartsWith("a ", StringComparison.OrdinalIgnoreCase))
            return trimmed[2..];

        return trimmed;
    }
}
