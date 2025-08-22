using LibraryManagement.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    ImagePath="images/C# 12 .jpg" },

                new(){ 
                    Id = 2,
                    Title = "C++",
                    Author="Stephan Prata",
                    Copies=4,
                    Description="Intro to C++",
                    ImagePath="images/C++.jpg" },
                new(){
                    Id = 3,
                    Title = "Data Structures and Algorithms",
                    Author="David Mount",
                    Copies=4,
                    Description="Data Structures and Algorithms in C++, Goodrich",
                    ImagePath="images/C++.jpg" },


         };
        modelBuilder.Entity<Book>().HasData(items);
    }
}