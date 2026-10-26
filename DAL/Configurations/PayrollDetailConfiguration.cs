using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class PayrollDetailConfiguration : IEntityTypeConfiguration<PayrollDetail>
    {
        public void Configure(EntityTypeBuilder<PayrollDetail> builder)
        {
            builder.ToTable("PayrollDetails");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.PayrollId, x.EmployeeId })
                .IsUnique();

            builder.Property(x => x.BaseSalary).HasPrecision(18, 2);
            builder.Property(x => x.ActualWorkDays).HasPrecision(5, 2);
            builder.Property(x => x.TotalHoursWorked).HasPrecision(6, 2);
            builder.Property(x => x.OvertimeHours).HasPrecision(5, 2);
            builder.Property(x => x.SalaryByWorkDays).HasPrecision(18, 2);
            builder.Property(x => x.OvertimePay).HasPrecision(18, 2);
            builder.Property(x => x.Allowance).HasPrecision(18, 2);
            builder.Property(x => x.Bonus).HasPrecision(18, 2);
            builder.Property(x => x.Deductions).HasPrecision(18, 2);
            builder.Property(x => x.NetSalary).HasPrecision(18, 2);

            builder.HasOne(x => x.Employee)
                .WithMany(e => e.PayrollDetails)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
