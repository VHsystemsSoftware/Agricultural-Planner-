//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace VHS.Data.Core.Mappings;
//public class TrayTransactionMap : IEntityTypeConfiguration<TrayTransaction>
//{
//    public void Configure(EntityTypeBuilder<TrayTransaction> builder)
//    {
//        builder.ToTable("TrayTransactions");
//        builder.HasKey(tt => tt.Id);
//        builder.Property(tt => tt.Id).ValueGeneratedOnAdd();

//        builder.Property(tt => tt.TrayId).IsRequired();
//        builder.HasOne(tt => tt.Tray)
//               .WithMany()
//               .HasForeignKey(tt => tt.TrayId)
//               .OnDelete(DeleteBehavior.NoAction);

//        builder.Property(tt => tt.FromPhaseId).IsRequired();
//        builder.Property(tt => tt.ToPhaseId).IsRequired();

//        builder.Property(tt => tt.AddedDateTime).IsRequired();
//    }
//}
