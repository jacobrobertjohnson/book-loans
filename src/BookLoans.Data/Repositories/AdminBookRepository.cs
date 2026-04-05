using BookLoans.Abstractions.Extensions;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Abstractions.Models;
using BookLoans.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BookLoans.Data.Repositories;

public class AdminBookRepository(AppDbContext dbContext) : IAdminBookRepository
{
	public async Task<IReadOnlyList<Book>> GetBooksAsync(CancellationToken ct)
	{
		List<BookEntity> entities = await dbContext.Books
			.AsNoTracking()
			.Include(book => book.ConditionEntity)
			.Include(book => book.CurrentBorrower)
			.Include(book => book.Loans)
			.Include(book => book.Photos)
			.Include(book => book.BookAuthors)
			.ThenInclude(bookAuthor => bookAuthor.AuthorEntity)
			.Include(book => book.Series)
			.ToListAsync(ct);

		return entities
			.OrderBy(book => (book.Series?.Name ?? book.Title).NormalizeForSort())
			.ThenBy(book => book.Title.NormalizeForSort())
			.Select(book => book.ToAdminBookListItemDto())
			.ToList();
	}

	public async Task<Book> GetCreateFormAsync(CancellationToken ct)
	{
		IReadOnlyList<Author> authors = await GetAuthorsAsync(ct);
		IReadOnlyList<Condition> conditions = await GetConditionsAsync(ct);
		IReadOnlyList<Series> seriesOptions = await GetSeriesOptionsAsync(ct);

		return new Book
		{
			Authors = authors,
			Conditions = conditions,
			SeriesOptions = seriesOptions,
			YearFirstPublished = DateTime.UtcNow.Year,
			ConditionId = conditions.FirstOrDefault()?.Id ?? 0
		};
	}

	public async Task<Book?> GetEditFormAsync(int id, CancellationToken ct)
	{
		BookEntity? book = await dbContext.Books
			.AsNoTracking()
			.Include(entity => entity.Loans)
			.ThenInclude(loan => loan.BorrowerEntity)
			.Include(entity => entity.Photos)
			.Include(entity => entity.BookAuthors)
			.ThenInclude(ba => ba.AuthorEntity)
			.Include(entity => entity.Series)
			.FirstOrDefaultAsync(entity => entity.Id == id, ct);

		if (book is null)
			return null;

		return new Book
		{
			Id = book.Id,
			Title = book.Title,
			SelectedAuthorIds = await dbContext.BookAuthors
				.AsNoTracking()
				.Where(entity => entity.BookId == id)
				.OrderBy(entity => entity.AuthorEntity.Name)
				.Select(entity => entity.AuthorId)
				.ToListAsync(ct),
			SeriesId = book.SeriesId,
			Edition = book.Edition,
			YearFirstPublished = book.YearFirstPublished,
			YearEditionPublished = book.YearEditionPublished,
			Isbn = book.Isbn,
			DateOfPurchase = book.DateOfPurchase,
			LocationOfPurchase = book.LocationOfPurchase,
			ConditionId = book.ConditionId,
			Authors = await GetAuthorsAsync(ct),
			Conditions = await GetConditionsAsync(ct),
			SeriesOptions = await GetSeriesOptionsAsync(ct),
			CurrentCheckout = book.Loans
				.Where(loan => loan.ReturnedAtUtc == null)
				.OrderByDescending(loan => loan.CheckedOutAtUtc)
				.Select(loan => loan.ToBookLoanDto())
				.FirstOrDefault(),
			CheckoutHistory = book.Loans
				.OrderByDescending(loan => loan.CheckedOutAtUtc)
				.Select(loan => loan.ToBookLoanDto())
				.ToList(),
			BookPhotos = book.Photos
				.OrderBy(photo => photo.SortOrder)
				.ThenBy(photo => photo.Id)
				.Select((photo, index) => photo.ToAdminBookPhotoItemDto(index == 0))
				.ToList()
		};
	}

