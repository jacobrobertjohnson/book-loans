using System.ComponentModel.DataAnnotations;
using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class CheckoutFormViewModel
{
    [Required]
    public int BookId { get; init; }

    [Required]
    public int BorrowerId { get; init; }

    public IReadOnlyList<CheckoutBookOptionViewModel> Books { get; init; } = [];

    public IReadOnlyList<CheckoutBorrowerOptionViewModel> Borrowers { get; init; } = [];

    public static CheckoutFormViewModel FromDto(BookLoan dto)
        => dto.Adapt<CheckoutFormViewModel>();

    public BookLoan ToDto()
        => this.Adapt<BookLoan>();
}
