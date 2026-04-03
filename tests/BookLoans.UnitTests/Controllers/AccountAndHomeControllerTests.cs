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

public class HomeControllerTests
{
    [Fact]
    public async Task Index_CallsQueryService()
    {
        var mockService = new Mock<IPublicHomepageQueryService>();
        var groups = new List<BorrowerCheckoutGroup>
        {
            new() { BorrowerFullName = "John Doe", Books = new List<BookLoan>() }
        };
        mockService
            .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);
        var controller = new HomeController(mockService.Object);

        var result = await controller.Index(CancellationToken.None);

        mockService.Verify(s => s.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Index_WithEmptyCheckouts_ReturnsView()
    {
        var mockService = new Mock<IPublicHomepageQueryService>();
        mockService
            .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BorrowerCheckoutGroup>());
        var controller = new HomeController(mockService.Object);

        var result = await controller.Index(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public async Task Index_WithCheckouts_PassesModelToView()
    {
        var mockService = new Mock<IPublicHomepageQueryService>();
        var groups = new List<BorrowerCheckoutGroup>
        {
            new() { BorrowerFullName = "John Doe", Books = new List<BookLoan>() }
        };
        mockService
            .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);
        var controller = new HomeController(mockService.Object);

        var result = await controller.Index(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<HomepageViewModel>(viewResult.Model);
    }

}


