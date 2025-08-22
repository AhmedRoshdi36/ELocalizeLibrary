using LibraryManagement.DAL.Interfaces;


namespace LibraryManagement.DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
   IBookRepository Books { get; }
   Task<int> SaveAsync();
}


