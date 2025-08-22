using LibraryManagement.DAL.Entities;
using LibraryManagement.DAL.Seeder;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LibraryManagement.DAL.Persistance;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options){ }
    public DbSet<Book> Books { get; set; } = null!; 
    // Using null-forgiving operator to suppress nullability warning
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Additional model configuration can be added here
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Seed();
    }
    
}
