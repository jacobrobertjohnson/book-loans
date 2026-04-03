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
}