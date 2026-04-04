using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public sealed class HomepageViewModel
{
    public IReadOnlyList<BorrowerCheckoutGroupViewModel> BorrowerGroups { get; init; } = [];

    public IReadOnlyList<AvailableBookViewModel> AvailableBooks { get; init; } = [];

    public static HomepageViewModel FromDto(HomepageData dto)
        => new()
        {
            BorrowerGroups = dto.BorrowerGroups.Adapt<List<BorrowerCheckoutGroupViewModel>>(),
            AvailableBooks = dto.AvailableBooks.Adapt<List<AvailableBookViewModel>>()
        };

    public HomepageData ToDto()
        => new()
        {
            BorrowerGroups = BorrowerGroups.Adapt<List<BorrowerCheckoutGroup>>(),
            AvailableBooks = AvailableBooks.Adapt<List<Book>>()
        };
}
