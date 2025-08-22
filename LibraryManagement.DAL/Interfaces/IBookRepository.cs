

using LibraryManagement.DAL.Entities;

namespace LibraryManagement.DAL.Interfaces;

public interface IBookRepository
{
    public Task<IEnumerable<Book>> GetAllBooksAsync();


}
