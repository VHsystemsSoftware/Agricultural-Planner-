using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VHS.Data.Models.Audit;

namespace VHS.Data.Audit.Mappings;

public class SystemMessageMap : IEntityTypeConfiguration<SystemMessage>
{
    public void Configure(EntityTypeBuilder<SystemMessage> builder)
    {
        builder.ToTable("SystemMessages");
        builder.HasKey(sm => sm.Id);
        builder.Property(sm => sm.Id).ValueGeneratedOnAdd();

        builder.Property(sm => sm.Severity)
               .IsRequired();

        builder.Property(sm => sm.Category)
               .IsRequired();

        builder.Property(sm => sm.Message)
               .IsRequired();

        builder.Property(sm => sm.AddedDateTime)
               .IsRequired();
    }
}
