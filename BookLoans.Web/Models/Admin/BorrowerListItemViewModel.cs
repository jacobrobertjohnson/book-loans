using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BorrowerListItemViewModel
{
    public int Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public int CurrentBookCount { get; init; }

    public int LoanCount { get; init; }

    public static BorrowerListItemViewModel FromDto(Borrower dto)
        => dto.Adapt<BorrowerListItemViewModel>();

    public Borrower ToDto()
        => this.Adapt<Borrower>();
}
