using System.ComponentModel.DataAnnotations;
using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BorrowerFormViewModel
{
    public int? Id { get; init; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    public IReadOnlyList<BookLoanViewModel> CurrentCheckouts { get; init; } = [];

    public IReadOnlyList<BookLoanViewModel> CheckoutHistory { get; init; } = [];

    public static BorrowerFormViewModel FromDto(Borrower dto)
        => dto.Adapt<BorrowerFormViewModel>();

    public Borrower ToDto()
        => this.Adapt<Borrower>();
}
