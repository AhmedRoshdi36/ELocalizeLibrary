using LibraryManagement.BLL.Models;
using LibraryManagement.DAL.Entities;

namespace LibraryManagement.BLL.Interfaces;

public interface IBorrowingService
{
    Task<bool> BorrowBookAsync(int bookId);
    Task<bool> ReturnBookAsync(int bookId);
    Task<IEnumerable<BorrowingTransaction>> GetHistoryAsync();
    Task<PaginatedList<BorrowingTransaction>> GetHistoryPaginatedAsync(int pageIndex = 1, int pageSize = 10, 
        string searchTerm = "", string status = "", string dateFilter = "");
    Task<IEnumerable<BorrowingTransaction>> GetUnreturnedTransactionsAsync();
    Task<int> GetAvailableCopiesAsync(int bookId);
    Task<Dictionary<int, int>> GetBorrowedCopiesForBooksAsync(IEnumerable<int> bookIds);
    Task<IEnumerable<BorrowingTransaction>> GetArchivedTransactionsAsync();
    Task<bool> ArchiveTransactionAsync(int transactionId);
    Task<bool> UnarchiveTransactionAsync(int transactionId);
}
