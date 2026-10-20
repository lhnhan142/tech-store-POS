using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
    {
        public void Configure(EntityTypeBuilder<InvoicePayment> builder)
        {
            builder.ToTable("InvoicePayments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Method)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.Property(x => x.TransactionCode)
                .HasMaxLength(100);

            builder.Property(x => x.Note)
                .HasMaxLength(500);

            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
