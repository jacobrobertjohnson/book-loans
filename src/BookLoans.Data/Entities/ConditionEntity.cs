using BookLoans.Abstractions.Models;

namespace BookLoans.Data.Entities;

public class ConditionEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<BookEntity> Books { get; set; } = new List<BookEntity>();

    public Condition ToAdminConditionFormDto()
        => new() { Id = Id, Name = Name };

    public Condition ToConditionDto()
        => new() { Id = Id, Name = Name, BookCount = Books.Count };

    public static ConditionEntity FromFormDto(Condition dto)
        => new() { Name = dto.Name };
}
