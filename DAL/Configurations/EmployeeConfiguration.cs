using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EmployeeCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(x => x.EmployeeCode)
                .IsUnique();

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Position)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.BaseSalary)
                .HasPrecision(18, 2);

            builder.Property(x => x.Allowance)
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PhoneNumber);

            builder.HasOne(x => x.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Attendances)
                .WithOne(a => a.Employee)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.PayrollDetails)
                .WithOne(p => p.Employee)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
