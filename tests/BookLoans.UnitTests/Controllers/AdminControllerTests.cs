namespace BookLoans.UnitTests.Controllers;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Web.Controllers;
using BookLoans.Web.Models;
using Moq;
using Xunit;
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
}
