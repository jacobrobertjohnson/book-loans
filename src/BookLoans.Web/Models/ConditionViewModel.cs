using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class ConditionViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int BookCount { get; init; }

    public static ConditionViewModel FromDto(Condition dto)
        => dto.Adapt<ConditionViewModel>();

    public Condition ToDto()
        => this.Adapt<Condition>();
}
