using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Models;
using LibraryManagement.BLL.Services;
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Tests;

public class BookServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IImageService> _mockImageService;
    private readonly Mock<IBorrowingService> _mockBorrowingService;
    private readonly Mock<ILogger<BookService>> _mockLogger;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockImageService = new Mock<IImageService>();
        _mockBorrowingService = new Mock<IBorrowingService>();
        _mockLogger = new Mock<ILogger<BookService>>();
        _mockBookRepository = new Mock<IBookRepository>();

        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);

        _bookService = new BookService(
            _mockUnitOfWork.Object,
            _mockImageService.Object,
            _mockBorrowingService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAllBooks_ShouldReturnAllBooks()
    {
        // Arrange
        var expectedBooks = new List<Book>
        {
            new Book { Id = 1, Title = "Test Book 1", Author = "Author 1" },
            new Book { Id = 2, Title = "Test Book 2", Author = "Author 2" }
        };

        _mockBookRepository.Setup(repo => repo.GetAllBooksAsync())
            .ReturnsAsync(expectedBooks);

        // Act
        var result = await _bookService.GetAllBooks();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedBooks.Count, result.Count());
        Assert.Equal(expectedBooks, result);
        _mockBookRepository.Verify(repo => repo.GetAllBooksAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithValidId_ShouldReturnBook()
    {
        // Arrange
        var bookId = 1;
        var expectedBook = new Book { Id = bookId, Title = "Test Book", Author = "Test Author" };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(expectedBook);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedBook, result);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var bookId = 999;

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        Assert.Null(result);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task AddBookAsync_WithValidData_ShouldAddBookSuccessfully()
    {
        // Arrange
        var book = new Book { Title = "New Book", Author = "New Author", Copies = 5 };
        var mockImageFile = CreateMockImageFile();

        _mockImageService.Setup(img => img.SaveImageAsync(mockImageFile, "images/books"))
            .ReturnsAsync("images/books/test-image.jpg");

        // Act
        await _bookService.AddBookAsync(book, mockImageFile);

        // Assert
        _mockImageService.Verify(img => img.SaveImageAsync(mockImageFile, "images/books"), Times.Once);
        _mockBookRepository.Verify(repo => repo.AddAsync(book), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task AddBookAsync_WithNullImageFile_ShouldThrowValidationException()
    {
        // Arrange
        var book = new Book { Title = "New Book", Author = "New Author" };
        IFormFile? nullImageFile = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookService.AddBookAsync(book, nullImageFile!));

        Assert.Equal("Book cover image is required.", exception.Message);
        _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task AddBookAsync_WithEmptyImageFile_ShouldThrowValidationException()
    {
        // Arrange
        var book = new Book { Title = "New Book", Author = "New Author" };
        var emptyImageFile = CreateMockImageFile(0); // Empty file

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bookService.AddBookAsync(book, emptyImageFile));

        Assert.Equal("Book cover image is required.", exception.Message);
        _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteBookAsync_WithValidIdAndNoBorrowedCopies_ShouldDeleteSuccessfully()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 5 };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockBorrowingService.Setup(bs => bs.GetBorrowedCopiesForBooksAsync(new[] { bookId }))
            .ReturnsAsync(new Dictionary<int, int> { { bookId, 0 } });

        // Act
        await _bookService.DeleteBookAsync(bookId);

        // Assert
        _mockBookRepository.Verify(repo => repo.SoftDeleteBook(book), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var bookId = 999;

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookService.DeleteBookAsync(bookId));

        Assert.Equal("Book not found", exception.Message);
        _mockBookRepository.Verify(repo => repo.SoftDeleteBook(It.IsAny<Book>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteBookAsync_WithBorrowedCopies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 5 };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockBorrowingService.Setup(bs => bs.GetBorrowedCopiesForBooksAsync(new[] { bookId }))
            .ReturnsAsync(new Dictionary<int, int> { { bookId, 2 } });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookService.DeleteBookAsync(bookId));

        Assert.Contains("Cannot delete book 'Test Book' because it has 2 borrowed copies", exception.Message);
        _mockBookRepository.Verify(repo => repo.SoftDeleteBook(It.IsAny<Book>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetBookDeleteInfoAsync_WithValidId_ShouldReturnCorrectInfo()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 10 };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockBorrowingService.Setup(bs => bs.GetBorrowedCopiesForBooksAsync(new[] { bookId }))
            .ReturnsAsync(new Dictionary<int, int> { { bookId, 3 } });

        // Act
        var result = await _bookService.GetBookDeleteInfoAsync(bookId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(book, result.Book);
        Assert.Equal(10, result.TotalCopies);
        Assert.Equal(3, result.BorrowedCopies);
        Assert.Equal(7, result.AvailableCopies);
        Assert.False(result.CanDeleteSafely);
        Assert.True(result.HasBorrowedCopies);
    }

    [Fact]
    public async Task GetBookDeleteInfoAsync_WithNoBorrowedCopies_ShouldReturnCanDeleteTrue()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 5 };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockBorrowingService.Setup(bs => bs.GetBorrowedCopiesForBooksAsync(new[] { bookId }))
            .ReturnsAsync(new Dictionary<int, int> { { bookId, 0 } });

        // Act
        var result = await _bookService.GetBookDeleteInfoAsync(bookId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CanDeleteSafely);
        Assert.False(result.HasBorrowedCopies);
        Assert.Equal(5, result.AvailableCopies);
    }

    [Fact]
    public async Task GetBookDeleteInfoAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var bookId = 999;

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookService.GetBookDeleteInfoAsync(bookId));

        Assert.Equal("Book not found", exception.Message);
    }

    [Fact]
    public async Task UpdateBookAsync_WithValidDataAndNewImage_ShouldUpdateSuccessfully()
    {
        // Arrange
        var bookId = 1;
        var existingBook = new Book { Id = bookId, Title = "Old Title", ImagePath = "old-image.jpg" };
        var updatedBook = new Book { Id = bookId, Title = "New Title", Author = "New Author" };
        var mockImageFile = CreateMockImageFile();

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        _mockImageService.Setup(img => img.SaveImageAsync(mockImageFile, "images/books"))
            .ReturnsAsync("images/books/new-image.jpg");

        // Act
        await _bookService.UpdateBookAsync(updatedBook, mockImageFile);

        // Assert
        _mockImageService.Verify(img => img.DeleteImage("old-image.jpg"), Times.Once);
        _mockImageService.Verify(img => img.SaveImageAsync(mockImageFile, "images/books"), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
        
        // Verify book properties were updated
        Assert.Equal("New Title", existingBook.Title);
        Assert.Equal("New Author", existingBook.Author);
    }

    [Fact]
    public async Task UpdateBookAsync_WithValidDataAndNoNewImage_ShouldKeepOldImage()
    {
        // Arrange
        var bookId = 1;
        var existingBook = new Book { Id = bookId, Title = "Old Title", ImagePath = "old-image.jpg" };
        var updatedBook = new Book { Id = bookId, Title = "New Title" };
        IFormFile? nullImageFile = null;

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(existingBook);

        // Act
        await _bookService.UpdateBookAsync(updatedBook, nullImageFile!);

        // Assert
        _mockImageService.Verify(img => img.DeleteImage(It.IsAny<string>()), Times.Never);
        _mockImageService.Verify(img => img.SaveImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
        
        // Verify old image path is preserved
        Assert.Equal("old-image.jpg", existingBook.ImagePath);
    }

    [Fact]
    public async Task UpdateBookAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var bookId = 999;
        var updatedBook = new Book { Id = bookId, Title = "New Title" };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookService.UpdateBookAsync(updatedBook, null!));

        Assert.Equal("Book not found", exception.Message);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetDeletedBooksAsync_ShouldReturnDeletedBooks()
    {
        // Arrange
        var expectedDeletedBooks = new List<Book>
        {
            new Book { Id = 1, Title = "Deleted Book 1", IsDeleted = true },
            new Book { Id = 2, Title = "Deleted Book 2", IsDeleted = true }
        };

        _mockBookRepository.Setup(repo => repo.GetDeletedBooksAsync())
            .ReturnsAsync(expectedDeletedBooks);

        // Act
        var result = await _bookService.GetDeletedBooksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDeletedBooks.Count, result.Count());
        Assert.Equal(expectedDeletedBooks, result);
        _mockBookRepository.Verify(repo => repo.GetDeletedBooksAsync(), Times.Once);
    }

    private static IFormFile CreateMockImageFile(long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.FileName).Returns("test-image.jpg");
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        return mockFile.Object;
    }
}
