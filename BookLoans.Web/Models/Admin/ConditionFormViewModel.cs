using System.ComponentModel.DataAnnotations;
using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class ConditionFormViewModel
{
    public int? Id { get; init; }

    [Required]
    [MaxLength(20)]
    public string Name { get; init; } = string.Empty;

    public static ConditionFormViewModel FromDto(Condition dto)
        => dto.Adapt<ConditionFormViewModel>();

    public Condition ToDto()
        => this.Adapt<Condition>();
}
