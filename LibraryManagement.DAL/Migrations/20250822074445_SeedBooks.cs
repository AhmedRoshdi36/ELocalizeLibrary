using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "Books");

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Author", "Copies", "Description", "ImagePath" },
                values: new object[] { "Stephan Prata", 4, "Intro to C++", "images/C++.jpg" });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Copies", "Description", "ImagePath", "Title" },
                values: new object[] { 3, "David Mount", 4, "Data Structures and Algorithms in C++, Goodrich", "images/C++.jpg", "Data Structures and Algorithms" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 1,
                column: "PublishedDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Author", "Copies", "Description", "ImagePath", "PublishedDate" },
                values: new object[] { "", 1, "", "", null });
        }
    }
}
