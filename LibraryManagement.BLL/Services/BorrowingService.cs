using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Models;
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.BLL.Services;

public class BorrowingService : IBorrowingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BorrowingService> _logger;

    public BorrowingService(IUnitOfWork unitOfWork, ILogger<BorrowingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> BorrowBookAsync(int bookId)
    {
        _logger.LogInformation("BorrowBookAsync called with bookId: {BookId}", bookId);
        
        var book = await _unitOfWork.Books.GetByIdAsync(bookId);
        if (book == null)
        {
            _logger.LogWarning("Book not found with ID: {BookId}", bookId);
            return false;
        }

        _logger.LogDebug("Book found: {BookTitle} with {Copies} total copies", book.Title, book.Copies);

        // Check if there are available copies by counting unreturned transactions
        var unreturnedTransactions = await _unitOfWork.Transactions.GetUnreturnedTransactionsAsync();
        var borrowedCount = unreturnedTransactions.Count(t => t.BookId == bookId);
        var availableCopies = book.Copies - borrowedCount;
        
        _logger.LogInformation("Book {BookId} ({Title}): Total={TotalCopies}, Borrowed={BorrowedCount}, Available={AvailableCopies}", 
            bookId, book.Title, book.Copies, borrowedCount, availableCopies);
        
        if (availableCopies <= 0)
        {
            _logger.LogWarning("No available copies for book {BookId} ({Title})", bookId, book.Title);
            return false;
        }

        // Create transaction
        var transaction = new BorrowingTransaction
        {
            BookId = bookId,
            BorrowedDate = DateTime.Now
        };

        await _unitOfWork.Transactions.AddAsync(transaction);
        // Don't manually decrement book.Copies - let the transaction count handle it

        var result = await _unitOfWork.SaveAsync();
        _logger.LogInformation("Book {BookId} ({Title}) borrowed successfully. Save result: {SaveResult}", 
            bookId, book.Title, result);
        
        return true;
    }

    public async Task<bool> ReturnBookAsync(int bookId)
    {
        _logger.LogInformation("ReturnBookAsync called with bookId: {BookId}", bookId);
        
        var book = await _unitOfWork.Books.GetByIdAsync(bookId);
        if (book == null)
        {
            _logger.LogWarning("Book not found with ID: {BookId}", bookId);
            return false;
        }

        // Find an open transaction
        var openTransaction = await _unitOfWork.Transactions.GetLatestUnreturnedTransactionAsync(bookId);
        if (openTransaction == null)
        {
            _logger.LogWarning("No unreturned transaction found for book {BookId} ({Title})", bookId, book.Title);
            return false;
        }

        openTransaction.ReturnedDate = DateTime.Now;
        // Don't manually increment book.Copies - let the transaction count handle it

        var result = await _unitOfWork.SaveAsync();
        _logger.LogInformation("Book {BookId} ({Title}) returned successfully. Save result: {SaveResult}", 
            bookId, book.Title, result);
        
        return true;
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetHistoryAsync()
    {
        try
        {
            _logger.LogDebug("GetHistoryAsync called");
            var transactions = await _unitOfWork.Transactions.GetAllAsync();
            var result = transactions ?? new List<BorrowingTransaction>();
            _logger.LogDebug("GetHistoryAsync returned {Count} transactions", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetHistoryAsync");
            return new List<BorrowingTransaction>();
        }
    }

    public async Task<PaginatedList<BorrowingTransaction>> GetHistoryPaginatedAsync(int pageIndex = 1, int pageSize = 10, 
        string searchTerm = "", string status = "", string dateFilter = "")
    {
        try
        {
            _logger.LogDebug("GetHistoryPaginatedAsync called with pageIndex: {PageIndex}, pageSize: {PageSize}, searchTerm: {SearchTerm}, status: {Status}, dateFilter: {DateFilter}", 
                pageIndex, pageSize, searchTerm, status, dateFilter);
            
            var allTransactions = await _unitOfWork.Transactions.GetAllAsync();
            var filteredTransactions = allTransactions.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredTransactions = filteredTransactions.Where(t => 
                    t.Book?.Title?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    t.Book?.Author?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status.ToLower())
                {
                    case "borrowed":
                        filteredTransactions = filteredTransactions.Where(t => t.ReturnedDate == null);
                        break;
                    case "returned":
                        filteredTransactions = filteredTransactions.Where(t => t.ReturnedDate != null);
                        break;
                }
            }

            // Apply date filter
            if (!string.IsNullOrWhiteSpace(dateFilter))
            {
                var today = DateTime.Today;
                switch (dateFilter.ToLower())
                {
                    case "today":
                        filteredTransactions = filteredTransactions.Where(t => 
                            t.BorrowedDate.Date == today);
                        break;
                    case "week":
                        var weekAgo = today.AddDays(-7);
                        filteredTransactions = filteredTransactions.Where(t => 
                            t.BorrowedDate.Date >= weekAgo);
                        break;
                    case "month":
                        var monthAgo = today.AddMonths(-1);
                        filteredTransactions = filteredTransactions.Where(t => 
                            t.BorrowedDate.Date >= monthAgo);
                        break;
                    case "year":
                        var yearAgo = today.AddYears(-1);
                        filteredTransactions = filteredTransactions.Where(t => 
                            t.BorrowedDate.Date >= yearAgo);
                        break;
                }
            }

            var orderedTransactions = filteredTransactions.OrderByDescending(t => t.BorrowedDate).ToList();
            var result = PaginatedList<BorrowingTransaction>.Create(orderedTransactions, pageIndex, pageSize);
            
            _logger.LogDebug("GetHistoryPaginatedAsync returned {TotalCount} transactions across {TotalPages} pages", 
                result.TotalCount, result.TotalPages);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetHistoryPaginatedAsync");
            return new PaginatedList<BorrowingTransaction>(new List<BorrowingTransaction>(), 0, pageIndex, pageSize);
        }
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetUnreturnedTransactionsAsync()
    {
        _logger.LogDebug("GetUnreturnedTransactionsAsync called");
        var result = await _unitOfWork.Transactions.GetUnreturnedTransactionsAsync();
        _logger.LogDebug("GetUnreturnedTransactionsAsync returned {Count} transactions", result.Count());
        return result;
    }

    public async Task<int> GetAvailableCopiesAsync(int bookId)
    {
        _logger.LogDebug("GetAvailableCopiesAsync called for bookId: {BookId}", bookId);
        var book = await _unitOfWork.Books.GetByIdAsync(bookId);
        var result = book?.Copies ?? 0;
        _logger.LogDebug("GetAvailableCopiesAsync returned {AvailableCopies} for bookId: {BookId}", result, bookId);
        return result;
    }

    public async Task<Dictionary<int, int>> GetBorrowedCopiesForBooksAsync(IEnumerable<int> bookIds)
    {
        _logger.LogDebug("GetBorrowedCopiesForBooksAsync called for {BookCount} books", bookIds.Count());
        
        var borrowedCounts = new Dictionary<int, int>();
        
        // Get all unreturned transactions once
        var allUnreturnedTransactions = await _unitOfWork.Transactions.GetUnreturnedTransactionsAsync();
        
        foreach (var bookId in bookIds)
        {
            // Count unreturned transactions for this specific book
            var borrowedCount = allUnreturnedTransactions.Count(t => t.BookId == bookId);
            borrowedCounts[bookId] = borrowedCount;
        }
        
        _logger.LogDebug("GetBorrowedCopiesForBooksAsync completed. Total unreturned transactions: {TotalUnreturned}", 
            allUnreturnedTransactions.Count());
        
        return borrowedCounts;
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetArchivedTransactionsAsync()
    {
        try
        {
            _logger.LogDebug("GetArchivedTransactionsAsync called");
            var archivedTransactions = await _unitOfWork.Transactions.GetArchivedTransactionsAsync();
            var result = archivedTransactions ?? new List<BorrowingTransaction>();
            _logger.LogDebug("GetArchivedTransactionsAsync returned {Count} transactions", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetArchivedTransactionsAsync");
            return new List<BorrowingTransaction>();
        }
    }

    public async Task<bool> ArchiveTransactionAsync(int transactionId)
    {
        try
        {
            _logger.LogInformation("ArchiveTransactionAsync called for transactionId: {TransactionId}", transactionId);
            
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found with ID: {TransactionId}", transactionId);
                return false;
            }

            _unitOfWork.Transactions.ArchiveTransaction(transaction);
            await _unitOfWork.SaveAsync();
            
            _logger.LogInformation("Transaction {TransactionId} archived successfully", transactionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ArchiveTransactionAsync for transactionId: {TransactionId}", transactionId);
            return false;
        }
    }

    public async Task<bool> UnarchiveTransactionAsync(int transactionId)
    {
        try
        {
            _logger.LogInformation("UnarchiveTransactionAsync called for transactionId: {TransactionId}", transactionId);
            
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found with ID: {TransactionId}", transactionId);
                return false;
            }

            _unitOfWork.Transactions.UnarchiveTransaction(transaction);
            await _unitOfWork.SaveAsync();
            
            _logger.LogInformation("Transaction {TransactionId} unarchived successfully", transactionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnarchiveTransactionAsync for transactionId: {TransactionId}", transactionId);
            return false;
        }
    }

    // Method to fix copy count inconsistencies
    public async Task<bool> FixCopyCountInconsistenciesAsync()
    {
        try
        {
            _logger.LogInformation("FixCopyCountInconsistenciesAsync called");
            
            var books = await _unitOfWork.Books.GetAllAsync();
            var unreturnedTransactions = await _unitOfWork.Transactions.GetUnreturnedTransactionsAsync();
            
            var inconsistenciesFound = 0;
            
            foreach (var book in books)
            {
                var actualBorrowedCount = unreturnedTransactions.Count(t => t.BookId == book.Id);
                var originalCopies = book.Copies;
                
                // If there's an inconsistency, log it
                if (actualBorrowedCount > originalCopies)
                {
                    _logger.LogWarning("Inconsistency found for book {BookId} ({Title}): Book.Copies={BookCopies}, Actual borrowed={ActualBorrowed}", 
                        book.Id, book.Title, originalCopies, actualBorrowedCount);
                    inconsistenciesFound++;
                }
            }
            
            _logger.LogInformation("FixCopyCountInconsistenciesAsync completed. Found {Inconsistencies} inconsistencies", inconsistenciesFound);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FixCopyCountInconsistenciesAsync");
            return false;
        }
    }
}
