//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace VHS.Data.Core.Mappings;

//public class TrayCurrentStateMap : IEntityTypeConfiguration<TrayCurrentState>
//{
//    public void Configure(EntityTypeBuilder<TrayCurrentState> builder)
//    {
//        builder.ToTable("TrayCurrentStates");
//        builder.HasKey(tcs => tcs.TrayId);

//        builder.HasOne(tcs => tcs.Tray)
//               .WithOne()
//               .HasForeignKey<TrayCurrentState>(tcs => tcs.TrayId)
//               .OnDelete(DeleteBehavior.NoAction);

//        builder.HasOne(tcs => tcs.DestinationLayer)
//               .WithMany()
//               .HasForeignKey(tcs => tcs.DestinationLayerId)
//               .IsRequired(false)
//               .OnDelete(DeleteBehavior.NoAction);

//        builder.HasOne(tcs => tcs.Batch)
//               .WithMany()
//               .HasForeignKey(tcs => tcs.BatchId)
//               .IsRequired(false)
//               .OnDelete(DeleteBehavior.NoAction);

//        builder.HasOne(tcs => tcs.Job)
//               .WithMany()
//               .HasForeignKey(tcs => tcs.JobId)
//               .IsRequired(false)
//               .OnDelete(DeleteBehavior.NoAction);

//        builder.Property(tcs => tcs.OrderOnLayer)
//               .IsRequired()
//               .HasDefaultValue(0);

//        builder.Property(tcs => tcs.CurrentPhaseId)
//               .IsRequired();

//        builder.Property(tcs => tcs.SeededDateTimeUTC)
//               .IsRequired(false);

//        builder.Property(tcs => tcs.AddedDateTime)
//               .IsRequired();

//        builder.Property(tcs => tcs.ModifiedDateTime)
//               .IsRequired();
//    }
//}
