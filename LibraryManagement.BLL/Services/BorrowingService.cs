using LibraryManagement.BLL.Interfaces;
using LibraryManagement.BLL.Models;
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;

namespace LibraryManagement.BLL.Services;

public class BorrowingService : IBorrowingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BorrowingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> BorrowBookAsync(int bookId)
    {
        Console.WriteLine($"BorrowBookAsync called with bookId: {bookId}");
        var book = await _unitOfWork.Books.GetByIdAsync(bookId);
        Console.WriteLine($"Book found: {book != null}, Copies: {book?.Copies}");
        if (book == null || book.Copies <= 0)
            return false;

        // Create transaction
        var transaction = new BorrowingTransaction
        {
            BookId = bookId,
            BorrowedDate = DateTime.Now
        };

        await _unitOfWork.Transactions.AddAsync(transaction);
        book.Copies--;

        var result = await _unitOfWork.SaveAsync();
        Console.WriteLine($"SaveAsync result: {result}");
        return true;
    }

    public async Task<bool> ReturnBookAsync(int bookId)
    {
        Console.WriteLine($"ReturnBookAsync called with bookId: {bookId}");
        var book = await _unitOfWork.Books.GetByIdAsync(bookId);
        Console.WriteLine($"Book found: {book != null}");
        if (book == null)
            return false;

        // Find an open transaction
        var openTransaction = await _unitOfWork.Transactions.GetLatestUnreturnedTransactionAsync(bookId);
        if (openTransaction == null)
            return false;

        openTransaction.ReturnedDate = DateTime.Now;
        book.Copies++;

        var result = await _unitOfWork.SaveAsync();
        Console.WriteLine($"SaveAsync result: {result}");
        return true;
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetHistoryAsync()
    {
        try
        {
            var transactions = await _unitOfWork.Transactions.GetAllAsync();
            return transactions ?? new List<BorrowingTransaction>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetHistoryAsync: {ex.Message}");
            return new List<BorrowingTransaction>();
        }
    }

    public async Task<PaginatedList<BorrowingTransaction>> GetHistoryPaginatedAsync(int pageIndex = 1, int pageSize = 10, 
        string searchTerm = "", string status = "", string dateFilter = "")
    {
        try
        {
            var allTransactions = await _unitOfWork.Transactions.GetAllAsync();
            var filteredTransactions = allTransactions?.AsEnumerable() ?? new List<BorrowingTransaction>();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                filteredTransactions = filteredTransactions.Where(t => 
                    (t.Book?.Title?.ToLower().Contains(searchLower) ?? false) ||
                    (t.Book?.Author?.ToLower().Contains(searchLower) ?? false));
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
            return PaginatedList<BorrowingTransaction>.Create(orderedTransactions, pageIndex, pageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetHistoryPaginatedAsync: {ex.Message}");
            return new PaginatedList<BorrowingTransaction>(new List<BorrowingTransaction>(), 0, pageIndex, pageSize);
        }
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetUnreturnedTransactionsAsync()
    {
        return await _unitOfWork.Transactions.GetUnreturnedTransactionsAsync();
    }

    public async Task<int> GetAvailableCopiesAsync(int bookId)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(bookId);
        return book?.Copies ?? 0;
    }

    public async Task<Dictionary<int, int>> GetBorrowedCopiesForBooksAsync(IEnumerable<int> bookIds)
    {
        var borrowedCounts = new Dictionary<int, int>();
        
        foreach (var bookId in bookIds)
        {
            var unreturnedTransactions = await _unitOfWork.Transactions.GetUnreturnedTransactionsAsync();
            var borrowedCount = unreturnedTransactions.Count(t => t.BookId == bookId);
            borrowedCounts[bookId] = borrowedCount;
        }
        
        return borrowedCounts;
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetArchivedTransactionsAsync()
    {
        try
        {
            var archivedTransactions = await _unitOfWork.Transactions.GetArchivedTransactionsAsync();
            return archivedTransactions ?? new List<BorrowingTransaction>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetArchivedTransactionsAsync: {ex.Message}");
            return new List<BorrowingTransaction>();
        }
    }

    public async Task<bool> ArchiveTransactionAsync(int transactionId)
    {
        try
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
            if (transaction == null)
                return false;

            _unitOfWork.Transactions.ArchiveTransaction(transaction);
            await _unitOfWork.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ArchiveTransactionAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UnarchiveTransactionAsync(int transactionId)
    {
        try
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
            if (transaction == null)
                return false;

            _unitOfWork.Transactions.UnarchiveTransaction(transaction);
            await _unitOfWork.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UnarchiveTransactionAsync: {ex.Message}");
            return false;
        }
    }
}
