using LibraryManagement.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LibraryManagement.DAL.Seeder;

internal static class DataSeeder
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        // Seed Books
        List<Book> books = new()
        {
            new(){
                Id = 1,
                Title = "C# 12 Programming",
                Author = "Ian Griffiths",
                Copies = 5,
                Description = "Build Cloud, Web and Desktop Applications with the latest C# features",
                ImagePath = "/images/books/CSharp 12 .jpg",
                IsDeleted = false,
                Genre = Genre.SoftwareEngineering
            },
            new(){
                Id = 2,
                Title = "C++ nProgramming",
                Author = "Stephan Prata",
                Copies = 4,
                Description = "Introduction to C++ programming language",
                ImagePath = "/images/books/CPP .jpg",
                IsDeleted = false,
                Genre = Genre.SoftwareEngineering
            },
            new(){
                Id = 3,
                Title = "Data Structures and Algorithms",
                Author = "David Mount",
                Copies = 3,
                Description = "Data Structures and Algorithms in C++, Goodrich",
                ImagePath = "/images/books/DSA.jpg",
                IsDeleted = false,
                Genre = Genre.SoftwareEngineering
            },
            new(){
                Id = 4,
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                Copies = 2,
                Description = "A classic romantic novel about love and societal expectations",
                ImagePath = "/images/books/Pride .jpg",
                IsDeleted = false,
                Genre = Genre.Romance
            },
            new(){
                Id = 5,
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Copies = 4,
                Description = "A Handbook of Agile Software Craftsmanship",
                ImagePath = "/images/books/CSharp 12 .jpg", // Using existing image
                IsDeleted = false,
                Genre = Genre.SoftwareEngineering
            }
        };

        // Seed Borrowing Transactions (to demonstrate functionality)
        List<BorrowingTransaction> transactions = new()
        {
            new(){
                Id = 1,
                BookId = 1,
                BorrowedDate = new DateTime(2024, 1, 15, 10, 30, 0),
                ReturnedDate = new DateTime(2024, 1, 18, 14, 20, 0),
                IsArchived = false
            },
            new(){
                Id = 2,
                BookId = 2,
                BorrowedDate = new DateTime(2024, 1, 17, 9, 15, 0),
                ReturnedDate = null, // Currently borrowed
                IsArchived = false
            },
            new(){
                Id = 3,
                BookId = 3,
                BorrowedDate = new DateTime(2024, 1, 19, 16, 45, 0),
                ReturnedDate = null, // Currently borrowed
                IsArchived = false
            },
            new(){
                Id = 4,
                BookId = 4,
                BorrowedDate = new DateTime(2024, 1, 10, 11, 0, 0),
                ReturnedDate = new DateTime(2024, 1, 13, 15, 30, 0),
                IsArchived = true // Archived transaction
            }
        };

        // Temporarily commented out to avoid migration issues
        // modelBuilder.Entity<Book>().HasData(books);
        // modelBuilder.Entity<BorrowingTransaction>().HasData(transactions);
    }
}