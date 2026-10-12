using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.ToTable("StockMovements");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MovementType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasPrecision(15, 2);

            builder.Property(x => x.ReferenceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Note)
                .HasMaxLength(500);

            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.MovementType);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
