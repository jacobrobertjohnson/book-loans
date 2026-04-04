namespace BookLoans.UnitTests.Controllers;

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Data.Entities;
using BookLoans.Web.Controllers;
using BookLoans.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_CallsQueryService()
    {
        var mockService = new Mock<IPublicHomepageQueryService>();
        var homepageData = new HomepageData
        {
            BorrowerGroups = new List<BorrowerCheckoutGroup>
            {
                new() { BorrowerFullName = "John Doe", Books = new List<BookLoan>() }
            }
        };
        mockService
            .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(homepageData);
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
            .ReturnsAsync(new HomepageData());
        var controller = new HomeController(mockService.Object);

        var result = await controller.Index(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public async Task Index_WithCheckouts_PassesModelToView()
    {
        var mockService = new Mock<IPublicHomepageQueryService>();
        var homepageData = new HomepageData
        {
            BorrowerGroups = new List<BorrowerCheckoutGroup>
            {
                new() { BorrowerFullName = "John Doe", Books = new List<BookLoan>() }
            }
        };
        mockService
            .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(homepageData);
        var controller = new HomeController(mockService.Object);

        var result = await controller.Index(CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsType<HomepageViewModel>(viewResult.Model);
    }
}

public class AccountControllerTests
{
    private static (Mock<SignInManager<ApplicationUserEntity>>, Mock<UserManager<ApplicationUserEntity>>) CreateMocks()
    {
        var userStore = new Mock<IUserStore<ApplicationUserEntity>>();
        var userManager = new Mock<UserManager<ApplicationUserEntity>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        var userClaimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUserEntity>>();
        var signInManager = new Mock<SignInManager<ApplicationUserEntity>>(
            userManager.Object, httpContextAccessor.Object, userClaimsFactory.Object,
            null!, null!, null!, null!);

        return (signInManager, userManager);
    }

    private static AccountController CreateController(
        Mock<SignInManager<ApplicationUserEntity>> signInManager,
        Mock<UserManager<ApplicationUserEntity>> userManager)
    {
        var controller = new AccountController(signInManager.Object, userManager.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
        return controller;
    }

    [Fact]
    public void ChangePassword_Get_ReturnsView()
    {
        var (signInManager, userManager) = CreateMocks();
        var controller = CreateController(signInManager, userManager);

        var result = controller.ChangePassword();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task ChangePassword_Post_InvalidModel_ReturnsView()
    {
        var (signInManager, userManager) = CreateMocks();
        var controller = CreateController(signInManager, userManager);
        controller.ModelState.AddModelError("NewPassword", "Required");
        var model = new ChangePasswordViewModel();

        var result = await controller.ChangePassword(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task ChangePassword_Post_UserNotFound_RedirectsToLogin()
    {
        var (signInManager, userManager) = CreateMocks();
        userManager
            .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUserEntity?)null);
        var controller = CreateController(signInManager, userManager);
        var model = new ChangePasswordViewModel
        {
            CurrentPassword = "Old1!",
            NewPassword = "New1!",
            ConfirmNewPassword = "New1!"
        };

        var result = await controller.ChangePassword(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.Login), redirect.ActionName);
    }

    [Fact]
    public async Task ChangePassword_Post_ChangePasswordFails_ReturnsViewWithErrors()
    {
        var (signInManager, userManager) = CreateMocks();
        var user = new ApplicationUserEntity { UserName = "admin@bookloans.local" };
        userManager
            .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        userManager
            .Setup(m => m.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));
        var controller = CreateController(signInManager, userManager);
        var model = new ChangePasswordViewModel
        {
            CurrentPassword = "Wrong1!",
            NewPassword = "New1!",
            ConfirmNewPassword = "New1!"
        };

        var result = await controller.ChangePassword(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task ChangePassword_Post_Success_RefreshesSignInAndRedirects()
    {
        var (signInManager, userManager) = CreateMocks();
        var user = new ApplicationUserEntity { UserName = "admin@bookloans.local" };
        userManager
            .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        userManager
            .Setup(m => m.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        signInManager
            .Setup(m => m.RefreshSignInAsync(user))
            .Returns(Task.CompletedTask);
        var controller = CreateController(signInManager, userManager);
        var model = new ChangePasswordViewModel
        {
            CurrentPassword = "Old1!",
            NewPassword = "New1!",
            ConfirmNewPassword = "New1!"
        };

        var result = await controller.ChangePassword(model);

        signInManager.Verify(m => m.RefreshSignInAsync(user), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Admin", redirect.ControllerName);
    }
}
