namespace BookLoans.UnitTests.Controllers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Web.Controllers;
using BookLoans.Web.Models;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class AdminControllerTests
{
    [Fact]
    public void Index_RedirectsToCheckouts()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = controller.Index();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Checkouts), redirectResult.ActionName);
    }

    [Fact]
    public async Task Books_CallsBookService()
    {
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.GetBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.Books(CancellationToken.None);

        bookService.Verify(s => s.GetBooksAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Books_WithBooks_ReturnsView()
    {
        var bookService = new Mock<IAdminBookService>();
        var books = new List<Book> { new() { Id = 1, Title = "Test Book" } };
        bookService
            .Setup(s => s.GetBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.Books(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<BooksIndexViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task CreateBook_Get_CallsServiceForOptions()
    {
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.GetCreateFormAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Book() { Title = "" });
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.CreateBook(CancellationToken.None);

        bookService.Verify(s => s.GetCreateFormAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CreateBook_Post_WithSuccess_RedirectsToBooks()
    {
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.GetCreateFormAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Book() { Title = "" });
        bookService
            .Setup(s => s.CreateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);
        var model = new BookFormViewModel { Title = "Test Book" };

        var result = await controller.CreateBook(model, CancellationToken.None);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Books), redirectResult.ActionName);
    }

    [Fact]
    public async Task EditBook_Get_WithValidId_ReturnsView()
    {
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.GetEditFormAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Book() { Id = 1, Title = "Test Book" });
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.EditBook(1, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    [Fact]
    public async Task EditBook_Get_WithInvalidId_ReturnsNotFound()
    {
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.GetEditFormAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.EditBook(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteBook_WithSuccess_RedirectsToBooks()
    {
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.DeleteBook(1, CancellationToken.None);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Books), redirectResult.ActionName);
    }

    [Fact]
    public async Task Authors_CallsAuthorService()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        authorService
            .Setup(s => s.GetAuthorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Author>());
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.Authors(CancellationToken.None);

        authorService.Verify(s => s.GetAuthorsAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void CreateAuthor_Get_ReturnsView()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        authorService
            .Setup(s => s.GetCreateForm())
            .Returns(new Author());
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = controller.CreateAuthor();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    [Fact]
    public async Task CreateAuthor_Post_WithSuccess_RedirectsToAuthors()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        authorService
            .Setup(s => s.CreateAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);
        var model = new AuthorFormViewModel { Name = "New Author" };

        var result = await controller.CreateAuthor(model, CancellationToken.None);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Authors), redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteAuthor_WithSuccess_RedirectsToAuthors()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        authorService
            .Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.DeleteAuthor(1, CancellationToken.None);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Authors), redirectResult.ActionName);
    }

    [Fact]
    public async Task Borrowers_GetsBorrowersFromService()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        borrowerService
            .Setup(s => s.GetBorrowersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Borrower>());
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.Borrowers(CancellationToken.None);

        borrowerService.Verify(s => s.GetBorrowersAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void CreateBorrower_Get_ReturnsEmptyForm()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = controller.CreateBorrower();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<BorrowerFormViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task CreateBorrower_Post_WithSuccess_RedirectsToBorrowers()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        borrowerService
            .Setup(s => s.CreateAsync(It.IsAny<Borrower>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);
        var model = new BorrowerFormViewModel { FirstName = "John", LastName = "Doe" };

        var result = await controller.CreateBorrower(model, CancellationToken.None);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.Borrowers), redirectResult.ActionName);
    }

    [Fact]
    public void BulkImportBooks_Get_ReturnsView()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = controller.BulkImportBooks();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<BookImportViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task BulkImportBooks_Post_WithNullFile_ReturnsViewWithError()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.BulkImportBooks(null, CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task BulkImportBooks_Post_WithValidFile_ReturnsViewWithResult()
    {
        var importResult = new BookImportResult { SuccessCount = 2 };
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.ImportBooksFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(importResult);
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var csvBytes = System.Text.Encoding.UTF8.GetBytes("Title,Authors,Condition,YearFirstPublished\nBook One,Author A,New,2020");
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((s, _) => s.Write(csvBytes));

        var result = await controller.BulkImportBooks(formFile.Object, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BookImportViewModel>(viewResult.Model);
        Assert.NotNull(model.ImportResult);
        Assert.Equal(2, model.ImportResult.SuccessCount);
    }

    [Fact]
    public void BulkImportBooksTemplate_ReturnsFile()
    {
        var bookService = new Mock<IAdminBookService>();
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = controller.BulkImportBooksTemplate();

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Equal("books-import-template.csv", fileResult.FileDownloadName);
        var content = System.Text.Encoding.UTF8.GetString(fileResult.FileContents);
        Assert.Contains("Title", content);
        Assert.Contains("Authors", content);
    }

    [Fact]
    public async Task BulkExportBooks_ReturnsFile()
    {
        const string csv = "Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series\nBook A,Author One,,New,2020,,,,,";
        var bookService = new Mock<IAdminBookService>();
        bookService
            .Setup(s => s.ExportBooksToCsvAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(csv);
        var authorService = new Mock<IAdminAuthorService>();
        var borrowerService = new Mock<IAdminBorrowerService>();
        var conditionService = new Mock<IAdminConditionService>();
        var checkoutService = new Mock<IAdminCheckoutService>();
        var controller = new AdminController(bookService.Object, authorService.Object, borrowerService.Object, conditionService.Object, checkoutService.Object);

        var result = await controller.BulkExportBooks(CancellationToken.None);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Equal("books-export.csv", fileResult.FileDownloadName);
        Assert.Equal(csv, System.Text.Encoding.UTF8.GetString(fileResult.FileContents));
    }
}
