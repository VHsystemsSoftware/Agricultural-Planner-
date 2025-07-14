using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class ProductMap : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.FarmId).IsRequired();
        builder.HasOne(p => p.Farm)
               .WithMany()
               .HasForeignKey(p => p.FarmId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(p => p.ProductCategoryId).IsRequired();

        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(255);
        builder.Property(p => p.Description)
               .HasMaxLength(500);
        builder.Property(p => p.ImageData)
               .HasColumnType("nvarchar(max)")
               .IsRequired(false);
        builder.Property(p => p.SeedIdentifier)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(p => p.SeedSupplier)
               .IsRequired(false)
               .HasMaxLength(255);

        builder.Property(p => p.AddedDateTime).IsRequired();
        builder.Property(p => p.ModifiedDateTime).IsRequired();
        builder.Property(p => p.DeletedDateTime).IsRequired(false);
    }
}
