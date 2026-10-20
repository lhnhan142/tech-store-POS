using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
    {
        public void Configure(EntityTypeBuilder<ReturnRequest> builder)
        {
            builder.ToTable("ReturnRequests");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestNo)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.RequestNo)
                .IsUnique();

            builder.HasIndex(x => x.InvoiceId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CreatedAt);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Channel)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ReasonCode)
                .HasMaxLength(50);

            builder.Property(x => x.ReasonNote)
                .HasMaxLength(500);

            builder.Property(x => x.RejectReason)
                .HasMaxLength(500);

            builder.Property(x => x.TotalRefund)
                .HasPrecision(15, 2);

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            // Relationships
            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.ReturnRequests)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExchangeInvoice)
                .WithMany()
                .HasForeignKey(x => x.ExchangeInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceivedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReceivedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
