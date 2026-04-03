using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class CheckoutsIndexViewModel
{
    public IReadOnlyList<BookLoanViewModel> Checkouts { get; init; } = [];

    public static CheckoutsIndexViewModel FromDtos(IReadOnlyList<BookLoan> dtos)
        => new() { Checkouts = dtos.Adapt<List<BookLoanViewModel>>() };

    public IReadOnlyList<BookLoan> ToDtos()
        => Checkouts.Adapt<List<BookLoan>>();
}
