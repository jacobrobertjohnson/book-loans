using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class CheckoutBorrowerOptionViewModel
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public static CheckoutBorrowerOptionViewModel FromDto(Borrower dto)
        => dto.Adapt<CheckoutBorrowerOptionViewModel>();

    public Borrower ToDto()
        => this.Adapt<Borrower>();
}
