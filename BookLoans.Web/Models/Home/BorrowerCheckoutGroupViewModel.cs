using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public sealed class BorrowerCheckoutGroupViewModel
{
    public string BorrowerFullName { get; init; } = string.Empty;

    public IReadOnlyList<BookLoanViewModel> Books { get; init; } = [];

    public static BorrowerCheckoutGroupViewModel FromDto(BorrowerCheckoutGroup dto)
        => dto.Adapt<BorrowerCheckoutGroupViewModel>();

    public BorrowerCheckoutGroup ToDto()
        => this.Adapt<BorrowerCheckoutGroup>();
}
