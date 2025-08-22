

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
}
