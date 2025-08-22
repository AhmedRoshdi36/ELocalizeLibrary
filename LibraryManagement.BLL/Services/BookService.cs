using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Models;
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.BLL.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService imageService;
    private readonly IBorrowingService borrowingService;
    
    public BookService(IUnitOfWork _unitOfWork, IImageService _imageService, IBorrowingService _borrowingService)
    {
        unitOfWork = _unitOfWork;  
        imageService = _imageService;
        borrowingService = _borrowingService;
    }

    public Task<IEnumerable<Book>> GetAllBooks()
    {
       var books = unitOfWork.Books.GetAllBooksAsync();
        return books;
    }

    public async Task AddBookAsync(Book book, IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new ValidationException("Book cover image is required.");

        book.ImagePath = await imageService.SaveImageAsync(imageFile, "images/books");
        await unitOfWork.Books.AddAsync(book);
        await unitOfWork.SaveAsync();
    }

    public async Task DeleteBookAsync(int id)
    {
        var existingBook = await unitOfWork.Books.GetByIdAsync(id);
        if (existingBook == null)
            throw new KeyNotFoundException("Book not found");

        // Check if book has borrowed copies
        var borrowedCount = await borrowingService.GetBorrowedCopiesForBooksAsync(new[] { id });
        var borrowedCopies = borrowedCount.GetValueOrDefault(id, 0);
        
        if (borrowedCopies > 0)
        {
            throw new InvalidOperationException($"Cannot delete book '{existingBook.Title}' because it has {borrowedCopies} borrowed copies. All copies must be returned before deletion.");
        }

        unitOfWork.Books.SoftDeleteBook(existingBook);
        await unitOfWork.SaveAsync();
    }

    public async Task<BookDeleteInfo> GetBookDeleteInfoAsync(int id)
    {
        var book = await unitOfWork.Books.GetByIdAsync(id);
        if (book == null)
            throw new KeyNotFoundException("Book not found");

        var borrowedCount = await borrowingService.GetBorrowedCopiesForBooksAsync(new[] { id });
        var borrowedCopies = borrowedCount.GetValueOrDefault(id, 0);
        var availableCopies = book.Copies - borrowedCopies;

        return new BookDeleteInfo
        {
            Book = book,
            TotalCopies = book.Copies,
            BorrowedCopies = borrowedCopies,
            AvailableCopies = availableCopies,
            CanDeleteSafely = borrowedCopies == 0,
            HasBorrowedCopies = borrowedCopies > 0
        };
    }

    

    public async Task<Book> GetBookByIdAsync(int id)
    {
        var book = await unitOfWork.Books.GetByIdAsync(id);
        return book!;
    }

    public async Task UpdateBookAsync(Book book, IFormFile imageFile)
    {
        var existingBook = await unitOfWork.Books.GetByIdAsync(book.Id);
        if (existingBook == null)
            throw new KeyNotFoundException("Book not found");

        if (imageFile != null && imageFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(existingBook.ImagePath))
            {
                imageService.DeleteImage(existingBook.ImagePath);
            }
            existingBook.ImagePath = await imageService.SaveImageAsync(imageFile, "images/books");
        }
        else
        {
            // keep old image
        }
        existingBook.Title = book.Title;
        existingBook.Description = book.Description;
        existingBook.Author = book.Author;
        existingBook.Copies = book.Copies;
        existingBook.Genre = book.Genre;

        // No explicit Update call needed; existingBook is tracked
        await unitOfWork.SaveAsync();
    }

    public async Task<IEnumerable<Book>> GetDeletedBooksAsync()
    {
        return await unitOfWork.Books.GetDeletedBooksAsync();
    }


}
