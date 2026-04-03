using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class BooksIndexViewModel
{
    public IReadOnlyList<BookListItemViewModel> Books { get; init; } = [];

    public static BooksIndexViewModel FromDtos(IReadOnlyList<Book> dtos)
        => new() { Books = dtos.Adapt<List<BookListItemViewModel>>() };

    public IReadOnlyList<Book> ToDtos()
        => Books.Adapt<List<Book>>();
}
