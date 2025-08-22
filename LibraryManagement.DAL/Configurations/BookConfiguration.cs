

using LibraryManagement.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.DAL.Configurations;

internal class BookConfiguration :IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(b => b.ImagePath)
            .HasMaxLength(300);

        builder.Property(b => b.Description)
            .HasMaxLength(500);
    }
}
