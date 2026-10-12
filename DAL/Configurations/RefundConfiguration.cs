using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.ToTable("Refunds");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Method)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.Property(x => x.Note)
                .HasMaxLength(500);

            builder.HasOne(x => x.ReturnRequest)
                .WithMany(x => x.Refunds)
                .HasForeignKey(x => x.ReturnRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.InvoicePayment)
                .WithMany(x => x.Refunds)
                .HasForeignKey(x => x.InvoicePaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProcessedByUser)
                .WithMany()
                .HasForeignKey(x => x.ProcessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
