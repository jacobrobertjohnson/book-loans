using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public sealed class HomepageViewModel
{
    public IReadOnlyList<BorrowerCheckoutGroupViewModel> BorrowerGroups { get; init; } = [];

    public static HomepageViewModel FromDtos(IReadOnlyList<BorrowerCheckoutGroup> dtos)
        => new() { BorrowerGroups = dtos.Adapt<List<BorrowerCheckoutGroupViewModel>>() };

    public IReadOnlyList<BorrowerCheckoutGroup> ToDtos()
        => BorrowerGroups.Adapt<List<BorrowerCheckoutGroup>>();
}
