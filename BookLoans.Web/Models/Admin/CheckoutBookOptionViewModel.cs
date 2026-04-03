using BookLoans.Abstractions.Models;
using Mapster;

namespace BookLoans.Web.Models;

public class CheckoutBookOptionViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string AuthorNames { get; init; } = string.Empty;

    public bool IsCheckedOut { get; init; }

    public static CheckoutBookOptionViewModel FromDto(Book dto)
        => dto.Adapt<CheckoutBookOptionViewModel>();

    public Book ToDto()
        => this.Adapt<Book>();
}
