using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class AuthorsIndexViewModel
{
    public IReadOnlyList<AuthorViewModel> Authors { get; init; } = [];

    public static AuthorsIndexViewModel FromDtos(IReadOnlyList<Author> dtos)
        => new() { Authors = dtos.Adapt<List<AuthorViewModel>>() };

    public IReadOnlyList<Author> ToDtos()
        => Authors.Adapt<List<Author>>();
}
