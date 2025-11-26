using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VHS.Data.Auth.Models.Auth;

namespace VHS.Data.Auth.Mappings
{
    public class UserSettingMap : IEntityTypeConfiguration<UserSetting>
    {
        public void Configure(EntityTypeBuilder<UserSetting> builder)
        {
            builder.ToTable("UserSettings");
            builder.HasKey(us => us.Id);
            builder.Property(us => us.Id).ValueGeneratedOnAdd();

            builder.Property(us => us.UserId)
                   .IsRequired();

            builder.HasIndex(us => us.UserId).IsUnique();

            builder.Property(us => us.PreferredLanguage)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(us => us.PreferredTheme)
                   .HasMaxLength(20);

            builder.Property(us => us.PreferredMeasurementSystem)
                   .HasMaxLength(20);

            builder.Property(us => us.PreferredWeightUnit)
                   .HasMaxLength(20);

            builder.Property(us => us.PreferredLengthUnit)
                   .HasMaxLength(20);

            builder.Property(us => us.PreferredTemperatureUnit)
                   .HasMaxLength(20);

            builder.Property(us => us.PreferredVolumeUnit)
                   .HasMaxLength(20);

            builder.Property(us => us.PreferredDateTimeFormat)
                   .HasMaxLength(50)
                   .IsRequired()
                   .HasDefaultValue("dd-MM-yyyy HH:mm");

            builder.Property(us => us.AddedDateTime)
                   .IsRequired();

            builder.Property(us => us.ModifiedDateTime)
                   .IsRequired();

            builder.Property(us => us.DeletedDateTime)
                   .IsRequired(false);

            builder.HasOne(us => us.User)
                   .WithOne(u => u.UserSetting)
                   .HasForeignKey<UserSetting>(us => us.UserId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
