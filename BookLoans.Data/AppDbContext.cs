using BookLoans.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookLoans.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUserEntity>(options)
{
	public DbSet<AuthorEntity> Authors => Set<AuthorEntity>();
	public DbSet<BookAuthorEntity> BookAuthors => Set<BookAuthorEntity>();
	public DbSet<BookEntity> Books => Set<BookEntity>();
	public DbSet<BookPhotoEntity> BookPhotos => Set<BookPhotoEntity>();
	public DbSet<BorrowerEntity> Borrowers => Set<BorrowerEntity>();
	public DbSet<BookLoanEntity> BookLoans => Set<BookLoanEntity>();
	public DbSet<ConditionEntity> Conditions => Set<ConditionEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<BookEntity>(book =>
		{
			book.Property(b => b.Title).IsRequired().HasMaxLength(300);
			book.Property(b => b.Edition).HasMaxLength(100);
			book.Property(b => b.Isbn).HasMaxLength(20);
			book.Property(b => b.LocationOfPurchase).HasMaxLength(200);

			book.HasIndex(b => b.Isbn).IsUnique();
			book.ToTable(table =>
			{
				table.HasCheckConstraint(
					"CK_Books_YearFirstPublished",
					"\"YearFirstPublished\" >= -5000 AND \"YearFirstPublished\" <= 3000");
				table.HasCheckConstraint(
					"CK_Books_YearEditionPublished",
					"\"YearEditionPublished\" IS NULL OR (\"YearEditionPublished\" >= -5000 AND \"YearEditionPublished\" <= 3000)");
			});

			book.HasOne(b => b.ConditionEntity)
				.WithMany(c => c.Books)
				.HasForeignKey(b => b.ConditionId)
				.OnDelete(DeleteBehavior.Restrict);

			book.HasOne(b => b.CurrentBorrower)
				.WithMany(borrower => borrower.CurrentBooks)
				.HasForeignKey(b => b.CurrentBorrowerId)
				.OnDelete(DeleteBehavior.SetNull);
		});

		modelBuilder.Entity<AuthorEntity>(author =>
		{
			author.Property(a => a.Name).IsRequired().HasMaxLength(200);
			author.HasIndex(a => a.Name).IsUnique();
		});

		modelBuilder.Entity<BookAuthorEntity>(bookAuthor =>
		{
			bookAuthor.HasKey(entity => new { entity.BookId, entity.AuthorId });

			bookAuthor.HasOne(entity => entity.BookEntity)
				.WithMany(book => book.BookAuthors)
				.HasForeignKey(entity => entity.BookId)
				.OnDelete(DeleteBehavior.Cascade);

			bookAuthor.HasOne(entity => entity.AuthorEntity)
				.WithMany(author => author.BookAuthors)
				.HasForeignKey(entity => entity.AuthorId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<BookPhotoEntity>(photo =>
		{
			photo.Property(p => p.Content).IsRequired();
			photo.Property(p => p.Caption).HasMaxLength(500);
			photo.Property(p => p.SortOrder).HasDefaultValue(0);
			photo.Property(p => p.UploadedAtUtc).IsRequired();
			photo.Property(p => p.MimeType).HasMaxLength(100);
			photo.Property(p => p.ByteLength).IsRequired();

			photo.HasIndex(p => new { p.BookId, p.SortOrder });

			photo.HasOne(p => p.BookEntity)
				.WithMany(b => b.Photos)
				.HasForeignKey(p => p.BookId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<BorrowerEntity>(borrower =>
		{
			borrower.Property(b => b.FirstName).IsRequired().HasMaxLength(100);
			borrower.Property(b => b.LastName).IsRequired().HasMaxLength(100);
		});

		modelBuilder.Entity<BookLoanEntity>(loan =>
		{
			loan.Property(l => l.CheckedOutAtUtc).IsRequired();

			loan.HasOne(l => l.BookEntity)
				.WithMany(b => b.Loans)
				.HasForeignKey(l => l.BookId)
				.OnDelete(DeleteBehavior.Restrict);

			loan.HasOne(l => l.BorrowerEntity)
				.WithMany(b => b.Loans)
				.HasForeignKey(l => l.BorrowerId)
				.OnDelete(DeleteBehavior.Restrict);

			loan.HasIndex(l => new { l.BookId, l.ReturnedAtUtc })
				.IsUnique()
				.HasFilter("\"ReturnedAtUtc\" IS NULL");
			loan.ToTable(table =>
				table.HasCheckConstraint(
					"CK_BookLoans_ReturnedAfterCheckedOut",
					"\"ReturnedAtUtc\" IS NULL OR \"ReturnedAtUtc\" >= \"CheckedOutAtUtc\""));
		});

		modelBuilder.Entity<ConditionEntity>(condition =>
		{
			condition.Property(c => c.Name).IsRequired().HasMaxLength(20);
			condition.HasIndex(c => c.Name).IsUnique();
			condition.HasData(
				new ConditionEntity { Id = 1, Name = "New" },
				new ConditionEntity { Id = 2, Name = "Used" });
		});
	}
}
