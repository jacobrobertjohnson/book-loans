namespace BookLoans.UnitTests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Services;
using Moq;
using Xunit;

public class AdminAuthorServiceTests
{
    [Fact]
    public async Task GetAuthorsAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminAuthorRepository>();
        var expectedAuthors = new List<Author>();
        mockRepository
            .Setup(r => r.GetAuthorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAuthors);
        var service = new AdminAuthorService(mockRepository.Object);

        var result = await service.GetAuthorsAsync(CancellationToken.None);

        mockRepository.Verify(r => r.GetAuthorsAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Same(expectedAuthors, result);
    }

    [Fact]
    public void GetCreateForm_ReturnsEmptyAuthor()
    {
        var mockRepository = new Mock<IAdminAuthorRepository>();
        var service = new AdminAuthorService(mockRepository.Object);

        var result = service.GetCreateForm();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetEditFormAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminAuthorRepository>();
        var expectedAuthor = new Author { Id = 1, Name = "Test Author" };
        mockRepository
            .Setup(r => r.GetEditFormAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAuthor);
        var service = new AdminAuthorService(mockRepository.Object);

        var result = await service.GetEditFormAsync(1, CancellationToken.None);

        Assert.Same(expectedAuthor, result);
    }

    [Fact]
    public async Task CreateAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminAuthorRepository>();
        mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminAuthorService(mockRepository.Object);
        var author = new Author { Name = "New Author" };

        var result = await service.CreateAsync(author, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.CreateAsync(author, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminAuthorRepository>();
        mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminAuthorService(mockRepository.Object);
        var author = new Author { Name = "Updated Author" };

        var result = await service.UpdateAsync(1, author, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.UpdateAsync(1, author, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminAuthorRepository>();
        mockRepository
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminAuthorService(mockRepository.Object);

        var result = await service.DeleteAsync(1, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class AdminBorrowerServiceTests
{
    [Fact]
    public async Task GetBorrowersAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBorrowerRepository>();
        var expectedBorrowers = new List<Borrower>();
        mockRepository
            .Setup(r => r.GetBorrowersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBorrowers);
        var service = new AdminBorrowerService(mockRepository.Object);

        var result = await service.GetBorrowersAsync(CancellationToken.None);

        mockRepository.Verify(r => r.GetBorrowersAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Same(expectedBorrowers, result);
    }

    [Fact]
    public async Task GetEditFormAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBorrowerRepository>();
        var expectedBorrower = new Borrower { Id = 1, FirstName = "John", LastName = "Doe" };
        mockRepository
            .Setup(r => r.GetEditFormAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBorrower);
        var service = new AdminBorrowerService(mockRepository.Object);

        var result = await service.GetEditFormAsync(1, CancellationToken.None);

        Assert.Same(expectedBorrower, result);
    }

    [Fact]
    public async Task CreateAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBorrowerRepository>();
        mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminBorrowerService(mockRepository.Object);
        var borrower = new Borrower { FirstName = "Jane", LastName = "Smith" };

        var result = await service.CreateAsync(borrower, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.CreateAsync(borrower, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBorrowerRepository>();
        mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminBorrowerService(mockRepository.Object);
        var borrower = new Borrower { FirstName = "Jane", LastName = "Doe" };

        var result = await service.UpdateAsync(1, borrower, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.UpdateAsync(1, borrower, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBorrowerRepository>();
        mockRepository
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminBorrowerService(mockRepository.Object);

        var result = await service.DeleteAsync(1, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class AdminConditionServiceTests
{
    [Fact]
    public async Task GetConditionsAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminConditionRepository>();
        var expectedConditions = new List<Condition>();
        mockRepository
            .Setup(r => r.GetConditionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedConditions);
        var service = new AdminConditionService(mockRepository.Object);

        var result = await service.GetConditionsAsync(CancellationToken.None);

        mockRepository.Verify(r => r.GetConditionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Same(expectedConditions, result);
    }

    [Fact]
    public void GetCreateForm_ReturnsEmptyCondition()
    {
        var mockRepository = new Mock<IAdminConditionRepository>();
        var service = new AdminConditionService(mockRepository.Object);

        var result = service.GetCreateForm();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetEditFormAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminConditionRepository>();
        var expectedCondition = new Condition { Id = 1, Name = "Good" };
        mockRepository
            .Setup(r => r.GetEditFormAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCondition);
        var service = new AdminConditionService(mockRepository.Object);

        var result = await service.GetEditFormAsync(1, CancellationToken.None);

        Assert.Same(expectedCondition, result);
    }

    [Fact]
    public async Task CreateAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminConditionRepository>();
        mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Condition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminConditionService(mockRepository.Object);
        var condition = new Condition { Name = "Excellent" };

        var result = await service.CreateAsync(condition, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.CreateAsync(condition, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminConditionRepository>();
        mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Condition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminConditionService(mockRepository.Object);
        var condition = new Condition { Name = "Fair" };

        var result = await service.UpdateAsync(1, condition, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.UpdateAsync(1, condition, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminConditionRepository>();
        mockRepository
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var service = new AdminConditionService(mockRepository.Object);

        var result = await service.DeleteAsync(1, CancellationToken.None);

        Assert.Null(result);
        mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class AdminBookServiceTests
{
    [Fact]
    public async Task GetBooksAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBookRepository>();
        var expectedBooks = new List<Book>();
        mockRepository
            .Setup(r => r.GetBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooks);
        var service = new AdminBookService(mockRepository.Object);

        var result = await service.GetBooksAsync(CancellationToken.None);

        mockRepository.Verify(r => r.GetBooksAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Same(expectedBooks, result);
    }

    [Fact]
    public async Task GetCreateFormAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBookRepository>();
        var expectedBook = new Book { Title = "" };
        mockRepository
            .Setup(r => r.GetCreateFormAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBook);
        var service = new AdminBookService(mockRepository.Object);

        var result = await service.GetCreateFormAsync(CancellationToken.None);

        Assert.Same(expectedBook, result);
    }

    [Fact]
    public async Task GetEditFormAsync_CallsRepository()
    {
        var mockRepository = new Mock<IAdminBookRepository>();
        var expectedBook = new Book { Id = 1, Title = "Test Book" };
        mockRepository
            .Setup(r => r.GetEditFormAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBook);
        var service = new AdminBookService(mockRepository.Object);

        var result = await service.GetEditFormAsync(1, CancellationToken.None);

        Assert.Same(expectedBook, result);
    }

    [Fact]
    public async Task ImportBooksFromCsvAsync_DelegatesToRepository()
    {
        var mockRepository = new Mock<IAdminBookRepository>();
        var expectedResult = new BookImportResult { SuccessCount = 3 };
        mockRepository
            .Setup(r => r.ImportBooksFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);
        var service = new AdminBookService(mockRepository.Object);
        using var stream = new MemoryStream();

        var result = await service.ImportBooksFromCsvAsync(stream, CancellationToken.None);

        mockRepository.Verify(r => r.ImportBooksFromCsvAsync(stream, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task ExportBooksToCsvAsync_DelegatesToRepository()
    {
        var mockRepository = new Mock<IAdminBookRepository>();
        const string expectedCsv = "Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series";
        mockRepository
            .Setup(r => r.ExportBooksToCsvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCsv);
        var service = new AdminBookService(mockRepository.Object);

        var result = await service.ExportBooksToCsvAsync(CancellationToken.None);

        mockRepository.Verify(r => r.ExportBooksToCsvAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(expectedCsv, result);
    }
}
