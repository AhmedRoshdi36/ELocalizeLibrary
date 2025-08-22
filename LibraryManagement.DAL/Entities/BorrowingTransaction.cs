using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.DAL.Entities;

public class BorrowingTransaction
{
    public int Id { get; set; }

    [Required]
    public int BookId { get; set; }

    public virtual Book Book { get; set; } = default!;

    [Required]
    public DateTime BorrowedDate { get; set; }
    public bool IsArchived { get; set; } = false;


    public DateTime? ReturnedDate { get; set; }
}
