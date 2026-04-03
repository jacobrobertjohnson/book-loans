using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class ConditionsIndexViewModel
{
    public IReadOnlyList<ConditionViewModel> Conditions { get; init; } = [];

    public static ConditionsIndexViewModel FromDtos(IReadOnlyList<Condition> dtos)
        => new() { Conditions = dtos.Adapt<List<ConditionViewModel>>() };

    public IReadOnlyList<Condition> ToDtos()
        => Conditions.Adapt<List<Condition>>();
}
