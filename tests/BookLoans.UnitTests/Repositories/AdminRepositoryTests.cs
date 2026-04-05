namespace BookLoans.UnitTests.Repositories;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookLoans.Abstractions.Models;
using BookLoans.Data;
using BookLoans.Data.Entities;
using BookLoans.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AdminAuthorRepositoryTests : IAsyncLifetime
{
    private DbContextOptions<AppDbContext> _options = null!;
    private AppDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(_options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetAuthorsAsync_WithNoAuthors_ReturnsEmptyList()
    {
        var repository = new AdminAuthorRepository(_dbContext);

        var result = await repository.GetAuthorsAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAuthorsAsync_WithAuthors_ReturnsOrderedByName()
    {
        await _dbContext.Authors.AddRangeAsync(
            new AuthorEntity { Name = "Zara" },
            new AuthorEntity { Name = "Alice" },
            new AuthorEntity { Name = "Bob" }
        );
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);

        var result = await repository.GetAuthorsAsync(CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal("Bob", result[1].Name);
        Assert.Equal("Zara", result[2].Name);
    }

    [Fact]
    public async Task GetEditFormAsync_WithValidId_ReturnsAuthor()
    {
        var author = new AuthorEntity { Name = "Test Author" };
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);

        var result = await repository.GetEditFormAsync(author.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Test Author", result.Name);
    }

    [Fact]
    public async Task GetEditFormAsync_WithInvalidId_ReturnsNull()
    {
        var repository = new AdminAuthorRepository(_dbContext);

        var result = await repository.GetEditFormAsync(999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_ReturnsNull()
    {
        var repository = new AdminAuthorRepository(_dbContext);
        var author = new Author { Name = "New Author" };

        var result = await repository.CreateAsync(author, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_AddsToDB()
    {
        var repository = new AdminAuthorRepository(_dbContext);
        var author = new Author { Name = "New Author" };

        await repository.CreateAsync(author, CancellationToken.None);

        var added = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Name == "New Author");
        Assert.NotNull(added);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsError()
    {
        var repository = new AdminAuthorRepository(_dbContext);
        var author = new Author { Name = "  " };

        var result = await repository.CreateAsync(author, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("required", result);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsError()
    {
        await _dbContext.Authors.AddAsync(new AuthorEntity { Name = "Existing" });
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);
        var author = new Author { Name = "Existing" };

        var result = await repository.CreateAsync(author, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("already exists", result);
    }

    [Fact]
    public async Task CreateAsync_WithWhitespace_Trims()
    {
        var repository = new AdminAuthorRepository(_dbContext);
        var author = new Author { Name = "  Test Author  " };

        await repository.CreateAsync(author, CancellationToken.None);

        var added = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Name == "Test Author");
        Assert.NotNull(added);
    }

    [Fact]
    public async Task UpdateAsync_WithValidName_ReturnsNull()
    {
        var author = new AuthorEntity { Name = "Original" };
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);
        var updated = new Author { Name = "Updated" };

        var result = await repository.UpdateAsync(author.Id, updated, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidName_UpdatesDB()
    {
        var author = new AuthorEntity { Name = "Original" };
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);
        var updated = new Author { Name = "Updated" };

        await repository.UpdateAsync(author.Id, updated, CancellationToken.None);

        var foundAuthor = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Id == author.Id);
        Assert.NotNull(foundAuthor);
        Assert.Equal("Updated", foundAuthor.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentId_ReturnsError()
    {
        var repository = new AdminAuthorRepository(_dbContext);
        var author = new Author { Name = "Test" };

        var result = await repository.UpdateAsync(999, author, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("not found", result);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyName_ReturnsError()
    {
        var author = new AuthorEntity { Name = "Original" };
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);
        var updated = new Author { Name = "  " };

        var result = await repository.UpdateAsync(author.Id, updated, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("required", result);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsError()
    {
        var author1 = new AuthorEntity { Name = "Author One" };
        var author2 = new AuthorEntity { Name = "Author Two" };
        await _dbContext.Authors.AddRangeAsync(author1, author2);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);
        var updated = new Author { Name = "Author One" };

        var result = await repository.UpdateAsync(author2.Id, updated, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("already exists", result);
    }

    [Fact]
    public async Task UpdateAsync_AllowsSameName()
    {
        var author = new AuthorEntity { Name = "Test Author" };
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminAuthorRepository(_dbContext);
        var updated = new Author { Name = "Test Author" };

        var result = await repository.UpdateAsync(author.Id, updated, CancellationToken.None);

        Assert.Null(result);
    }
}

public class AdminBorrowerRepositoryTests : IAsyncLifetime
{
    private DbContextOptions<AppDbContext> _options = null!;
    private AppDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(_options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetBorrowersAsync_WithNoBorrowers_ReturnsEmptyList()
    {
        var repository = new AdminBorrowerRepository(_dbContext);

        var result = await repository.GetBorrowersAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBorrowersAsync_WithBorrowers_ReturnsOrderedByLastThenFirst()
    {
        await _dbContext.Borrowers.AddRangeAsync(
            new BorrowerEntity { FirstName = "Alice", LastName = "Smith" },
            new BorrowerEntity { FirstName = "Bob", LastName = "Adams" },
            new BorrowerEntity { FirstName = "Charlie", LastName = "Adams" }
        );
        await _dbContext.SaveChangesAsync();

        var repository = new AdminBorrowerRepository(_dbContext);

        var result = await repository.GetBorrowersAsync(CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("Adams", result[0].LastName);
        Assert.Equal("Bob", result[0].FirstName);
    }

    [Fact]
    public async Task CreateAsync_WithValidNames_ReturnsNull()
    {
        var repository = new AdminBorrowerRepository(_dbContext);
        var borrower = new Borrower { FirstName = "John", LastName = "Doe" };

        var result = await repository.CreateAsync(borrower, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyFirstName_ReturnsError()
    {
        var repository = new AdminBorrowerRepository(_dbContext);
        var borrower = new Borrower { FirstName = "  ", LastName = "Doe" };

        var result = await repository.CreateAsync(borrower, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("required", result);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyLastName_ReturnsError()
    {
        var repository = new AdminBorrowerRepository(_dbContext);
        var borrower = new Borrower { FirstName = "John", LastName = "  " };

        var result = await repository.CreateAsync(borrower, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("required", result);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsError()
    {
        await _dbContext.Borrowers.AddAsync(new BorrowerEntity { FirstName = "John", LastName = "Doe" });
        await _dbContext.SaveChangesAsync();

        var repository = new AdminBorrowerRepository(_dbContext);
        var borrower = new Borrower { FirstName = "John", LastName = "Doe" };

        var result = await repository.CreateAsync(borrower, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("already exists", result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidNames_ReturnsNull()
    {
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminBorrowerRepository(_dbContext);
        var updated = new Borrower { FirstName = "Jane", LastName = "Smith" };

        var result = await repository.UpdateAsync(borrower.Id, updated, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentId_ReturnsError()
    {
        var repository = new AdminBorrowerRepository(_dbContext);
        var borrower = new Borrower { FirstName = "John", LastName = "Doe" };

        var result = await repository.UpdateAsync(999, borrower, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("not found", result);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsError()
    {
        var b1 = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        var b2 = new BorrowerEntity { FirstName = "Jane", LastName = "Smith" };
        await _dbContext.Borrowers.AddRangeAsync(b1, b2);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminBorrowerRepository(_dbContext);
        var updated = new Borrower { FirstName = "John", LastName = "Doe" };

        var result = await repository.UpdateAsync(b2.Id, updated, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("already exists", result);
    }
}

public class AdminConditionRepositoryTests : IAsyncLifetime
{
    private DbContextOptions<AppDbContext> _options = null!;
    private AppDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(_options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetConditionsAsync_WithNoConditions_ReturnsSeededList()
    {
        var repository = new AdminConditionRepository(_dbContext);

        var result = await repository.GetConditionsAsync(CancellationToken.None);

        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.Name == "New");
        Assert.Contains(result, c => c.Name == "Used");
    }

    [Fact]
    public async Task GetConditionsAsync_WithConditions_ReturnsOrderedByName()
    {
        await _dbContext.Conditions.AddRangeAsync(
            new ConditionEntity { Name = "Poor" },
            new ConditionEntity { Name = "Excellent" },
            new ConditionEntity { Name = "Good" }
        );
        await _dbContext.SaveChangesAsync();

        var repository = new AdminConditionRepository(_dbContext);

        var result = await repository.GetConditionsAsync(CancellationToken.None);

        Assert.NotEmpty(result);
        var allNames = result.Select(r => r.Name).ToList();
        Assert.Equal(allNames.OrderBy(n => n), allNames);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_ReturnsNull()
    {
        var repository = new AdminConditionRepository(_dbContext);
        var condition = new Condition { Name = "Very Good" };

        var result = await repository.CreateAsync(condition, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsError()
    {
        var repository = new AdminConditionRepository(_dbContext);
        var condition = new Condition { Name = "  " };

        var result = await repository.CreateAsync(condition, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("required", result);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsError()
    {
        await _dbContext.Conditions.AddAsync(new ConditionEntity { Name = "Good" });
        await _dbContext.SaveChangesAsync();

        var repository = new AdminConditionRepository(_dbContext);
        var condition = new Condition { Name = "Good" };

        var result = await repository.CreateAsync(condition, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("already exists", result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidName_ReturnsNull()
    {
        var condition = new ConditionEntity { Name = "Poor" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminConditionRepository(_dbContext);
        var updated = new Condition { Name = "Very Poor" };

        var result = await repository.UpdateAsync(condition.Id, updated, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentId_ReturnsError()
    {
        var repository = new AdminConditionRepository(_dbContext);
        var condition = new Condition { Name = "Test" };

        var result = await repository.UpdateAsync(999, condition, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("not found", result);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsError()
    {
        var c1 = new ConditionEntity { Name = "Good" };
        var c2 = new ConditionEntity { Name = "Poor" };
        await _dbContext.Conditions.AddRangeAsync(c1, c2);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminConditionRepository(_dbContext);
        var updated = new Condition { Name = "Good" };

        var result = await repository.UpdateAsync(c2.Id, updated, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("already exists", result);
    }
}

public class PublicHomepageRepositoryTests : IAsyncLifetime
{
    private DbContextOptions<AppDbContext> _options = null!;
    private AppDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(_options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetAsync_WithNoActiveCheckouts_ReturnsEmptyList()
    {
        var repository = new PublicHomepageRepository(_dbContext);

        var result = await repository.GetAsync(CancellationToken.None);

        Assert.Empty(result.BorrowerGroups);
        Assert.Empty(result.AvailableBooks);
    }

    [Fact]
    public async Task GetAsync_WithReturnedCheckouts_ReturnsEmpty()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var loan = new BookLoanEntity
        {
            BookId = book.Id,
            BorrowerId = borrower.Id,
            CheckedOutAtUtc = DateTime.UtcNow,
            ReturnedAtUtc = DateTime.UtcNow.AddDays(1)
        };
        await _dbContext.BookLoans.AddAsync(loan);
        await _dbContext.SaveChangesAsync();

        var repository = new PublicHomepageRepository(_dbContext);

        var result = await repository.GetAsync(CancellationToken.None);

        Assert.Empty(result.BorrowerGroups);
        Assert.Single(result.AvailableBooks);
        Assert.Equal("Book", result.AvailableBooks[0].Title);
    }

    [Fact]
    public async Task GetAsync_WithActiveCheckouts_ReturnsGroupedByBorrower()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        var book1 = new BookEntity { Title = "Book1", ConditionId = condition.Id };
        var book2 = new BookEntity { Title = "Book2", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(book1, book2);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var loan1 = new BookLoanEntity { BookId = book1.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = DateTime.UtcNow };
        var loan2 = new BookLoanEntity { BookId = book2.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = DateTime.UtcNow.AddHours(1) };
        await _dbContext.BookLoans.AddRangeAsync(loan1, loan2);
        await _dbContext.SaveChangesAsync();

        var repository = new PublicHomepageRepository(_dbContext);

        var result = await repository.GetAsync(CancellationToken.None);

        Assert.Single(result.BorrowerGroups);
        Assert.Equal("John Doe", result.BorrowerGroups[0].BorrowerFullName);
        Assert.Equal(2, result.BorrowerGroups[0].Books.Count);
        Assert.Empty(result.AvailableBooks);
    }

    [Fact]
    public async Task GetAsync_WithMultipleBorrowers_ReturnsMultipleGroups()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        var book1 = new BookEntity { Title = "Book1", ConditionId = condition.Id };
        var book2 = new BookEntity { Title = "Book2", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(book1, book2);
        var b1 = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        var b2 = new BorrowerEntity { FirstName = "Jane", LastName = "Smith" };
        await _dbContext.Borrowers.AddRangeAsync(b1, b2);
        await _dbContext.SaveChangesAsync();

        var loan1 = new BookLoanEntity { BookId = book1.Id, BorrowerId = b1.Id, CheckedOutAtUtc = DateTime.UtcNow };
        var loan2 = new BookLoanEntity { BookId = book2.Id, BorrowerId = b2.Id, CheckedOutAtUtc = DateTime.UtcNow.AddHours(1) };
        await _dbContext.BookLoans.AddRangeAsync(loan1, loan2);
        await _dbContext.SaveChangesAsync();

        var repository = new PublicHomepageRepository(_dbContext);

        var result = await repository.GetAsync(CancellationToken.None);

        Assert.Equal(2, result.BorrowerGroups.Count);
        Assert.Empty(result.AvailableBooks);
    }

    [Fact]
    public async Task GetAsync_OrdersBooksWithinGroupByCheckoutDate()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        var book1 = new BookEntity { Title = "Book1", ConditionId = condition.Id };
        var book2 = new BookEntity { Title = "Book2", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(book1, book2);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var loan1 = new BookLoanEntity { BookId = book1.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = now.AddDays(-2) };
        var loan2 = new BookLoanEntity { BookId = book2.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = now.AddDays(-1) };
        await _dbContext.BookLoans.AddRangeAsync(loan1, loan2);
        await _dbContext.SaveChangesAsync();

        var repository = new PublicHomepageRepository(_dbContext);

        var result = await repository.GetAsync(CancellationToken.None);

        Assert.Single(result.BorrowerGroups);
        Assert.Equal(2, result.BorrowerGroups[0].Books.Count);
        Assert.True(result.BorrowerGroups[0].Books[0].CheckedOutAtUtc <= result.BorrowerGroups[0].Books[1].CheckedOutAtUtc);
    }

    [Fact]
    public async Task GetAsync_ReturnsAvailableBooksOrderedByTitle()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        var zebraBook = new BookEntity { Title = "Zebra Tales", ConditionId = condition.Id };
        var alphaBook = new BookEntity { Title = "Alpha Guide", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(zebraBook, alphaBook);
        await _dbContext.SaveChangesAsync();

        var repository = new PublicHomepageRepository(_dbContext);

        var result = await repository.GetAsync(CancellationToken.None);

        Assert.Equal(2, result.AvailableBooks.Count);
        Assert.Equal("Alpha Guide", result.AvailableBooks[0].Title);
        Assert.Equal("Zebra Tales", result.AvailableBooks[1].Title);
        Assert.Empty(result.BorrowerGroups);
    }

    [Fact]
    public async Task GetAsync_AvailableBooks_SortsBySeriesOrTitleWithArticleStripping()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);

        var narniaSeries = new SeriesEntity { Name = "The Chronicles of Narnia" };
        var lotrSeries = new SeriesEntity { Name = "Lord of the Rings" };
        await _dbContext.Series.AddRangeAsync(narniaSeries, lotrSeries);
        await _dbContext.SaveChangesAsync();

        var beowulf = new BookEntity { Title = "Beowulf", ConditionId = condition.Id };
        var silverChair = new BookEntity { Title = "The Silver Chair", ConditionId = condition.Id, SeriesId = narniaSeries.Id };
        var fellowship = new BookEntity { Title = "The Fellowship of the Ring", ConditionId = condition.Id, SeriesId = lotrSeries.Id };
        var theMartian = new BookEntity { Title = "The Martian", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(beowulf, silverChair, fellowship, theMartian);
        await _dbContext.SaveChangesAsync();

        var repository = new PublicHomepageRepository(_dbContext);

        var result = await repository.GetAsync(CancellationToken.None);

        // Sort keys: "Beowulf", "Chronicles of Narnia", "Lord of the Rings", "Martian"
        Assert.Equal(4, result.AvailableBooks.Count);
        Assert.Equal("Beowulf", result.AvailableBooks[0].Title);
        Assert.Equal("The Silver Chair", result.AvailableBooks[1].Title);
        Assert.Equal("The Fellowship of the Ring", result.AvailableBooks[2].Title);
        Assert.Equal("The Martian", result.AvailableBooks[3].Title);
    }
}

public class AdminBookRepositoryTests : IAsyncLifetime
{
    private DbContextOptions<AppDbContext> _options = null!;
    private AppDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(_options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetBooksAsync_SortsBySeriesOrTitleWithArticleStripping()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);

        var narniaSeries = new SeriesEntity { Name = "The Chronicles of Narnia" };
        var lotrSeries = new SeriesEntity { Name = "Lord of the Rings" };
        await _dbContext.Series.AddRangeAsync(narniaSeries, lotrSeries);
        await _dbContext.SaveChangesAsync();

        var beowulf = new BookEntity { Title = "Beowulf", ConditionId = condition.Id };
        var silverChair = new BookEntity { Title = "The Silver Chair", ConditionId = condition.Id, SeriesId = narniaSeries.Id };
        var fellowship = new BookEntity { Title = "The Fellowship of the Ring", ConditionId = condition.Id, SeriesId = lotrSeries.Id };
        var theMartian = new BookEntity { Title = "The Martian", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(beowulf, silverChair, fellowship, theMartian);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminBookRepository(_dbContext);

        var result = await repository.GetBooksAsync(CancellationToken.None);

        // Sort keys: "Beowulf", "Chronicles of Narnia", "Lord of the Rings", "Martian"
        Assert.Equal(4, result.Count);
        Assert.Equal("Beowulf", result[0].Title);
        Assert.Equal("The Silver Chair", result[1].Title);
        Assert.Equal("The Fellowship of the Ring", result[2].Title);
        Assert.Equal("The Martian", result[3].Title);
    }

    [Fact]
    public async Task DeleteAsync_WithCheckoutHistory_RemovesBookAndLoans()
    {
        var condition = new ConditionEntity { Name = "Good" };
        var borrower = new BorrowerEntity { FirstName = "Test", LastName = "Borrower" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Delete Me", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        var loan = new BookLoanEntity
        {
            BookId = book.Id,
            BorrowerId = borrower.Id,
            CheckedOutAtUtc = DateTime.UtcNow.AddDays(-3),
            ReturnedAtUtc = DateTime.UtcNow.AddDays(-1)
        };
        await _dbContext.BookLoans.AddAsync(loan);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminBookRepository(_dbContext);

        var result = await repository.DeleteAsync(book.Id, CancellationToken.None);

        Assert.Null(result);
        Assert.False(await _dbContext.Books.AnyAsync(entity => entity.Id == book.Id));
        Assert.False(await _dbContext.BookLoans.AnyAsync(entity => entity.BookId == book.Id));
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithHeaderOnly_ReturnsEmptyResult()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream("Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(0, result.SuccessCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithEmptyStream_ReturnsEmptyResult()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(string.Empty);

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(0, result.SuccessCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithMissingRequiredColumns_ReturnsError()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream("Title,ISBN\nMy Book,123");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(0, result.SuccessCount);
        Assert.Single(result.Errors);
        Assert.Contains("required columns", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithValidRow_ImportsBook()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(
            "Title,Authors,ISBN,Condition,YearFirstPublished",
            "The Great Gatsby,F. Scott Fitzgerald,,New,1925");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(1, result.SuccessCount);
        Assert.Empty(result.Errors);
        Assert.True(await _dbContext.Books.AnyAsync(b => b.Title == "The Great Gatsby"));
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithPipeSeparatedAuthors_CreatesAllAuthors()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(
            "Title,Authors,ISBN,Condition,YearFirstPublished",
            "Co-authored Book,Alice Smith|Bob Jones,,New,2020");

        await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.True(await _dbContext.Authors.AnyAsync(a => a.Name == "Alice Smith"));
        Assert.True(await _dbContext.Authors.AnyAsync(a => a.Name == "Bob Jones"));
        var book = await _dbContext.Books.FirstAsync(b => b.Title == "Co-authored Book");
        Assert.Equal(2, await _dbContext.BookAuthors.CountAsync(ba => ba.BookId == book.Id));
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithUnknownCondition_ReportsError()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(
            "Title,Authors,ISBN,Condition,YearFirstPublished",
            "Bad Book,Some Author,,NonExistentCondition,2020");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(0, result.SuccessCount);
        Assert.Single(result.Errors);
        Assert.Contains("NonExistentCondition", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithInvalidYear_ReportsError()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(
            "Title,Authors,ISBN,Condition,YearFirstPublished",
            "Bad Year Book,Some Author,,New,notAYear");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(0, result.SuccessCount);
        Assert.Single(result.Errors);
        Assert.Contains("YearFirstPublished", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_PartialSuccess_ReportsCorrectCounts()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(
            "Title,Authors,ISBN,Condition,YearFirstPublished",
            "Good Book,Author One,,New,2000",
            "Bad Book,Author Two,,InvalidCondition,2001",
            "Another Good Book,Author Three,,Used,2002");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(2, result.SuccessCount);
        Assert.Single(result.Errors);
        Assert.Equal(3, result.Errors[0].RowNumber);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_WithOptionalFields_ImportsCorrectly()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(
            "Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series",
            "Full Book,Author One,978-0000000001,New,1990,2nd,2000,2000-01-15,Library Store,My Series");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(1, result.SuccessCount);
        Assert.Empty(result.Errors);
        var book = await _dbContext.Books.FirstAsync(b => b.Title == "Full Book");
        Assert.Equal("978-0000000001", book.Isbn);
        Assert.Equal("2nd", book.Edition);
        Assert.Equal(2000, book.YearEditionPublished);
        Assert.Equal(new DateOnly(2000, 1, 15), book.DateOfPurchase);
        Assert.Equal("Library Store", book.LocationOfPurchase);
        Assert.NotNull(book.SeriesId);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_SkipsBlankRows()
    {
        var repository = new AdminBookRepository(_dbContext);
        using var stream = MakeCsvStream(
            "Title,Authors,ISBN,Condition,YearFirstPublished",
            "Real Book,Some Author,,New,2020",
            "   ",
            "Another Real Book,Other Author,,Used,2021");

        var result = await repository.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        Assert.Equal(2, result.SuccessCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExportBooksToCsvAsync_WithNoBooks_ReturnsHeaderOnly()
    {
        var repository = new AdminBookRepository(_dbContext);

        var result = await repository.ExportBooksToCsvAsync(CancellationToken.None);

        Assert.Equal("Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series", result);
    }

    [Fact]
    public async Task ExportBooksToCsvAsync_UsesImportCompatibleFormat()
    {
        var condition = await _dbContext.Conditions.FirstAsync(c => c.Name == "New");
        var authorA = new AuthorEntity { Name = "Author One" };
        var authorB = new AuthorEntity { Name = "Author Two" };
        var series = new SeriesEntity { Name = "Saga, \"Special\"" };
        await _dbContext.Authors.AddRangeAsync(authorA, authorB);
        await _dbContext.Series.AddAsync(series);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity
        {
            Title = "Book, \"Quoted\"",
            ConditionId = condition.Id,
            YearFirstPublished = 2021,
            Isbn = "978-1",
            Edition = "First",
            YearEditionPublished = 2022,
            DateOfPurchase = new DateOnly(2023, 4, 5),
            LocationOfPurchase = "Main, Branch",
            SeriesId = series.Id
        };
        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        await _dbContext.BookAuthors.AddRangeAsync(
            new BookAuthorEntity { BookId = book.Id, AuthorId = authorA.Id },
            new BookAuthorEntity { BookId = book.Id, AuthorId = authorB.Id });
        await _dbContext.SaveChangesAsync();

        var repository = new AdminBookRepository(_dbContext);

        var result = await repository.ExportBooksToCsvAsync(CancellationToken.None);

        Assert.Contains("Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series", result);
        Assert.Contains("\"Book, \"\"Quoted\"\"\"", result);
        Assert.Contains("Author One|Author Two", result);
        Assert.Contains(",New,2021,First,2022,2023-04-05,", result);
        Assert.Contains("\"Main, Branch\"", result);
        Assert.Contains("\"Saga, \"\"Special\"\"\"", result);
    }

    private static Stream MakeCsvStream(params string[] lines)
    {
        string content = string.Join('\n', lines);
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
    }
}
