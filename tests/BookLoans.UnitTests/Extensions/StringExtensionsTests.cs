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

    [Theory]
    [InlineData("Beowulf", "Beowulf")]
    [InlineData("The Martian", "Martian")]
    [InlineData("the martian", "martian")]
    [InlineData("An American Werewolf", "American Werewolf")]
    [InlineData("A Room with a View", "Room with a View")]
    [InlineData("Anchor", "Anchor")]
    [InlineData("There Will Be Blood", "There Will Be Blood")]
    public void NormalizeForSort_StripsLeadingArticle(string input, string expected)
    {
        Assert.Equal(expected, input.NormalizeForSort());
    }
}
