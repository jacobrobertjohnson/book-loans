using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BorrowersIndexViewModel
{
    public IReadOnlyList<BorrowerListItemViewModel> Borrowers { get; init; } = [];

    public static BorrowersIndexViewModel FromDtos(IReadOnlyList<Borrower> dtos)
        => new() { Borrowers = dtos.Adapt<List<BorrowerListItemViewModel>>() };

    public IReadOnlyList<Borrower> ToDtos()
        => Borrowers.Adapt<List<Borrower>>();
}
