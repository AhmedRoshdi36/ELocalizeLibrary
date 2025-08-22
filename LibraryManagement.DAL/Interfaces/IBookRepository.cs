

using LibraryManagement.DAL.Entities;

namespace LibraryManagement.DAL.Interfaces;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<bool> ExistsAsync(int id);
    void SoftDeleteBook(Book book);
    Task<IEnumerable<Book>> GetDeletedBooksAsync();

}
