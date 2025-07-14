using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class RecipeMap : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.Name)
               .IsRequired()
               .HasMaxLength(255);
        builder.Property(r => r.Description)
               .HasMaxLength(500);

        builder.HasOne(r => r.Product)
               .WithMany()
               .HasForeignKey(r => r.ProductId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(r => r.GerminationDays).IsRequired();
        builder.Property(r => r.PropagationDays).IsRequired();
        builder.Property(r => r.GrowDays).IsRequired();

        builder.Property(r => r.AddedDateTime).IsRequired();
        builder.Property(r => r.ModifiedDateTime).IsRequired();
        builder.Property(r => r.DeletedDateTime).IsRequired(false);

        builder.HasMany(r => r.BatchPlans)
               .WithOne(bc => bc.Recipe)
               .HasForeignKey(bc => bc.RecipeId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
