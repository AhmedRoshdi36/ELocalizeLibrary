using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Models;
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.BLL.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService imageService;
    private readonly IBorrowingService borrowingService;
    private readonly ILogger<BookService> _logger;
    
    public BookService(IUnitOfWork _unitOfWork, IImageService _imageService, IBorrowingService _borrowingService, ILogger<BookService> logger)
    {
        unitOfWork = _unitOfWork;  
        imageService = _imageService;
        borrowingService = _borrowingService;
        _logger = logger;
    }

    public async Task<IEnumerable<Book>> GetAllBooks()
    {
        _logger.LogDebug("GetAllBooks called");
        var books = await unitOfWork.Books.GetAllBooksAsync();
        _logger.LogDebug("GetAllBooks returned {Count} books", books.Count());
        return books;
    }

    public async Task AddBookAsync(Book book, IFormFile imageFile)
    {
        _logger.LogInformation("AddBookAsync called for book: {Title}", book.Title);
        
        if (imageFile == null || imageFile.Length == 0)
        {
            _logger.LogWarning("Book cover image is required for book: {Title}", book.Title);
            throw new ValidationException("Book cover image is required.");
        }

        try
        {
            book.ImagePath = await imageService.SaveImageAsync(imageFile, "images/books");
            await unitOfWork.Books.AddAsync(book);
            await unitOfWork.SaveAsync();
            
            _logger.LogInformation("Book '{Title}' added successfully with {Copies} copies", book.Title, book.Copies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book: {Title}", book.Title);
            throw;
        }
    }

    public async Task DeleteBookAsync(int id)
    {
        _logger.LogInformation("DeleteBookAsync called for bookId: {BookId}", id);
        
        var existingBook = await unitOfWork.Books.GetByIdAsync(id);
        if (existingBook == null)
        {
            _logger.LogWarning("Book not found with ID: {BookId}", id);
            throw new KeyNotFoundException("Book not found");
        }

        // Check if book has borrowed copies
        var borrowedCount = await borrowingService.GetBorrowedCopiesForBooksAsync(new[] { id });
        var borrowedCopies = borrowedCount.GetValueOrDefault(id, 0);
        
        if (borrowedCopies > 0)
        {
            _logger.LogWarning("Cannot delete book '{Title}' because it has {BorrowedCopies} borrowed copies", 
                existingBook.Title, borrowedCopies);
            throw new InvalidOperationException($"Cannot delete book '{existingBook.Title}' because it has {borrowedCopies} borrowed copies. All copies must be returned before deletion.");
        }

        try
        {
            unitOfWork.Books.SoftDeleteBook(existingBook);
            await unitOfWork.SaveAsync();
            
            _logger.LogInformation("Book '{Title}' soft deleted successfully", existingBook.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book: {Title}", existingBook.Title);
            throw;
        }
    }

    public async Task<BookDeleteInfo> GetBookDeleteInfoAsync(int id)
    {
        _logger.LogDebug("GetBookDeleteInfoAsync called for bookId: {BookId}", id);
        
        var book = await unitOfWork.Books.GetByIdAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Book not found with ID: {BookId}", id);
            throw new KeyNotFoundException("Book not found");
        }

        var borrowedCount = await borrowingService.GetBorrowedCopiesForBooksAsync(new[] { id });
        var borrowedCopies = borrowedCount.GetValueOrDefault(id, 0);
        var availableCopies = book.Copies - borrowedCopies;

        var result = new BookDeleteInfo
        {
            Book = book,
            TotalCopies = book.Copies,
            BorrowedCopies = borrowedCopies,
            AvailableCopies = availableCopies,
            CanDeleteSafely = borrowedCopies == 0,
            HasBorrowedCopies = borrowedCopies > 0
        };
        
        _logger.LogDebug("GetBookDeleteInfoAsync for book '{Title}': Total={Total}, Borrowed={Borrowed}, Available={Available}, CanDelete={CanDelete}", 
            book.Title, result.TotalCopies, result.BorrowedCopies, result.AvailableCopies, result.CanDeleteSafely);
        
        return result;
    }

    public async Task<Book> GetBookByIdAsync(int id)
    {
        _logger.LogDebug("GetBookByIdAsync called for bookId: {BookId}", id);
        var book = await unitOfWork.Books.GetByIdAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Book not found with ID: {BookId}", id);
        }
        else
        {
            _logger.LogDebug("GetBookByIdAsync found book: {Title}", book.Title);
        }
        return book!;
    }

    public async Task UpdateBookAsync(Book book, IFormFile imageFile)
    {
        _logger.LogInformation("UpdateBookAsync called for book: {Title}", book.Title);
        
        var existingBook = await unitOfWork.Books.GetByIdAsync(book.Id);
        if (existingBook == null)
        {
            _logger.LogWarning("Book not found with ID: {BookId}", book.Id);
            throw new KeyNotFoundException("Book not found");
        }

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingBook.ImagePath))
                {
                    imageService.DeleteImage(existingBook.ImagePath);
                    _logger.LogDebug("Deleted old image for book: {Title}", book.Title);
                }
                existingBook.ImagePath = await imageService.SaveImageAsync(imageFile, "images/books");
                _logger.LogDebug("Saved new image for book: {Title}", book.Title);
            }
            else
            {
                // keep old image
                _logger.LogDebug("Keeping existing image for book: {Title}", book.Title);
            }
            
            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.Author = book.Author;
            existingBook.Copies = book.Copies;
            existingBook.Genre = book.Genre;

            await unitOfWork.SaveAsync();
            
            _logger.LogInformation("Book '{Title}' updated successfully", book.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book: {Title}", book.Title);
            throw;
        }
    }

    public async Task<IEnumerable<Book>> GetDeletedBooksAsync()
    {
        _logger.LogDebug("GetDeletedBooksAsync called");
        var books = await unitOfWork.Books.GetDeletedBooksAsync();
        _logger.LogDebug("GetDeletedBooksAsync returned {Count} deleted books", books.Count());
        return books;
    }
}
