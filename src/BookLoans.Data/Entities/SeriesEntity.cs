using BookLoans.Abstractions.Models;

namespace BookLoans.Data.Entities;

public class SeriesEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<BookEntity> Books { get; set; } = new List<BookEntity>();

    public Series ToSeriesDto()
        => new() { Id = Id, Name = Name, BookCount = Books.Count };

    public static SeriesEntity FromFormDto(Series dto)
        => new() { Name = dto.Name };
}
