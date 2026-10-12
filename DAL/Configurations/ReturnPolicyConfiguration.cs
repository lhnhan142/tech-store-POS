using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ReturnPolicyConfiguration : IEntityTypeConfiguration<ReturnPolicy>
    {
        public void Configure(EntityTypeBuilder<ReturnPolicy> builder)
        {
            builder.ToTable("ReturnPolicies");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReturnWindowDays)
                .HasDefaultValue(7)
                .IsRequired();

            builder.Property(x => x.ApprovalThresholdAmount)
                .HasPrecision(15, 2)
                .HasDefaultValue(5_000_000m)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
