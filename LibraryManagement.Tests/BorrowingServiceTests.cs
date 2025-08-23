using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Models;
using LibraryManagement.BLL.Services;
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LibraryManagement.Tests;

public class BorrowingServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<BorrowingService>> _mockLogger;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IBorrowingTransactionRepository> _mockTransactionRepository;
    private readonly BorrowingService _borrowingService;

    public BorrowingServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<BorrowingService>>();
        _mockBookRepository = new Mock<IBookRepository>();
        _mockTransactionRepository = new Mock<IBorrowingTransactionRepository>();

        _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.Transactions).Returns(_mockTransactionRepository.Object);

        _borrowingService = new BorrowingService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task BorrowBookAsync_WithValidBookAndAvailableCopies_ShouldReturnTrue()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 5 };
        var unreturnedTransactions = new List<BorrowingTransaction>(); // No borrowed copies

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);
        _mockTransactionRepository.Setup(repo => repo.GetUnreturnedTransactionsAsync())
            .ReturnsAsync(unreturnedTransactions);
        _mockTransactionRepository.Setup(repo => repo.AddAsync(It.IsAny<BorrowingTransaction>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.SaveAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _borrowingService.BorrowBookAsync(bookId);

        // Assert
        Assert.True(result);
        _mockTransactionRepository.Verify(repo => repo.AddAsync(It.Is<BorrowingTransaction>(t => 
            t.BookId == bookId && t.ReturnedDate == null)), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task BorrowBookAsync_WithInvalidBookId_ShouldReturnFalse()
    {
        // Arrange
        var bookId = 999;

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _borrowingService.BorrowBookAsync(bookId);

        // Assert
        Assert.False(result);
        _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<BorrowingTransaction>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task BorrowBookAsync_WithNoAvailableCopies_ShouldReturnFalse()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 2 };
        var unreturnedTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { BookId = bookId, BorrowedDate = DateTime.Now },
            new BorrowingTransaction { BookId = bookId, BorrowedDate = DateTime.Now }
        };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);
        _mockTransactionRepository.Setup(repo => repo.GetUnreturnedTransactionsAsync())
            .ReturnsAsync(unreturnedTransactions);

        // Act
        var result = await _borrowingService.BorrowBookAsync(bookId);

        // Assert
        Assert.False(result);
        _mockTransactionRepository.Verify(repo => repo.AddAsync(It.IsAny<BorrowingTransaction>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ReturnBookAsync_WithValidBookAndOpenTransaction_ShouldReturnTrue()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 5 };
        var openTransaction = new BorrowingTransaction 
        { 
            Id = 1, 
            BookId = bookId, 
            BorrowedDate = DateTime.Now.AddDays(-1),
            ReturnedDate = null 
        };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);
        _mockTransactionRepository.Setup(repo => repo.GetLatestUnreturnedTransactionAsync(bookId))
            .ReturnsAsync(openTransaction);
        _mockUnitOfWork.Setup(uow => uow.SaveAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _borrowingService.ReturnBookAsync(bookId);

        // Assert
        Assert.True(result);
        Assert.NotNull(openTransaction.ReturnedDate);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task ReturnBookAsync_WithInvalidBookId_ShouldReturnFalse()
    {
        // Arrange
        var bookId = 999;

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _borrowingService.ReturnBookAsync(bookId);

        // Assert
        Assert.False(result);
        _mockTransactionRepository.Verify(repo => repo.GetLatestUnreturnedTransactionAsync(It.IsAny<int>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ReturnBookAsync_WithNoOpenTransaction_ShouldReturnFalse()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 5 };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);
        _mockTransactionRepository.Setup(repo => repo.GetLatestUnreturnedTransactionAsync(bookId))
            .ReturnsAsync((BorrowingTransaction?)null);

        // Act
        var result = await _borrowingService.ReturnBookAsync(bookId);

        // Assert
        Assert.False(result);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnAllTransactions()
    {
        // Arrange
        var expectedTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { Id = 1, BookId = 1, BorrowedDate = DateTime.Now },
            new BorrowingTransaction { Id = 2, BookId = 2, BorrowedDate = DateTime.Now }
        };

        _mockTransactionRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _borrowingService.GetHistoryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTransactions.Count, result.Count());
        Assert.Equal(expectedTransactions, result);
        _mockTransactionRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenExceptionOccurs_ShouldReturnEmptyList()
    {
        // Arrange
        _mockTransactionRepository.Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _borrowingService.GetHistoryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHistoryPaginatedAsync_WithNoFilters_ShouldReturnPaginatedResults()
    {
        // Arrange
        var allTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { Id = 1, BookId = 1, BorrowedDate = DateTime.Now.AddDays(-1) },
            new BorrowingTransaction { Id = 2, BookId = 2, BorrowedDate = DateTime.Now.AddDays(-2) },
            new BorrowingTransaction { Id = 3, BookId = 3, BorrowedDate = DateTime.Now.AddDays(-3) }
        };

        _mockTransactionRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(allTransactions);

        // Act
        var result = await _borrowingService.GetHistoryPaginatedAsync(pageIndex: 1, pageSize: 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetHistoryPaginatedAsync_WithSearchTerm_ShouldFilterResults()
    {
        // Arrange
        var allTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction 
            { 
                Id = 1, 
                BookId = 1, 
                BorrowedDate = DateTime.Now,
                Book = new Book { Title = "C# Programming", Author = "John Doe" }
            },
            new BorrowingTransaction 
            { 
                Id = 2, 
                BookId = 2, 
                BorrowedDate = DateTime.Now,
                Book = new Book { Title = "Java Programming", Author = "Jane Smith" }
            }
        };

        _mockTransactionRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(allTransactions);

        // Act
        var result = await _borrowingService.GetHistoryPaginatedAsync(searchTerm: "C#");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetHistoryPaginatedAsync_WithStatusFilter_ShouldFilterResults()
    {
        // Arrange
        var allTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { Id = 1, BookId = 1, BorrowedDate = DateTime.Now, ReturnedDate = null },
            new BorrowingTransaction { Id = 2, BookId = 2, BorrowedDate = DateTime.Now, ReturnedDate = DateTime.Now }
        };

        _mockTransactionRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(allTransactions);

        // Act
        var result = await _borrowingService.GetHistoryPaginatedAsync(status: "borrowed");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Null(result.Items.First().ReturnedDate);
    }

    [Fact]
    public async Task GetHistoryPaginatedAsync_WithDateFilter_ShouldFilterResults()
    {
        // Arrange
        var allTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { Id = 1, BookId = 1, BorrowedDate = DateTime.Today },
            new BorrowingTransaction { Id = 2, BookId = 2, BorrowedDate = DateTime.Today.AddDays(-10) }
        };

        _mockTransactionRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(allTransactions);

        // Act
        var result = await _borrowingService.GetHistoryPaginatedAsync(dateFilter: "today");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(DateTime.Today.Date, result.Items.First().BorrowedDate.Date);
    }

    [Fact]
    public async Task GetUnreturnedTransactionsAsync_ShouldReturnUnreturnedTransactions()
    {
        // Arrange
        var expectedTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { Id = 1, BookId = 1, ReturnedDate = null },
            new BorrowingTransaction { Id = 2, BookId = 2, ReturnedDate = null }
        };

        _mockTransactionRepository.Setup(repo => repo.GetUnreturnedTransactionsAsync())
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _borrowingService.GetUnreturnedTransactionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTransactions.Count, result.Count());
        Assert.Equal(expectedTransactions, result);
        _mockTransactionRepository.Verify(repo => repo.GetUnreturnedTransactionsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableCopiesAsync_WithValidBook_ShouldReturnBookCopies()
    {
        // Arrange
        var bookId = 1;
        var book = new Book { Id = bookId, Title = "Test Book", Copies = 5 };

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        // Act
        var result = await _borrowingService.GetAvailableCopiesAsync(bookId);

        // Assert
        Assert.Equal(5, result);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetAvailableCopiesAsync_WithInvalidBook_ShouldReturnZero()
    {
        // Arrange
        var bookId = 999;

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _borrowingService.GetAvailableCopiesAsync(bookId);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetBorrowedCopiesForBooksAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var bookIds = new[] { 1, 2, 3 };
        var unreturnedTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { BookId = 1 },
            new BorrowingTransaction { BookId = 1 },
            new BorrowingTransaction { BookId = 2 },
            new BorrowingTransaction { BookId = 3 },
            new BorrowingTransaction { BookId = 3 }
        };

        _mockTransactionRepository.Setup(repo => repo.GetUnreturnedTransactionsAsync())
            .ReturnsAsync(unreturnedTransactions);

        // Act
        var result = await _borrowingService.GetBorrowedCopiesForBooksAsync(bookIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result[1]); // Book 1 has 2 borrowed copies
        Assert.Equal(1, result[2]); // Book 2 has 1 borrowed copy
        Assert.Equal(2, result[3]); // Book 3 has 2 borrowed copies
    }

    [Fact]
    public async Task GetArchivedTransactionsAsync_ShouldReturnArchivedTransactions()
    {
        // Arrange
        var expectedTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { Id = 1, IsArchived = true },
            new BorrowingTransaction { Id = 2, IsArchived = true }
        };

        _mockTransactionRepository.Setup(repo => repo.GetArchivedTransactionsAsync())
            .ReturnsAsync(expectedTransactions);

        // Act
        var result = await _borrowingService.GetArchivedTransactionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTransactions.Count, result.Count());
        Assert.Equal(expectedTransactions, result);
        _mockTransactionRepository.Verify(repo => repo.GetArchivedTransactionsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetArchivedTransactionsAsync_WhenExceptionOccurs_ShouldReturnEmptyList()
    {
        // Arrange
        _mockTransactionRepository.Setup(repo => repo.GetArchivedTransactionsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _borrowingService.GetArchivedTransactionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ArchiveTransactionAsync_WithValidTransaction_ShouldReturnTrue()
    {
        // Arrange
        var transactionId = 1;
        var transaction = new BorrowingTransaction { Id = transactionId };

        _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);
        _mockTransactionRepository.Setup(repo => repo.ArchiveTransaction(transaction))
            .Verifiable();
        _mockUnitOfWork.Setup(uow => uow.SaveAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _borrowingService.ArchiveTransactionAsync(transactionId);

        // Assert
        Assert.True(result);
        _mockTransactionRepository.Verify(repo => repo.ArchiveTransaction(transaction), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task ArchiveTransactionAsync_WithInvalidTransactionId_ShouldReturnFalse()
    {
        // Arrange
        var transactionId = 999;

        _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync((BorrowingTransaction?)null);

        // Act
        var result = await _borrowingService.ArchiveTransactionAsync(transactionId);

        // Assert
        Assert.False(result);
        _mockTransactionRepository.Verify(repo => repo.ArchiveTransaction(It.IsAny<BorrowingTransaction>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task ArchiveTransactionAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        var transactionId = 1;
        var transaction = new BorrowingTransaction { Id = transactionId };

        _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);
        _mockTransactionRepository.Setup(repo => repo.ArchiveTransaction(transaction))
            .Throws(new Exception("Archive error"));

        // Act
        var result = await _borrowingService.ArchiveTransactionAsync(transactionId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UnarchiveTransactionAsync_WithValidTransaction_ShouldReturnTrue()
    {
        // Arrange
        var transactionId = 1;
        var transaction = new BorrowingTransaction { Id = transactionId };

        _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);
        _mockTransactionRepository.Setup(repo => repo.UnarchiveTransaction(transaction))
            .Verifiable();
        _mockUnitOfWork.Setup(uow => uow.SaveAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _borrowingService.UnarchiveTransactionAsync(transactionId);

        // Assert
        Assert.True(result);
        _mockTransactionRepository.Verify(repo => repo.UnarchiveTransaction(transaction), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UnarchiveTransactionAsync_WithInvalidTransactionId_ShouldReturnFalse()
    {
        // Arrange
        var transactionId = 999;

        _mockTransactionRepository.Setup(repo => repo.GetByIdAsync(transactionId))
            .ReturnsAsync((BorrowingTransaction?)null);

        // Act
        var result = await _borrowingService.UnarchiveTransactionAsync(transactionId);

        // Assert
        Assert.False(result);
        _mockTransactionRepository.Verify(repo => repo.UnarchiveTransaction(It.IsAny<BorrowingTransaction>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task FixCopyCountInconsistenciesAsync_ShouldReturnTrue()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Copies = 5 },
            new Book { Id = 2, Title = "Book 2", Copies = 3 }
        };
        var unreturnedTransactions = new List<BorrowingTransaction>
        {
            new BorrowingTransaction { BookId = 1 },
            new BorrowingTransaction { BookId = 1 },
            new BorrowingTransaction { BookId = 2 }
        };

        _mockBookRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(books);
        _mockTransactionRepository.Setup(repo => repo.GetUnreturnedTransactionsAsync())
            .ReturnsAsync(unreturnedTransactions);

        // Act
        var result = await _borrowingService.FixCopyCountInconsistenciesAsync();

        // Assert
        Assert.True(result);
        _mockBookRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        _mockTransactionRepository.Verify(repo => repo.GetUnreturnedTransactionsAsync(), Times.Once);
    }

    [Fact]
    public async Task FixCopyCountInconsistenciesAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        _mockBookRepository.Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _borrowingService.FixCopyCountInconsistenciesAsync();

        // Assert
        Assert.False(result);
    }
}