	public async Task<string?> SetDefaultPhotoAsync(int bookId, int photoId, CancellationToken ct)
	{
		List<BookPhotoEntity> photos = await dbContext.BookPhotos
			.Where(photo => photo.BookId == bookId)
			.OrderBy(photo => photo.SortOrder)
			.ThenBy(photo => photo.Id)
			.ToListAsync(ct);

		if (photos.Count == 0)
			return "No photos found for this book.";

		BookPhotoEntity? selectedPhoto = photos
			.FirstOrDefault(photo => photo.Id == photoId);

		if (selectedPhoto is null)
			return "Photo not found.";

		List<BookPhotoEntity> reorderedPhotos = photos
			.Where(photo => photo.Id == photoId)
			.Concat(photos.Where(photo => photo.Id != photoId))
			.ToList();

		for (int i = 0; i < reorderedPhotos.Count; i++)
			reorderedPhotos[i].SortOrder = i;

		await dbContext.SaveChangesAsync(ct);
		return null;
	}

	public async Task<string?> UploadPhotoAsync(int bookId, BookPhoto? photo, CancellationToken ct)
	{
		BookEntity? book = await dbContext.Books
			.Include(entity => entity.Photos)
			.FirstOrDefaultAsync(entity => entity.Id == bookId, ct);

		if (book is null)
			return "BookEntity not found.";

		if (photo is null || photo.Content.Length == 0)
			return "Please select an image file to upload.";

		string? normalizedMimeType = photo.MimeType.NormalizeOrNull();
		if (normalizedMimeType is null || !normalizedMimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
			return "Only image uploads are supported.";

		string? normalizedCaption = photo.Caption.NormalizeOrNull();
		if (normalizedCaption is not null && normalizedCaption.Length > 500)
			return "Photo caption must be 500 characters or fewer.";

		int sortOrder = book.Photos.Count == 0
			? 0
			: book.Photos.Max(entity => entity.SortOrder) + 1;

		BookPhotoEntity bookPhoto = new()
		{
			BookId = bookId,
			Content = photo.Content,
			Caption = normalizedCaption,
			SortOrder = sortOrder,
			UploadedAtUtc = DateTime.UtcNow,
			MimeType = normalizedMimeType,
			ByteLength = photo.Content.LongLength
		};

		dbContext.BookPhotos.Add(bookPhoto);
		await dbContext.SaveChangesAsync(ct);
		return null;
	}

	public async Task<string?> DeletePhotoAsync(int bookId, int photoId, CancellationToken ct)
	{
		BookPhotoEntity? photo = await dbContext.BookPhotos
			.FirstOrDefaultAsync(entity => entity.Id == photoId && entity.BookId == bookId, ct);

		if (photo is null)
			return "Photo not found.";

		dbContext.BookPhotos.Remove(photo);
		await dbContext.SaveChangesAsync(ct);
		return null;
	}

	public async Task<BookPhoto?> GetPhotoContentAsync(int photoId, CancellationToken ct)
	{
		BookPhotoEntity? photo = await dbContext.BookPhotos
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == photoId, ct);

		return photo?.ToContentDto();
	}

	public async Task<BookPhoto?> GetDefaultPhotoContentAsync(int bookId, CancellationToken ct)
	{
		BookPhotoEntity? photo = await dbContext.BookPhotos
			.AsNoTracking()
			.Where(p => p.BookId == bookId)
			.OrderBy(p => p.SortOrder)
			.ThenBy(p => p.Id)
			.FirstOrDefaultAsync(ct);

		return photo?.ToContentDto();
	}

	public async Task<string?> CreateAsync(Book model, CancellationToken ct)
	{
		string? normalizedIsbn = model.Isbn.NormalizeOrNull();
		bool isbnExists = normalizedIsbn is not null && await dbContext.Books
			.AnyAsync(book => book.Isbn == normalizedIsbn, ct);

		if (isbnExists)
			return "A book with this ISBN already exists.";

		bool conditionExists = await dbContext.Conditions
			.AnyAsync(condition => condition.Id == model.ConditionId, ct);

		if (!conditionExists)
			return "Please select a valid condition.";

		(IReadOnlyList<int> authorIds, string? authorError) = await ResolveAuthorIdsAsync(model.SelectedAuthorIds, model.NewAuthorNames, ct);
		if (authorError is not null)
			return authorError;

		(int? seriesId, string? seriesError) = await ResolveSeriesIdAsync(model.SeriesId, model.NewSeriesName, ct);
		if (seriesError is not null)
			return seriesError;

		BookEntity book = BookEntity.FromFormDto(model);
		book.Isbn = normalizedIsbn;
		book.SeriesId = seriesId;
		dbContext.Books.Add(book);

		foreach (int authorId in authorIds)
		{
			dbContext.BookAuthors.Add(new BookAuthorEntity
			{
				BookEntity = book,
				AuthorId = authorId
			});
		}

		await dbContext.SaveChangesAsync(ct);
		return null;
	}

