using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.ToTable("Attendances");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.UserId, x.Date });
            builder.HasIndex(x => new { x.EmployeeId, x.Date });
            builder.HasIndex(x => x.Date);

            builder.Property(x => x.HoursWorked)
                .HasPrecision(5, 2);

            builder.Property(x => x.OvertimeHours)
                .HasPrecision(5, 2);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
