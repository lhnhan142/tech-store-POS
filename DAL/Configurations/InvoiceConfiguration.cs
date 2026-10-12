using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.InvoiceNo)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.InvoiceNo)
                .IsUnique();

            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.Status);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.SubTotal)
                .HasPrecision(15, 2);

            builder.Property(x => x.DiscountAmount)
                .HasPrecision(15, 2);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(15, 2);

            builder.Property(x => x.CancelReason)
                .HasMaxLength(500);

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            // Relationships
            builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ParentInvoice)
                .WithMany(x => x.ChildInvoices)
                .HasForeignKey(x => x.ParentInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