	public async Task<string?> UpdateAsync(int id, Book model, CancellationToken ct)
	{
		BookEntity? book = await dbContext.Books
			.FirstOrDefaultAsync(entity => entity.Id == id, ct);

		if (book is null)
			return "BookEntity not found.";

		string? normalizedIsbn = model.Isbn.NormalizeOrNull();
		bool isbnExists = normalizedIsbn is not null && await dbContext.Books
			.AnyAsync(entity => entity.Id != id && entity.Isbn == normalizedIsbn, ct);

		if (isbnExists)
			return "A book with this ISBN already exists.";

		bool conditionExists = await dbContext.Conditions
			.AnyAsync(condition => condition.Id == model.ConditionId, ct);

		if (!conditionExists)
			return "Please select a valid condition.";

		(IReadOnlyList<int> authorIds, string? authorError) = await ResolveAuthorIdsAsync(model.SelectedAuthorIds, model.NewAuthorNames, ct);
		if (authorError is not null)
			return authorError;

		(int? seriesId, string? seriesError) = await ResolveSeriesIdAsync(model.SeriesId, model.NewSeriesName, ct);
		if (seriesError is not null)
			return seriesError;

		book.Title = model.Title;
		book.Edition = model.Edition;
		book.YearFirstPublished = model.YearFirstPublished;
		book.YearEditionPublished = model.YearEditionPublished;
		book.Isbn = normalizedIsbn;
		book.DateOfPurchase = model.DateOfPurchase;
		book.LocationOfPurchase = model.LocationOfPurchase;
		book.ConditionId = model.ConditionId;
		book.SeriesId = seriesId;

		List<BookAuthorEntity> existingBookAuthors = await dbContext.BookAuthors
			.Where(entity => entity.BookId == id)
			.ToListAsync(ct);

		foreach (BookAuthorEntity bookAuthor in existingBookAuthors)
		{
			if (!authorIds.Contains(bookAuthor.AuthorId))
				dbContext.BookAuthors.Remove(bookAuthor);
		}

		IReadOnlyCollection<int> existingAuthorIds = existingBookAuthors
			.Select(entity => entity.AuthorId)
			.ToList();

		foreach (int authorId in authorIds)
		{
			if (!existingAuthorIds.Contains(authorId))
			{
				dbContext.BookAuthors.Add(new BookAuthorEntity
				{
					BookId = id,
					AuthorId = authorId
				});
			}
		}

		await dbContext.SaveChangesAsync(ct);
		return null;
	}

	public async Task<string?> DeleteAsync(int id, CancellationToken ct)
	{
		BookEntity? book = await dbContext.Books
			.FirstOrDefaultAsync(entity => entity.Id == id, ct);

		if (book is null)
			return "BookEntity not found.";

		List<BookLoanEntity> loans = await dbContext.BookLoans
			.Where(entity => entity.BookId == id)
			.ToListAsync(ct);
		dbContext.BookLoans.RemoveRange(loans);

		List<BookAuthorEntity> bookAuthors = await dbContext.BookAuthors
			.Where(entity => entity.BookId == id)
			.ToListAsync(ct);
		dbContext.BookAuthors.RemoveRange(bookAuthors);

		List<BookPhotoEntity> photos = await dbContext.BookPhotos
			.Where(entity => entity.BookId == id)
			.ToListAsync(ct);
		dbContext.BookPhotos.RemoveRange(photos);

		dbContext.Books.Remove(book);
		await dbContext.SaveChangesAsync(ct);
		return null;
	}

