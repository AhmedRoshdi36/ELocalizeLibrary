using LibraryManagement.DAL.Entities;

namespace LibraryManagement.BLL.Models;

public class BookDeleteInfo
{
    public Book Book { get; set; } = default!;
    public int TotalCopies { get; set; }
    public int BorrowedCopies { get; set; }
    public int AvailableCopies { get; set; }
    public bool CanDeleteSafely { get; set; }
    public bool HasBorrowedCopies { get; set; }
}
