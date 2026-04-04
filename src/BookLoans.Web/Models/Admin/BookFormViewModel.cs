using System.ComponentModel.DataAnnotations;
using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BookFormViewModel
{
    public int? Id { get; init; }

    [Required]
    [MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    public List<int> SelectedAuthorIds { get; set; } = [];

    [MaxLength(1000)]
    public string? NewAuthorNames { get; init; }

    public int? SeriesId { get; init; }

    [MaxLength(300)]
    public string? NewSeriesName { get; init; }

    [MaxLength(100)]
    public string? Edition { get; init; }

    [Range(-5000, 3000)]
    public int YearFirstPublished { get; init; }

    [Range(-5000, 3000)]
    public int? YearEditionPublished { get; init; }

    [MaxLength(20)]
    public string? Isbn { get; init; }

    [DataType(DataType.Date)]
    public DateOnly? DateOfPurchase { get; init; }

    [MaxLength(200)]
    public string? LocationOfPurchase { get; init; }

    [Required]
    public int ConditionId { get; init; }

    public IReadOnlyList<AuthorViewModel> Authors { get; init; } = [];

    public IReadOnlyList<ConditionViewModel> Conditions { get; init; } = [];

    public IReadOnlyList<SeriesViewModel> SeriesOptions { get; init; } = [];

    public BookLoanViewModel? CurrentCheckout { get; init; }

    public IReadOnlyList<BookLoanViewModel> CheckoutHistory { get; init; } = [];

    public IReadOnlyList<BookPhotoItemViewModel> BookPhotos { get; init; } = [];

    public static BookFormViewModel FromDto(Book dto)
        => dto.Adapt<BookFormViewModel>();

    public Book ToDto()
        => this.Adapt<Book>();

    public BookFormViewModel Normalize()
    {
        return new BookFormViewModel
        {
            Id = Id,
            Title = Title,
            SelectedAuthorIds = SelectedAuthorIds
                .Distinct()
                .ToList(),
            NewAuthorNames = NewAuthorNames?.Trim(),
            SeriesId = SeriesId,
            NewSeriesName = NewSeriesName?.Trim(),
            Edition = Edition,
            YearFirstPublished = YearFirstPublished,
            YearEditionPublished = YearEditionPublished,
            Isbn = Isbn,
            DateOfPurchase = DateOfPurchase,
            LocationOfPurchase = LocationOfPurchase,
            ConditionId = ConditionId,
            Authors = Authors,
            Conditions = Conditions,
            SeriesOptions = SeriesOptions,
            CurrentCheckout = CurrentCheckout,
            CheckoutHistory = CheckoutHistory,
            BookPhotos = BookPhotos
        };
    }
}
