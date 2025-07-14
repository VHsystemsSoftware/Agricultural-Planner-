using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class FarmTypeMap : IEntityTypeConfiguration<FarmType>
{
    public void Configure(EntityTypeBuilder<FarmType> builder)
    {
        builder.ToTable("FarmTypes");
        builder.HasKey(ft => ft.Id);
        builder.Property(ft => ft.Id).ValueGeneratedOnAdd();

        builder.Property(ft => ft.Name)
               .IsRequired()
               .HasMaxLength(50);
        builder.Property(ft => ft.Description)
               .HasMaxLength(255);

        builder.Property(ft => ft.AddedDateTime)
               .IsRequired();
        builder.Property(ft => ft.DeletedDateTime)
               .IsRequired(false);
    }
}
