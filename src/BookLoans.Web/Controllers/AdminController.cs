using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLoans.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    IAdminBookService bookService,
    IAdminAuthorService authorService,
    IAdminBorrowerService borrowerService,
    IAdminConditionService conditionService,
    IAdminCheckoutService checkoutService) : Controller
{
    public IActionResult Index()
        => RedirectToAction(nameof(Checkouts));

    [HttpGet]
    public async Task<IActionResult> Books(CancellationToken ct)
    {
        BooksIndexViewModel viewModel = BooksIndexViewModel.FromDtos(await bookService.GetBooksAsync(ct));
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateBook(CancellationToken ct)
    {
        BookFormViewModel viewModel = BookFormViewModel.FromDto(await bookService.GetCreateFormAsync(ct));
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBook(BookFormViewModel model, CancellationToken ct)
    {
        BookFormViewModel normalizedModel = model.Normalize();

        if (!ModelState.IsValid)
        {
            BookFormViewModel refreshedModel = BookFormViewModel.FromDto(await bookService.GetCreateFormAsync(ct));
            return View(Merge(normalizedModel, refreshedModel));
        }

        string? error = await bookService.CreateAsync(normalizedModel.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            BookFormViewModel refreshedModel = BookFormViewModel.FromDto(await bookService.GetCreateFormAsync(ct));
            return View(Merge(normalizedModel, refreshedModel));
        }

        return RedirectToAction(nameof(Books));
    }

    [HttpGet]
    public async Task<IActionResult> EditBook(int id, CancellationToken ct)
    {
        Book? dto = await bookService.GetEditFormAsync(id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        BookFormViewModel viewModel = BookFormViewModel.FromDto(dto);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBook(int id, BookFormViewModel model, CancellationToken ct)
    {
        BookFormViewModel normalizedModel = model.Normalize();

        if (!ModelState.IsValid)
        {
            Book? refreshedDto = await bookService.GetEditFormAsync(id, ct);
            if (refreshedDto is null)
            {
                return NotFound();
            }

            BookFormViewModel refreshedModel = BookFormViewModel.FromDto(refreshedDto);
            return View(Merge(normalizedModel, refreshedModel));
        }

        string? error = await bookService.UpdateAsync(id, normalizedModel.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            Book? refreshedDto = await bookService.GetEditFormAsync(id, ct);
            if (refreshedDto is null)
            {
                return NotFound();
            }

            BookFormViewModel refreshedModel = BookFormViewModel.FromDto(refreshedDto);
            return View(Merge(normalizedModel, refreshedModel));
        }

        return RedirectToAction(nameof(Books));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBook(int id, CancellationToken ct)
    {
        string? error = await bookService.DeleteAsync(id, ct);
        if (error is not null)
        {
            TempData["BooksError"] = error;
        }

        return RedirectToAction(nameof(Books));
    }

    [HttpGet]
    public async Task<IActionResult> BookPhotoEntity(int id, CancellationToken ct)
    {
        BookPhoto? dto = await bookService.GetPhotoContentAsync(id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        return File(dto.Content, dto.MimeType);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> BookDefaultPhoto(int id, CancellationToken ct)
    {
        BookPhoto? dto = await bookService.GetDefaultPhotoContentAsync(id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        return File(dto.Content, dto.MimeType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBookPhoto(int id, IFormFile? photoFile, string? caption, CancellationToken ct)
    {
        BookPhoto? photo = null;
        if (photoFile is not null)
        {
            await using MemoryStream stream = new();
            await photoFile.CopyToAsync(stream, ct);
            photo = new BookPhoto
            {
                Content = stream.ToArray(),
                MimeType = photoFile.ContentType,
                Caption = caption
            };
        }

        string? error = await bookService.UploadPhotoAsync(id, photo, ct);
        if (error is not null)
        {
            TempData["EditBookError"] = error;
        }

        return RedirectToAction(nameof(EditBook), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBookPhoto(int id, int photoId, CancellationToken ct)
    {
        string? error = await bookService.DeletePhotoAsync(id, photoId, ct);
        if (error is not null)
        {
            TempData["EditBookError"] = error;
        }

        return RedirectToAction(nameof(EditBook), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultBookPhoto(int id, int photoId, CancellationToken ct)
    {
        string? error = await bookService.SetDefaultPhotoAsync(id, photoId, ct);
        if (error is not null)
        {
            TempData["EditBookError"] = error;
        }

        return RedirectToAction(nameof(EditBook), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Authors(CancellationToken ct)
    {
        AuthorsIndexViewModel viewModel = AuthorsIndexViewModel.FromDtos(await authorService.GetAuthorsAsync(ct));
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateAuthor()
    {
        AuthorFormViewModel viewModel = AuthorFormViewModel.FromDto(authorService.GetCreateForm());
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAuthor(AuthorFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? error = await authorService.CreateAsync(model.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        return RedirectToAction(nameof(Authors));
    }

    [HttpGet]
    public async Task<IActionResult> EditAuthor(int id, CancellationToken ct)
    {
        var dto = await authorService.GetEditFormAsync(id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        AuthorFormViewModel viewModel = AuthorFormViewModel.FromDto(dto);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAuthor(int id, AuthorFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? error = await authorService.UpdateAsync(id, model.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        return RedirectToAction(nameof(Authors));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAuthor(int id, CancellationToken ct)
    {
        string? error = await authorService.DeleteAsync(id, ct);
        if (error is not null)
        {
            TempData["AuthorsError"] = error;
        }

        return RedirectToAction(nameof(Authors));
    }

    [HttpGet]
    public async Task<IActionResult> Borrowers(CancellationToken ct)
    {
        BorrowersIndexViewModel viewModel = BorrowersIndexViewModel.FromDtos(await borrowerService.GetBorrowersAsync(ct));
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateBorrower()
    {
        return View(new BorrowerFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBorrower(BorrowerFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? error = await borrowerService.CreateAsync(model.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        return RedirectToAction(nameof(Borrowers));
    }

    [HttpGet]
    public async Task<IActionResult> EditBorrower(int id, CancellationToken ct)
    {
        var dto = await borrowerService.GetEditFormAsync(id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        BorrowerFormViewModel viewModel = BorrowerFormViewModel.FromDto(dto);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBorrower(int id, BorrowerFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var refreshedDto = await borrowerService.GetEditFormAsync(id, ct);
            if (refreshedDto is null)
            {
                return NotFound();
            }

            BorrowerFormViewModel refreshedModel = BorrowerFormViewModel.FromDto(refreshedDto);
            return View(Merge(model, refreshedModel));
        }

        string? error = await borrowerService.UpdateAsync(id, model.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            var refreshedDto = await borrowerService.GetEditFormAsync(id, ct);
            if (refreshedDto is null)
            {
                return NotFound();
            }

            BorrowerFormViewModel refreshedModel = BorrowerFormViewModel.FromDto(refreshedDto);
            return View(Merge(model, refreshedModel));
        }

        return RedirectToAction(nameof(Borrowers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBorrower(int id, CancellationToken ct)
    {
        string? error = await borrowerService.DeleteAsync(id, ct);
        if (error is not null)
        {
            TempData["BorrowersError"] = error;
        }

        return RedirectToAction(nameof(Borrowers));
    }

    [HttpGet]
    public async Task<IActionResult> Conditions(CancellationToken ct)
    {
        ConditionsIndexViewModel viewModel = ConditionsIndexViewModel.FromDtos(await conditionService.GetConditionsAsync(ct));
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateCondition()
    {
        ConditionFormViewModel viewModel = ConditionFormViewModel.FromDto(conditionService.GetCreateForm());
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCondition(ConditionFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? error = await conditionService.CreateAsync(model.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        return RedirectToAction(nameof(Conditions));
    }

    [HttpGet]
    public async Task<IActionResult> EditCondition(int id, CancellationToken ct)
    {
        var dto = await conditionService.GetEditFormAsync(id, ct);
        if (dto is null)
        {
            return NotFound();
        }

        ConditionFormViewModel viewModel = ConditionFormViewModel.FromDto(dto);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCondition(int id, ConditionFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? error = await conditionService.UpdateAsync(id, model.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        return RedirectToAction(nameof(Conditions));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCondition(int id, CancellationToken ct)
    {
        string? error = await conditionService.DeleteAsync(id, ct);
        if (error is not null)
        {
            TempData["ConditionsError"] = error;
        }

        return RedirectToAction(nameof(Conditions));
    }

    [HttpGet]
    public async Task<IActionResult> Checkouts(CancellationToken ct)
    {
        CheckoutsIndexViewModel viewModel = CheckoutsIndexViewModel.FromDtos(await checkoutService.GetCheckoutsAsync(ct));
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateCheckout(int? bookId, int? borrowerId, CancellationToken ct)
    {
        CheckoutFormViewModel viewModel = CheckoutFormViewModel.FromDto(
            await checkoutService.GetCreateFormAsync(bookId, borrowerId, ct));
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCheckout(CheckoutFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var refreshedDto = await checkoutService.RebuildFormAsync(model.ToDto(), ct);
            CheckoutFormViewModel refreshedModel = CheckoutFormViewModel.FromDto(refreshedDto);
            return View(refreshedModel);
        }

        string? error = await checkoutService.CreateAsync(model.ToDto(), ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            var refreshedDto = await checkoutService.RebuildFormAsync(model.ToDto(), ct);
            CheckoutFormViewModel refreshedModel = CheckoutFormViewModel.FromDto(refreshedDto);
            return View(refreshedModel);
        }

        return RedirectToAction(nameof(Checkouts));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnCheckout(int loanId, string returnTo, int sourceId, CancellationToken ct)
    {
        string? error = await checkoutService.ReturnAsync(loanId, ct);
        if (error is not null)
        {
            if (returnTo == nameof(EditBook))
            {
                TempData["EditBookError"] = error;
                return RedirectToAction(nameof(EditBook), new { id = sourceId });
            }

            if (returnTo == nameof(EditBorrower))
            {
                TempData["EditBorrowerError"] = error;
                return RedirectToAction(nameof(EditBorrower), new { id = sourceId });
            }

            TempData["CheckoutsError"] = error;
            return RedirectToAction(nameof(Checkouts));
        }

        if (returnTo == nameof(EditBook))
            return RedirectToAction(nameof(EditBook), new { id = sourceId });

        if (returnTo == nameof(EditBorrower))
            return RedirectToAction(nameof(EditBorrower), new { id = sourceId });

        return RedirectToAction(nameof(Checkouts));
    }

    private static BookFormViewModel Merge(BookFormViewModel model, BookFormViewModel source)
    {
        return new BookFormViewModel
        {
            Id = model.Id,
            Title = model.Title,
            SelectedAuthorIds = model.SelectedAuthorIds,
            NewAuthorNames = model.NewAuthorNames,
            Edition = model.Edition,
            YearFirstPublished = model.YearFirstPublished,
            YearEditionPublished = model.YearEditionPublished,
            Isbn = model.Isbn,
            DateOfPurchase = model.DateOfPurchase,
            LocationOfPurchase = model.LocationOfPurchase,
            ConditionId = model.ConditionId,
            Authors = source.Authors,
            Conditions = source.Conditions,
            CurrentCheckout = source.CurrentCheckout,
            CheckoutHistory = source.CheckoutHistory,
            BookPhotos = source.BookPhotos
        };
    }

    private static BorrowerFormViewModel Merge(BorrowerFormViewModel model, BorrowerFormViewModel source)
    {
        return new BorrowerFormViewModel
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CurrentCheckouts = source.CurrentCheckouts,
            CheckoutHistory = source.CheckoutHistory
        };
    }

}
