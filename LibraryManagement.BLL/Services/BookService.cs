using LibraryManagement.BLL.Interfaces;
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.BLL.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork unitOfWork;
    public BookService(IUnitOfWork _unitOfWork)
    {
        unitOfWork = _unitOfWork;   
    }

    public Task<IEnumerable<Book>> GetAllBooks()
    {
       var books = unitOfWork.Books.GetAllBooksAsync();
        return books;
    }

    public Task AddBookAsync(Book book, IFormFile imageFile)
    {
        throw new NotImplementedException();
    }

    public Task DeleteBookAsync(int id)
    {
        throw new NotImplementedException();
    }

    

    public Task<Book> GetBookByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateBookAsync(Book book, IFormFile imageFile)
    {
        throw new NotImplementedException();
    }
}
