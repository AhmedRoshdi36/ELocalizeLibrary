

using LibraryManagement.BLL.Models;
using LibraryManagement.DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.BLL.Interfaces;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooks();
    Task<Book> GetBookByIdAsync(int id);
    Task AddBookAsync(Book book, IFormFile imageFile);
    Task UpdateBookAsync(Book book, IFormFile imageFile);
    Task DeleteBookAsync(int id);
    Task<IEnumerable<Book>> GetDeletedBooksAsync();

    Task<BookDeleteInfo> GetBookDeleteInfoAsync(int id);
}
