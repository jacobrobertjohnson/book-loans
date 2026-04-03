using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public sealed class PublicHomepageViewModel
{
    public IReadOnlyList<BorrowerCheckoutGroupViewModel> BorrowerGroups { get; init; } = [];

    public static PublicHomepageViewModel FromDtos(IReadOnlyList<BorrowerCheckoutGroup> dtos)
        => new() { BorrowerGroups = dtos.Adapt<List<BorrowerCheckoutGroupViewModel>>() };

    public IReadOnlyList<BorrowerCheckoutGroup> ToDtos()
        => BorrowerGroups.Adapt<List<BorrowerCheckoutGroup>>();
}