	private async Task<IReadOnlyList<Condition>> GetConditionsAsync(CancellationToken ct)
	{
		List<ConditionEntity> conditions = await dbContext.Conditions
			.AsNoTracking()
			.Include(condition => condition.Books)
			.OrderBy(condition => condition.Name)
			.ToListAsync(ct);

		return conditions
			.Select(condition => condition.ToConditionDto())
			.ToList();
	}

	private async Task<IReadOnlyList<Author>> GetAuthorsAsync(CancellationToken ct)
	{
		List<AuthorEntity> authors = await dbContext.Authors
			.AsNoTracking()
			.Include(author => author.BookAuthors)
			.OrderBy(author => author.Name)
			.ToListAsync(ct);

		return authors
			.Select(author => author.ToAuthorDto())
			.ToList();
	}

	private async Task<IReadOnlyList<Series>> GetSeriesOptionsAsync(CancellationToken ct)
	{
		List<SeriesEntity> series = await dbContext.Series
			.AsNoTracking()
			.Include(s => s.Books)
			.OrderBy(s => s.Name)
			.ToListAsync(ct);

		return series
			.Select(s => s.ToSeriesDto())
			.ToList();
	}

	private async Task<(int? seriesId, string? error)> ResolveSeriesIdAsync(
		int? selectedSeriesId,
		string? newSeriesName,
		CancellationToken ct)
	{
		string? normalizedName = newSeriesName?.Trim().NormalizeOrNull();

		if (normalizedName is not null && selectedSeriesId.HasValue)
			return (null, "Please either select an existing series or enter a new series name, not both.");

		if (normalizedName is not null)
		{
			SeriesEntity? existing = await dbContext.Series
				.FirstOrDefaultAsync(s => s.Name == normalizedName, ct);

			if (existing is not null)
				return (existing.Id, null);

			SeriesEntity newSeries = new() { Name = normalizedName };
			dbContext.Series.Add(newSeries);
			await dbContext.SaveChangesAsync(ct);
			return (newSeries.Id, null);
		}

		if (selectedSeriesId.HasValue)
		{
			bool exists = await dbContext.Series.AnyAsync(s => s.Id == selectedSeriesId, ct);
			if (!exists)
				return (null, "Please select a valid series.");

			return (selectedSeriesId, null);
		}

		return (null, null);
	}

	private async Task<(IReadOnlyList<int> authorIds, string? error)> ResolveAuthorIdsAsync(
		IReadOnlyList<int> existingAuthorIds,
		string? newAuthorNames,
		CancellationToken ct)
	{
		List<int> resolvedAuthorIds = existingAuthorIds
			.Distinct()
			.ToList();

		List<string> parsedNewAuthorNames = ParseNewAuthorNames(newAuthorNames);
		foreach (string parsedName in parsedNewAuthorNames)
		{
			AuthorEntity? existingAuthor = await dbContext.Authors
				.FirstOrDefaultAsync(author => author.Name == parsedName, ct);

			if (existingAuthor is not null)
			{
				if (!resolvedAuthorIds.Contains(existingAuthor.Id))
					resolvedAuthorIds.Add(existingAuthor.Id);

				continue;
			}

			AuthorEntity newAuthor = new()
			{
				Name = parsedName
			};

			dbContext.Authors.Add(newAuthor);
			await dbContext.SaveChangesAsync(ct);
			resolvedAuthorIds.Add(newAuthor.Id);
		}

		if (resolvedAuthorIds.Count == 0)
			return ([], "Please select at least one author or enter a new author name.");

		List<int> validAuthorIds = await dbContext.Authors
			.AsNoTracking()
			.Where(author => resolvedAuthorIds.Contains(author.Id))
			.Select(author => author.Id)
			.ToListAsync(ct);

		if (validAuthorIds.Count != resolvedAuthorIds.Count)
			return ([], "Please select valid authors.");

		return (resolvedAuthorIds, null);
	}

