using LibraryManagement.DAL.Entities;

namespace LibraryManagement.DAL.Interfaces;

public interface IBorrowingTransactionRepository : IGenericRepository<BorrowingTransaction>
{
    Task<IEnumerable<BorrowingTransaction>> GetUnreturnedTransactionsAsync();
    Task<IEnumerable<BorrowingTransaction>> GetTransactionsByBookIdAsync(int bookId);
    Task<BorrowingTransaction?> GetLatestUnreturnedTransactionAsync(int bookId);
    Task<IEnumerable<BorrowingTransaction>> GetArchivedTransactionsAsync();
    void ArchiveTransaction(BorrowingTransaction transaction);
    void UnarchiveTransaction(BorrowingTransaction transaction);
}
