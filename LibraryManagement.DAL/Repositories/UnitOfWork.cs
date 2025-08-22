

using LibraryManagement.DAL.Interfaces;
using LibraryManagement.DAL.Persistance;

namespace LibraryManagement.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;
    public IBookRepository Books { get; }
    public IBorrowingTransactionRepository Transactions { get; }

    public UnitOfWork(LibraryDbContext context)
    {
        _context = context;
        Books = new BookRepository(_context);
        Transactions = new BorrowingTransactionRepository(_context);
    }
    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}

