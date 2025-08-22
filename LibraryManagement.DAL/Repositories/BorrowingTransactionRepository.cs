using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using LibraryManagement.DAL.Persistance;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories;

public class BorrowingTransactionRepository : GenericRepository<BorrowingTransaction>, IBorrowingTransactionRepository
{
    public BorrowingTransactionRepository(LibraryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetUnreturnedTransactionsAsync()
    {
        return await _context.Set<BorrowingTransaction>()
            .Include(t => t.Book)
            .Where(t => t.ReturnedDate == null && !t.IsArchived)
            .ToListAsync();
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetTransactionsByBookIdAsync(int bookId)
    {
        return await _context.Set<BorrowingTransaction>()
            .Include(t => t.Book)
            .Where(t => t.BookId == bookId && !t.IsArchived)
            .OrderByDescending(t => t.BorrowedDate)
            .ToListAsync();
    }

    public async Task<BorrowingTransaction?> GetLatestUnreturnedTransactionAsync(int bookId)
    {
        return await _context.Set<BorrowingTransaction>()
            .Include(t => t.Book)
            .Where(t => t.BookId == bookId && t.ReturnedDate == null && !t.IsArchived)
            .OrderByDescending(t => t.BorrowedDate)
            .FirstOrDefaultAsync();
    }

    // Override GetAllAsync to include Book navigation property and exclude archived transactions
    public override async Task<IEnumerable<BorrowingTransaction>> GetAllAsync()
    {
        return await _context.Set<BorrowingTransaction>()
            .Include(t => t.Book)
            .Where(t => !t.IsArchived)
            .OrderByDescending(t => t.BorrowedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BorrowingTransaction>> GetArchivedTransactionsAsync()
    {
        return await _context.Set<BorrowingTransaction>()
            .Include(t => t.Book)
            .Where(t => t.IsArchived)
            .OrderByDescending(t => t.BorrowedDate)
            .ToListAsync();
    }

    public void ArchiveTransaction(BorrowingTransaction transaction)
    {
        transaction.IsArchived = true;
        _context.Update(transaction);
    }

    public void UnarchiveTransaction(BorrowingTransaction transaction)
    {
        transaction.IsArchived = false;
        _context.Update(transaction);
    }
}