	private static List<string> ParseNewAuthorNames(string? authorNames)
	{
		if (string.IsNullOrWhiteSpace(authorNames))
			return [];

		return authorNames
			.Split(['\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(name => !string.IsNullOrWhiteSpace(name))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	public async Task<BookImportResult> ImportBooksFromCsvAsync(Stream stream, CancellationToken ct)
	{
		List<string[]> rows = ParseCsvRows(stream);

		if (rows.Count < 2)
			return new BookImportResult();

		string[] header = rows[0];
		int titleIdx = FindColumnIndex(header, "Title");
		int authorsIdx = FindColumnIndex(header, "Authors");
		int isbnIdx = FindColumnIndex(header, "ISBN");
		int conditionIdx = FindColumnIndex(header, "Condition");
		int yearFirstPublishedIdx = FindColumnIndex(header, "YearFirstPublished");
		int editionIdx = FindColumnIndex(header, "Edition");
		int yearEditionPublishedIdx = FindColumnIndex(header, "YearEditionPublished");
		int dateOfPurchaseIdx = FindColumnIndex(header, "DateOfPurchase");
		int locationOfPurchaseIdx = FindColumnIndex(header, "LocationOfPurchase");
		int seriesIdx = FindColumnIndex(header, "Series");

		if (titleIdx < 0 || authorsIdx < 0 || conditionIdx < 0 || yearFirstPublishedIdx < 0)
			return new BookImportResult { Errors = [new BookImportRowError(0, string.Empty, "CSV is missing required columns: Title, Authors, Condition, and YearFirstPublished are required.")] };

		Dictionary<string, int> conditionIdsByName = await dbContext.Conditions
			.AsNoTracking()
			.ToDictionaryAsync(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase, ct);

		int successCount = 0;
		List<BookImportRowError> errors = [];

		for (int i = 1; i < rows.Count; i++)
		{
			string[] row = rows[i];
			int rowNumber = i + 1;
			string title = GetField(row, titleIdx);

			if (string.IsNullOrWhiteSpace(title))
				continue;

			string authorsRaw = GetField(row, authorsIdx);
			string newAuthorNames = string.Join('\n', authorsRaw
				.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Where(a => !string.IsNullOrWhiteSpace(a)));

			string conditionName = GetField(row, conditionIdx);
			if (!conditionIdsByName.TryGetValue(conditionName, out int conditionId))
			{
				errors.Add(new BookImportRowError(rowNumber, title, $"Unknown condition \"{conditionName}\". Valid values are: {string.Join(", ", conditionIdsByName.Keys)}."));
				continue;
			}

			string yearRaw = GetField(row, yearFirstPublishedIdx);
			if (!int.TryParse(yearRaw, out int yearFirstPublished))
			{
				errors.Add(new BookImportRowError(rowNumber, title, $"Invalid YearFirstPublished value \"{yearRaw}\". Must be an integer."));
				continue;
			}

			int? yearEditionPublished = null;
			string yearEditionRaw = GetField(row, yearEditionPublishedIdx);
			if (!string.IsNullOrWhiteSpace(yearEditionRaw))
			{
				if (!int.TryParse(yearEditionRaw, out int parsedYearEdition))
				{
					errors.Add(new BookImportRowError(rowNumber, title, $"Invalid YearEditionPublished value \"{yearEditionRaw}\". Must be an integer."));
					continue;
				}

				yearEditionPublished = parsedYearEdition;
			}

			DateOnly? dateOfPurchase = null;
			string dateRaw = GetField(row, dateOfPurchaseIdx);
			if (!string.IsNullOrWhiteSpace(dateRaw))
			{
				if (!DateOnly.TryParse(dateRaw, out DateOnly parsedDate))
				{
					errors.Add(new BookImportRowError(rowNumber, title, $"Invalid DateOfPurchase value \"{dateRaw}\". Expected format: YYYY-MM-DD."));
					continue;
				}

				dateOfPurchase = parsedDate;
			}

			Book book = new()
			{
				Title = title,
				NewAuthorNames = newAuthorNames,
				SelectedAuthorIds = [],
				Isbn = GetField(row, isbnIdx).NormalizeOrNull(),
				ConditionId = conditionId,
				YearFirstPublished = yearFirstPublished,
				Edition = GetField(row, editionIdx).NormalizeOrNull(),
				YearEditionPublished = yearEditionPublished,
				DateOfPurchase = dateOfPurchase,
				LocationOfPurchase = GetField(row, locationOfPurchaseIdx).NormalizeOrNull(),
				NewSeriesName = GetField(row, seriesIdx).NormalizeOrNull()
			};

			string? error = await CreateAsync(book, ct);
			if (error is not null)
				errors.Add(new BookImportRowError(rowNumber, title, error));
			else
				successCount++;
		}

		return new BookImportResult { SuccessCount = successCount, Errors = errors };
	}

	public async Task<string> ExportBooksToCsvAsync(CancellationToken ct)
	{
		const string header = "Title,Authors,ISBN,Condition,YearFirstPublished,Edition,YearEditionPublished,DateOfPurchase,LocationOfPurchase,Series";

		List<BookEntity> books = await dbContext.Books
			.AsNoTracking()
			.Include(book => book.ConditionEntity)
			.Include(book => book.BookAuthors)
			.ThenInclude(bookAuthor => bookAuthor.AuthorEntity)
			.Include(book => book.Series)
			.ToListAsync(ct);

		List<string> lines = [header];

		foreach (BookEntity book in books
			.OrderBy(entity => (entity.Series?.Name ?? entity.Title).NormalizeForSort())
			.ThenBy(entity => entity.Title.NormalizeForSort()))
		{
			string authors = string.Join('|', book.BookAuthors
				.OrderBy(bookAuthor => bookAuthor.AuthorEntity.Name)
				.Select(bookAuthor => bookAuthor.AuthorEntity.Name));

			string[] fields =
			[
				book.Title,
				authors,
				book.Isbn ?? string.Empty,
				book.ConditionEntity.Name,
				book.YearFirstPublished.ToString(),
				book.Edition ?? string.Empty,
				book.YearEditionPublished?.ToString() ?? string.Empty,
				book.DateOfPurchase?.ToString("yyyy-MM-dd") ?? string.Empty,
				book.LocationOfPurchase ?? string.Empty,
				book.Series?.Name ?? string.Empty
			];

			lines.Add(string.Join(',', fields.Select(EscapeCsvField)));
		}

		return string.Join('\n', lines);
	}

	private static List<string[]> ParseCsvRows(Stream stream)
	{
		List<string[]> rows = [];
		using StreamReader reader = new(stream, leaveOpen: true);
		string? line;
		while ((line = reader.ReadLine()) is not null)
		{
			if (!string.IsNullOrWhiteSpace(line))
				rows.Add(SplitCsvRow(line));
		}

		return rows;
	}

	private static string[] SplitCsvRow(string line)
	{
		List<string> fields = [];
		int i = 0;
		while (i < line.Length)
		{
			if (line[i] == '"')
			{
				i++;
					StringBuilder sb = new();
				while (i < line.Length)
				{
					if (line[i] == '"')
					{
						i++;
						if (i < line.Length && line[i] == '"')
						{
							sb.Append('"');
							i++;
						}
						else
							break;
					}
					else
						sb.Append(line[i++]);
				}

				fields.Add(sb.ToString());
				if (i < line.Length && line[i] == ',')
					i++;
			}
			else
			{
				int comma = line.IndexOf(',', i);
				if (comma < 0)
				{
					fields.Add(line[i..].Trim());
					i = line.Length;
				}
				else
				{
					fields.Add(line[i..comma].Trim());
					i = comma + 1;
				}
			}
		}

		if (line.Length > 0 && line[^1] == ',')
			fields.Add(string.Empty);

		return [.. fields];
	}

	private static int FindColumnIndex(string[] header, string name)
		=> Array.FindIndex(header, col => string.Equals(col.Trim(), name, StringComparison.OrdinalIgnoreCase));

	private static string GetField(string[] row, int index)
		=> index >= 0 && index < row.Length ? row[index] : string.Empty;

	private static string EscapeCsvField(string value)
	{
		if (value.Contains('"'))
			value = value.Replace("\"", "\"\"");

		return value.IndexOfAny([',', '"', '\r', '\n']) >= 0
			? $"\"{value}\""
			: value;
	}
}