namespace BookLoans.UnitTests.Extensions;

using BookLoans.Abstractions.Extensions;
using Xunit;

public class StringExtensionsTests
{
    [Fact]
    public void NormalizeOrNull_WithNull_ReturnsNull()
    {
        string? input = null;

        var result = input.NormalizeOrNull();

        Assert.Null(result);
    }

    [Fact]
    public void NormalizeOrNull_WithEmptyString_ReturnsNull()
    {
        string input = string.Empty;

        var result = input.NormalizeOrNull();

        Assert.Null(result);
    }

    [Fact]
    public void NormalizeOrNull_WithWhitespace_ReturnsNull()
    {
        string input = "   ";

        var result = input.NormalizeOrNull();

        Assert.Null(result);
    }

    [Fact]
    public void NormalizeOrNull_WithValidString_ReturnsTrimmed()
    {
        string input = "  hello  ";

        var result = input.NormalizeOrNull();

        Assert.Equal("hello", result);
    }

    [Fact]
    public void NormalizeOrNull_WithStringContainingLeadingWhitespace_ReturnsTrimmed()
    {
        string input = "  test string";

        var result = input.NormalizeOrNull();

        Assert.Equal("test string", result);
    }

    [Fact]
    public void NormalizeOrNull_WithStringContainingTrailingWhitespace_ReturnsTrimmed()
    {
        string input = "test string  ";

        var result = input.NormalizeOrNull();

        Assert.Equal("test string", result);
    }

    [Fact]
    public void NormalizeOrNull_WithMultipleLinesAndWhitespace_ReturnsTrimmed()
    {
        string input = "  \n  multi  \n  line  \n  ";

        var result = input.NormalizeOrNull();

        Assert.NotNull(result);
        Assert.Equal("multi  \n  line", result);
    }
}
