using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VHS.Data.Auth.Models.Auth;

namespace VHS.Data.Auth.Mappings
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedOnAdd();

            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.AddedDateTime)
                   .IsRequired();

            builder.Property(u => u.ModifiedDateTime)
                   .IsRequired();

            builder.Property(u => u.DeletedDateTime)
                   .IsRequired(false);
        }
    }
}
