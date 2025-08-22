
using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Interfaces;
using LibraryManagement.DAL.Persistance;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Repositories;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _context.Books.Where(b=> !b.IsDeleted).ToListAsync();
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Books.AnyAsync(b => b.Id == id);
    }
    public void SoftDeleteBook(Book book)
    { 
        book.IsDeleted = true;
        book.DeletedAt = DateTime.Now;
        _context.Books.Update(book);
    }

    public async Task<IEnumerable<Book>> GetDeletedBooksAsync()
    {
        return await _context.Books.Where(b => b.IsDeleted).ToListAsync();
    }




}
