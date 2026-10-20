using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.ToTable("InvoiceItems", t =>
            {
                t.HasCheckConstraint("CK_InvoiceItem_ReturnedQty", "[ReturnedQty] <= [Qty]");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.UnitPrice)
                .HasPrecision(15, 2);

            builder.Property(x => x.DiscountAlloc)
                .HasPrecision(15, 2);

            builder.Property(x => x.LineTotal)
                .HasPrecision(15, 2);

            builder.Property(x => x.ReturnedQty)
                .HasDefaultValue(0);

            builder.Property(x => x.IsReturnable)
                .HasDefaultValue(true);

            builder.Property(x => x.RowVersion)
                .IsRowVersion();

            // Relationships
            builder.HasOne(x => x.Invoice)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
