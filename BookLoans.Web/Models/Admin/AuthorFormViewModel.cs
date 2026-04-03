using System.ComponentModel.DataAnnotations;
using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class AuthorFormViewModel
{
    public int? Id { get; init; }

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    public static AuthorFormViewModel FromDto(Author dto)
        => dto.Adapt<AuthorFormViewModel>();

    public Author ToDto()
        => this.Adapt<Author>();
}
