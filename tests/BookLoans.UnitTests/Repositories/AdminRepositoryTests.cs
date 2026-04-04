namespace BookLoans.UnitTests.Repositories;

using System;
using System.Collections.Generic;
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
}
