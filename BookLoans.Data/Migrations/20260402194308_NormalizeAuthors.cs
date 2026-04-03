using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLoans.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAuthors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Books",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "Authors" ("Name")
                SELECT DISTINCT TRIM("AuthorEntity")
                FROM "Books"
                WHERE "AuthorEntity" IS NOT NULL
                    AND TRIM("AuthorEntity") <> '';
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Books"
                SET "AuthorId" = (
                    SELECT "Id"
                    FROM "Authors"
                    WHERE "Authors"."Name" = TRIM("Books"."AuthorEntity")
                    LIMIT 1
                )
                WHERE "AuthorEntity" IS NOT NULL
                    AND TRIM("AuthorEntity") <> '';
                """);

            migrationBuilder.DropColumn(
                name: "AuthorEntity",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_AuthorId",
                table: "Books",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Name",
                table: "Authors",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Authors_AuthorId",
                table: "Books",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorEntity",
                table: "Books",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE "Books"
                SET "AuthorEntity" = COALESCE((
                    SELECT "Name"
                    FROM "Authors"
                    WHERE "Authors"."Id" = "Books"."AuthorId"
                    LIMIT 1
                ), '');
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Authors_AuthorId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropIndex(
                name: "IX_Books_AuthorId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Books");
        }
    }
}
