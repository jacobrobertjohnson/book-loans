using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLoans.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPublicationYearRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Books_YearEditionPublished",
                table: "Books");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Books_YearFirstPublished",
                table: "Books");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Books_YearEditionPublished",
                table: "Books",
                sql: "\"YearEditionPublished\" IS NULL OR (\"YearEditionPublished\" >= -5000 AND \"YearEditionPublished\" <= 3000)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Books_YearFirstPublished",
                table: "Books",
                sql: "\"YearFirstPublished\" >= -5000 AND \"YearFirstPublished\" <= 3000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Books_YearEditionPublished",
                table: "Books");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Books_YearFirstPublished",
                table: "Books");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Books_YearEditionPublished",
                table: "Books",
                sql: "\"YearEditionPublished\" IS NULL OR (\"YearEditionPublished\" >= 1000 AND \"YearEditionPublished\" <= 3000)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Books_YearFirstPublished",
                table: "Books",
                sql: "\"YearFirstPublished\" >= 1000 AND \"YearFirstPublished\" <= 3000");
        }
    }
}
