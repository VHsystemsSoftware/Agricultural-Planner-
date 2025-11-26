using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class NoteMap : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.EntityId)
               .IsRequired();

        builder.Property(n => n.Text)
               .IsRequired();

        builder.Property(n => n.AddedDateTime)
               .IsRequired();

        builder.HasIndex(n => n.EntityId);
    }
}
