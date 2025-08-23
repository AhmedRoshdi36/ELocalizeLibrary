using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using LibraryManagement.DAL.Persistance;

namespace LibraryManagement.DAL.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(LibraryDbContext context)
    {
        // Only seed if no books exist
        if (!context.Books.Any())
        {
            var books = new List<Book>
            {
                new(){
                    Title = "C# 12 Programming",
                    Author = "Ian Griffiths",
                    Copies = 5,
                    Description = "Build Cloud, Web and Desktop Applications with the latest C# features",
                    ImagePath = "/images/books/CSharp 12 .jpg",
                    IsDeleted = false,
                    Genre = Genre.SoftwareEngineering
                },
                new(){
                    Title = "C++ Programming",
                    Author = "Stephan Prata",
                    Copies = 4,
                    Description = "Introduction to C++ programming language",
                    ImagePath = "/images/books/CPP .jpg",
                    IsDeleted = false,
                    Genre = Genre.SoftwareEngineering
                },
                new(){
                    Title = "Data Structures and Algorithms",
                    Author = "David Mount",
                    Copies = 3,
                    Description = "Data Structures and Algorithms in C++, Goodrich",
                    ImagePath = "/images/books/DSA.jpg",
                    IsDeleted = false,
                    Genre = Genre.SoftwareEngineering
                },
                new(){
                    Title = "Pride and Prejudice",
                    Author = "Jane Austen",
                    Copies = 2,
                    Description = "A classic romantic novel about love and societal expectations",
                    ImagePath = "/images/books/Pride .jpg",
                    IsDeleted = false,
                    Genre = Genre.Romance
                },
                new(){
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Copies = 4,
                    Description = "A Handbook of Agile Software Craftsmanship",
                    ImagePath = "/images/books/CSharp 12 .jpg",
                    IsDeleted = false,
                    Genre = Genre.SoftwareEngineering
                }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();

            // Add borrowing transactions after books are created
            var transactions = new List<BorrowingTransaction>
            {
                new(){
                    BookId = 1,
                    BorrowedDate = new DateTime(2024, 1, 15, 10, 30, 0),
                    ReturnedDate = new DateTime(2024, 1, 18, 14, 20, 0),
                    IsArchived = false
                },
                new(){
                    BookId = 2,
                    BorrowedDate = new DateTime(2024, 1, 17, 9, 15, 0),
                    ReturnedDate = null, // Currently borrowed
                    IsArchived = false
                },
                new(){
                    BookId = 3,
                    BorrowedDate = new DateTime(2024, 1, 19, 16, 45, 0),
                    ReturnedDate = null, // Currently borrowed
                    IsArchived = false
                },
                new(){
                    BookId = 4,
                    BorrowedDate = new DateTime(2024, 1, 10, 11, 0, 0),
                    ReturnedDate = new DateTime(2024, 1, 13, 15, 30, 0),
                    IsArchived = true // Archived transaction
                }
            };

            await context.BorrowingTransactions.AddRangeAsync(transactions);
            await context.SaveChangesAsync();
        }
    }
}
