using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.DAL.Entities;


public class Book
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [MinLength(2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    [MinLength(5)]
    public string Description { get; set; } = string.Empty;
    public Genre Genre { get; set; }
    public bool IsDeleted { get; set; } = false;     // Soft deletee
    public DateTime? DeletedAt { get; set; }




    [Required]
    [StringLength(100)]
    [MinLength(2)]
    public string Author { get; set; } = string.Empty;

    public string? ImagePath { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Copies { get; set; }

    // Navigation property for transactions
    public virtual ICollection<BorrowingTransaction> Transactions { get; set; } = new List<BorrowingTransaction>();
}
