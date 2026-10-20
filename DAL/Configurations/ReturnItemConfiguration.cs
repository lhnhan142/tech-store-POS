using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ReturnItemConfiguration : IEntityTypeConfiguration<ReturnItem>
    {
        public void Configure(EntityTypeBuilder<ReturnItem> builder)
        {
            builder.ToTable("ReturnItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Condition)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.RefundAmount)
                .HasPrecision(15, 2);

            builder.HasOne(x => x.ReturnRequest)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ReturnRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.InvoiceItem)
                .WithMany(x => x.ReturnItems)
                .HasForeignKey(x => x.InvoiceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
