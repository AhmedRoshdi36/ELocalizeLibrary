using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Seeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Genre", "ImagePath" },
                values: new object[] { 1, "images/books/CSharp 12 .jpg" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Genre", "ImagePath" },
                values: new object[] { 1, "images/books/CPP .jpg" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Genre", "ImagePath" },
                values: new object[] { 1, "images/books/DSA.jpg" });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Copies", "Description", "Genre", "ImagePath", "Title" },
                values: new object[] { 4, "Jane Austen", 3, "A classic romantic novel about love and societal expectations.", 4, "images/books/Pride .jpg", "Pride and Prejudice" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Genre", "ImagePath" },
                values: new object[] { 0, "images/C# 12 .jpg" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Genre", "ImagePath" },
                values: new object[] { 0, "images/C++.jpg" });

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Genre", "ImagePath" },
                values: new object[] { 0, "images/C++.jpg" });
        }
    }
}
