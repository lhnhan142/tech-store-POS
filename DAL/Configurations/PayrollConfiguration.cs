using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
    {
        public void Configure(EntityTypeBuilder<Payroll> builder)
        {
            builder.ToTable("Payrolls");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PayrollCode)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(x => x.PayrollCode)
                .IsUnique();

            builder.HasIndex(x => new { x.Month, x.Year })
                .IsUnique();

            builder.Property(x => x.TotalGrossAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.TotalNetAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FinalizedByUser)
                .WithMany()
                .HasForeignKey(x => x.FinalizedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Details)
                .WithOne(d => d.Payroll)
                .HasForeignKey(d => d.PayrollId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
