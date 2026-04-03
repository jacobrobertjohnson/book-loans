namespace BookLoans.Data.Entities;

public class BookAuthorEntity
{
    public int BookId { get; set; }

    public BookEntity BookEntity { get; set; } = null!;

    public int AuthorId { get; set; }

    public AuthorEntity AuthorEntity { get; set; } = null!;
}
