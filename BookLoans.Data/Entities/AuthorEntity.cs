using BookLoans.Abstractions.Models;

namespace BookLoans.Data.Entities;

public class AuthorEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<BookAuthorEntity> BookAuthors { get; set; } = new List<BookAuthorEntity>();

    public Author ToAdminAuthorFormDto()
        => new() { Id = Id, Name = Name };

    public Author ToAuthorDto()
        => new() { Id = Id, Name = Name, BookCount = BookAuthors.Count };

    public static AuthorEntity FromFormDto(Author dto)
        => new() { Name = dto.Name };
}
