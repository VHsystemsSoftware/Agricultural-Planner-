using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class FarmMap : IEntityTypeConfiguration<Farm>
{
    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.ToTable("Farms");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();

        builder.Property(f => f.Name)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(f => f.Description)
               .HasMaxLength(255);

        builder.Property(f => f.AddedDateTime)
               .IsRequired();
        builder.Property(f => f.DeletedDateTime)
               .IsRequired(false);

        builder.Property(f => f.FarmTypeId)
               .IsRequired();
        builder.HasOne(f => f.FarmType)
               .WithMany(ft => ft.Farms)
               .HasForeignKey(f => f.FarmTypeId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(f => f.Floors)
               .WithOne(fl => fl.Farm)
               .HasForeignKey(fl => fl.FarmId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
