using BookLoans.Abstractions.Models;

namespace BookLoans.Data.Entities;

public class BorrowerEntity
{
	public int Id { get; set; }

	public string FirstName { get; set; } = string.Empty;

	public string LastName { get; set; } = string.Empty;

	public ICollection<BookEntity> CurrentBooks { get; set; } = new List<BookEntity>();

	public ICollection<BookLoanEntity> Loans { get; set; } = new List<BookLoanEntity>();

	public Borrower ToAdminBorrowerFormDto()
		=> new()
		{
			Id = Id,
			FirstName = FirstName,
			LastName = LastName,
			CurrentCheckouts = Loans
				.Where(loan => loan.ReturnedAtUtc == null)
				.OrderBy(loan => loan.CheckedOutAtUtc)
				.Select(loan => loan.ToBookLoanDto())
				.ToList(),
			CheckoutHistory = Loans
				.Where(loan => loan.ReturnedAtUtc != null)
				.OrderByDescending(loan => loan.CheckedOutAtUtc)
				.Select(loan => loan.ToBookLoanDto())
				.ToList()
		};

	public Borrower ToAdminBorrowerListItemDto()
		=> new()
		{
			Id = Id,
			FirstName = FirstName,
			LastName = LastName,
			CurrentBookCount = CurrentBooks.Count,
			LoanCount = Loans.Count
		};

	public Borrower ToAdminCheckoutBorrowerOptionDto()
		=> new()
		{
			Id = Id,
			FullName = FirstName + " " + LastName
		};

	public static BorrowerEntity FromFormDto(Borrower dto)
		=> new()
		{
			FirstName = dto.FirstName,
			LastName = dto.LastName
		};
}