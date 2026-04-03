using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class AuthorViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int BookCount { get; init; }

    public static AuthorViewModel FromDto(Author dto)
        => dto.Adapt<AuthorViewModel>();

    public Author ToDto()
        => this.Adapt<Author>();
}
