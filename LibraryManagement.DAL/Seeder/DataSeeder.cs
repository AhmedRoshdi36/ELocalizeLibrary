using LibraryManagement.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LibraryManagement.DAL.Seeder;

internal static class DataSeeder
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        List<Book> items = new()
         {
                new(){
                    Id = 1 ,
                    Title = "C# 12" ,
                    Author ="Ian Griffiths" ,
                    Copies= 5,
                    Description="Build Cloud,Web and Desktop Applications" ,
                    ImagePath="/images/books/CSharp 12 .jpg",
                    IsDeleted=false,
                    Genre =Genre.SoftwareEngineering},

                new(){
                    Id = 2,
                    Title = "C++",
                    Author="Stephan Prata",
                    Copies=4,
                    Description="Intro to C++",
                    ImagePath="/images/books/CPP .jpg" ,
                    IsDeleted=false,
                    Genre =Genre.SoftwareEngineering},

                new(){
                    Id = 3,
                    Title = "Data Structures and Algorithms",
                    Author="David Mount",
                    Copies=4,
                    Description="Data Structures and Algorithms in C++, Goodrich",
                    ImagePath="/images/books/DSA.jpg" ,
                    IsDeleted=false,
                    Genre =Genre.SoftwareEngineering
                },
                new(){
                     Id = 4,
                     Title = "Pride and Prejudice",
                     Author = "Jane Austen",
                     Copies = 3,
                     Description = "A classic romantic novel about love and societal expectations.",
                     ImagePath = "/images/books/Pride .jpg",
                     IsDeleted=false,
                     Genre =Genre.Romance
                },


         };
        modelBuilder.Entity<Book>().HasData(items);
    }
}