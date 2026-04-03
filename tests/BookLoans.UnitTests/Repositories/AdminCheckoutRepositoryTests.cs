namespace BookLoans.UnitTests.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookLoans.Abstractions.Models;
using BookLoans.Data;
using BookLoans.Data.Entities;
using BookLoans.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AdminCheckoutRepositoryTests : IAsyncLifetime
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
    public async Task GetCheckoutsAsync_WithNoLoans_ReturnsEmptyList()
    {
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCheckoutsAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCheckoutsAsync_WithLoans_ReturnsOrderedByCheckoutDateDescending()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Test Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var loan1 = new BookLoanEntity { BookId = book.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = now.AddDays(-2) };
        var loan2 = new BookLoanEntity { BookId = book.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = now.AddDays(-1) };
        await _dbContext.BookLoans.AddRangeAsync(loan1, loan2);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCheckoutsAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].CheckedOutAtUtc >= result[1].CheckedOutAtUtc);
    }

    [Fact]
    public async Task GetCreateFormAsync_WithNoPreselection_ReturnFirstBookAndBorrower()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "First Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCreateFormAsync(null, null, CancellationToken.None);

        Assert.Equal(book.Id, result.BookId);
        Assert.Equal(borrower.Id, result.BorrowerId);
        Assert.Single(result.Books);
        Assert.Single(result.Borrowers);
    }

    [Fact]
    public async Task GetCreateFormAsync_WithValidBookPreselection_ReturnPreselectedBook()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book1 = new BookEntity { Title = "Book One", ConditionId = condition.Id };
        var book2 = new BookEntity { Title = "Book Two", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(book1, book2);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCreateFormAsync(book2.Id, null, CancellationToken.None);

        Assert.Equal(book2.Id, result.BookId);
    }

    [Fact]
    public async Task GetCreateFormAsync_WithInvalidBookPreselection_ReturnFirstBook()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCreateFormAsync(999, null, CancellationToken.None);

        Assert.Equal(book.Id, result.BookId);
    }

    [Fact]
    public async Task GetCreateFormAsync_WithValidBorrowerPreselection_ReturnPreselectedBorrower()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower1 = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        var borrower2 = new BorrowerEntity { FirstName = "Jane", LastName = "Smith" };
        await _dbContext.Borrowers.AddRangeAsync(borrower1, borrower2);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCreateFormAsync(null, borrower2.Id, CancellationToken.None);

        Assert.Equal(borrower2.Id, result.BorrowerId);
    }

    [Fact]
    public async Task GetCreateFormAsync_WithNoBooks_ReturnZeroBookId()
    {
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCreateFormAsync(null, null, CancellationToken.None);

        Assert.Equal(0, result.BookId);
    }

    [Fact]
    public async Task GetCreateFormAsync_WithNoBorrowers_ReturnZeroBorrowerId()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.GetCreateFormAsync(null, null, CancellationToken.None);

        Assert.Equal(0, result.BorrowerId);
    }

    [Fact]
    public async Task RebuildFormAsync_PreservesBookAndBorrowerId()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book1 = new BookEntity { Title = "Book One", ConditionId = condition.Id };
        var book2 = new BookEntity { Title = "Book Two", ConditionId = condition.Id };
        await _dbContext.Books.AddRangeAsync(book1, book2);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var inputModel = new BookLoan { BookId = book2.Id, BorrowerId = borrower.Id };
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.RebuildFormAsync(inputModel, CancellationToken.None);

        Assert.Equal(book2.Id, result.BookId);
        Assert.Equal(borrower.Id, result.BorrowerId);
    }

    [Fact]
    public async Task RebuildFormAsync_RefreshesBookAndBorrowerLists()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var inputModel = new BookLoan { BookId = book.Id, BorrowerId = borrower.Id };
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.RebuildFormAsync(inputModel, CancellationToken.None);

        Assert.NotNull(result.Books);
        Assert.NotNull(result.Borrowers);
        Assert.Single(result.Books);
        Assert.Single(result.Borrowers);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidBook_ReturnsErrorMessage()
    {
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var model = new BookLoan { BookId = 999, BorrowerId = borrower.Id };
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.CreateAsync(model, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Please select a valid book.", result);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidBorrower_ReturnsErrorMessage()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        await _dbContext.SaveChangesAsync();

        var model = new BookLoan { BookId = book.Id, BorrowerId = 999 };
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.CreateAsync(model, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Please select a valid borrower.", result);
    }

    [Fact]
    public async Task CreateAsync_WithAlreadyCheckedOutBook_ReturnsErrorMessage()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var existingLoan = new BookLoanEntity { BookId = book.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = DateTime.UtcNow };
        await _dbContext.BookLoans.AddAsync(existingLoan);
        await _dbContext.SaveChangesAsync();

        var model = new BookLoan { BookId = book.Id, BorrowerId = borrower.Id };
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.CreateAsync(model, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("This book is already checked out.", result);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsNull()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var model = new BookLoan { BookId = book.Id, BorrowerId = borrower.Id };
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.CreateAsync(model, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_UpdatesBookCurrentBorrower()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var model = new BookLoan { BookId = book.Id, BorrowerId = borrower.Id };
        var repository = new AdminCheckoutRepository(_dbContext);

        await repository.CreateAsync(model, CancellationToken.None);

        var updatedBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
        Assert.NotNull(updatedBook);
        Assert.Equal(borrower.Id, updatedBook.CurrentBorrowerId);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_AddsDatabaseLoan()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var model = new BookLoan { BookId = book.Id, BorrowerId = borrower.Id };
        var repository = new AdminCheckoutRepository(_dbContext);

        await repository.CreateAsync(model, CancellationToken.None);

        var loan = await _dbContext.BookLoans.FirstOrDefaultAsync(l => l.BookId == book.Id);
        Assert.NotNull(loan);
        Assert.Equal(book.Id, loan.BookId);
        Assert.Equal(borrower.Id, loan.BorrowerId);
    }

    [Fact]
    public async Task ReturnAsync_WithInvalidLoan_ReturnsErrorMessage()
    {
        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.ReturnAsync(999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Checkout not found.", result);
    }

    [Fact]
    public async Task ReturnAsync_WithAlreadyReturnedLoan_ReturnsErrorMessage()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var loan = new BookLoanEntity
        {
            BookId = book.Id,
            BorrowerId = borrower.Id,
            CheckedOutAtUtc = DateTime.UtcNow.AddDays(-1),
            ReturnedAtUtc = DateTime.UtcNow
        };
        await _dbContext.BookLoans.AddAsync(loan);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.ReturnAsync(loan.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("This checkout has already been returned.", result);
    }

    [Fact]
    public async Task ReturnAsync_WithValidLoan_ReturnsNull()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var loan = new BookLoanEntity { BookId = book.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = DateTime.UtcNow };
        await _dbContext.BookLoans.AddAsync(loan);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        var result = await repository.ReturnAsync(loan.Id, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnAsync_WithValidLoan_SetsReturnedAtUtc()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var loan = new BookLoanEntity { BookId = book.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = DateTime.UtcNow.AddDays(-1) };
        await _dbContext.BookLoans.AddAsync(loan);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        await repository.ReturnAsync(loan.Id, CancellationToken.None);

        var returnedLoan = await _dbContext.BookLoans.FirstOrDefaultAsync(l => l.Id == loan.Id);
        Assert.NotNull(returnedLoan);
        Assert.NotNull(returnedLoan.ReturnedAtUtc);
    }

    [Fact]
    public async Task ReturnAsync_WithValidLoan_ClearsBookCurrentBorrower()
    {
        var condition = new ConditionEntity { Name = "Good" };
        await _dbContext.Conditions.AddAsync(condition);
        await _dbContext.SaveChangesAsync();

        var book = new BookEntity { Title = "Book", ConditionId = condition.Id, CurrentBorrowerId = 1 };
        await _dbContext.Books.AddAsync(book);
        var borrower = new BorrowerEntity { FirstName = "John", LastName = "Doe" };
        await _dbContext.Borrowers.AddAsync(borrower);
        await _dbContext.SaveChangesAsync();

        var loan = new BookLoanEntity { BookId = book.Id, BorrowerId = borrower.Id, CheckedOutAtUtc = DateTime.UtcNow };
        await _dbContext.BookLoans.AddAsync(loan);
        await _dbContext.SaveChangesAsync();

        var repository = new AdminCheckoutRepository(_dbContext);

        await repository.ReturnAsync(loan.Id, CancellationToken.None);

        var returnedBook = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
        Assert.NotNull(returnedBook);
        Assert.Null(returnedBook.CurrentBorrowerId);
    }
}
