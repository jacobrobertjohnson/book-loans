using BookLoans.Abstractions.Models;

namespace BookLoans.Data.Entities;

public class BookEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Edition { get; set; }

    public int YearFirstPublished { get; set; }

    public int? YearEditionPublished { get; set; }

    public string? Isbn { get; set; }

    public DateOnly? DateOfPurchase { get; set; }

    public string? LocationOfPurchase { get; set; }

    public int ConditionId { get; set; }

    public ConditionEntity ConditionEntity { get; set; } = null!;

    public int? CurrentBorrowerId { get; set; }

    public BorrowerEntity? CurrentBorrower { get; set; }

    public int? SeriesId { get; set; }

    public SeriesEntity? Series { get; set; }

    public ICollection<BookAuthorEntity> BookAuthors { get; set; } = new List<BookAuthorEntity>();

    public ICollection<BookPhotoEntity> Photos { get; set; } = new List<BookPhotoEntity>();

    public ICollection<BookLoanEntity> Loans { get; set; } = new List<BookLoanEntity>();

    public Book ToAdminBookListItemDto()
        => new()
        {
            Id = Id,
            HasDefaultPhoto = Photos.Count > 0,
            Title = Title,
            SeriesName = Series?.Name,
            AuthorNames = BookAuthors.Count == 0
                ? "(unknown)"
                : string.Join(
                    ", ",
                    BookAuthors
                        .OrderBy(bookAuthor => bookAuthor.AuthorEntity.Name)
                        .Select(bookAuthor => bookAuthor.AuthorEntity.Name)),
            Isbn = Isbn,
            ConditionName = ConditionEntity.Name,
            CurrentBorrowerName = CurrentBorrower == null
                ? null
                : CurrentBorrower.FirstName + " " + CurrentBorrower.LastName,
            CurrentBorrowerId = CurrentBorrowerId,
            CurrentCheckedOutAtUtc = Loans
                .Where(loan => loan.ReturnedAtUtc == null)
                .Select(loan => (DateTime?)loan.CheckedOutAtUtc)
                .FirstOrDefault(),
            Authors = BookAuthors
                .OrderBy(bookAuthor => bookAuthor.AuthorEntity.Name)
                .Select(bookAuthor => new Author { Id = bookAuthor.AuthorEntity.Id, Name = bookAuthor.AuthorEntity.Name })
                .ToList()
        };

    public Book ToAdminCheckoutBookOptionDto()
        => new()
        {
            Id = Id,
            Title = Title,
            AuthorNames = BookAuthors.Count == 0
                ? "(unknown)"
                : string.Join(
                    ", ",
                    BookAuthors
                        .OrderBy(bookAuthor => bookAuthor.AuthorEntity.Name)
                        .Select(bookAuthor => bookAuthor.AuthorEntity.Name)),
            IsCheckedOut = Loans.Any(loan => loan.ReturnedAtUtc == null)
        };

    public static BookEntity FromFormDto(Book dto)
        => new()
        {
            Title = dto.Title,
            Edition = dto.Edition,
            YearFirstPublished = dto.YearFirstPublished,
            YearEditionPublished = dto.YearEditionPublished,
            Isbn = dto.Isbn,
            DateOfPurchase = dto.DateOfPurchase,
            LocationOfPurchase = dto.LocationOfPurchase,
            ConditionId = dto.ConditionId
        };
}