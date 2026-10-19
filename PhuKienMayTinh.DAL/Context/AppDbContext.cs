using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PhuKienMayTinh.Common.Constants;
using PhuKienMayTinh.Common.Enums;
using PhuKienMayTinh.DAL.Entities;

namespace PhuKienMayTinh.DAL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<SalaryConfiguration> SalaryConfigurations { get; set; } = null!;
        public DbSet<Payroll> Payrolls { get; set; } = null!;
        public DbSet<PayrollDetail> PayrollDetails { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.Property(e => e.EmployeeCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.IdentityCard).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
                entity.Property(e => e.HourlyRate).HasPrecision(18, 2);
                entity.Property(e => e.LunchAllowance).HasPrecision(18, 2);
                entity.Property(e => e.TravelAllowance).HasPrecision(18, 2);
                entity.Property(e => e.ResponsibilityAllowance).HasPrecision(18, 2);
            });

            // Attendance
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.Employee)
                      .WithMany(e => e.Attendances)
                      .HasForeignKey(a => a.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SalaryConfiguration
            modelBuilder.Entity<SalaryConfiguration>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.OvertimeMultiplier).HasPrecision(5, 2);
                entity.Property(s => s.LatePenaltyPerMinute).HasPrecision(18, 2);
                entity.Property(s => s.AbsencePenaltyPerDay).HasPrecision(18, 2);
                entity.Property(s => s.DiligenceBonus).HasPrecision(18, 2);
                entity.Property(s => s.TaxExemptionLevel).HasPrecision(18, 2);
            });

            // Payroll
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.PayrollCode).IsUnique();
                entity.Property(p => p.PayrollCode).HasMaxLength(30).IsRequired();
                entity.Property(p => p.TotalBaseSalary).HasPrecision(18, 2);
                entity.Property(p => p.TotalAllowance).HasPrecision(18, 2);
                entity.Property(p => p.TotalBonus).HasPrecision(18, 2);
                entity.Property(p => p.TotalDeductions).HasPrecision(18, 2);
                entity.Property(p => p.TotalNetSalary).HasPrecision(18, 2);
            });

            // PayrollDetail
            modelBuilder.Entity<PayrollDetail>(entity =>
            {
                entity.HasKey(pd => pd.Id);
                entity.HasOne(pd => pd.Payroll)
                      .WithMany(p => p.Details)
                      .HasForeignKey(pd => pd.PayrollId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pd => pd.Employee)
                      .WithMany(e => e.PayrollDetails)
                      .HasForeignKey(pd => pd.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(pd => pd.BaseSalary).HasPrecision(18, 2);
                entity.Property(pd => pd.ActualBaseSalary).HasPrecision(18, 2);
                entity.Property(pd => pd.LunchAllowance).HasPrecision(18, 2);
                entity.Property(pd => pd.TravelAllowance).HasPrecision(18, 2);
                entity.Property(pd => pd.ResponsibilityAllowance).HasPrecision(18, 2);
                entity.Property(pd => pd.TotalAllowance).HasPrecision(18, 2);
                entity.Property(pd => pd.DiligenceBonus).HasPrecision(18, 2);
                entity.Property(pd => pd.PerformanceBonus).HasPrecision(18, 2);
                entity.Property(pd => pd.TotalBonus).HasPrecision(18, 2);
                entity.Property(pd => pd.LatePenalty).HasPrecision(18, 2);
                entity.Property(pd => pd.AbsencePenalty).HasPrecision(18, 2);
                entity.Property(pd => pd.InsuranceDeduction).HasPrecision(18, 2);
                entity.Property(pd => pd.OtherDeductions).HasPrecision(18, 2);
                entity.Property(pd => pd.TotalDeductions).HasPrecision(18, 2);
                entity.Property(pd => pd.NetSalary).HasPrecision(18, 2);
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.ProductCode).IsUnique();
                entity.Property(p => p.ProductCode).HasMaxLength(30).IsRequired();
                entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);
                entity.Property(p => p.SellingPrice).HasPrecision(18, 2);
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasIndex(o => o.OrderCode).IsUnique();
                entity.Property(o => o.OrderCode).HasMaxLength(30).IsRequired();
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
                entity.Property(o => o.DiscountAmount).HasPrecision(18, 2);
                entity.Property(o => o.FinalAmount).HasPrecision(18, 2);

                entity.HasOne(o => o.Employee)
                      .WithMany(e => e.Orders)
                      .HasForeignKey(o => o.EmployeeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(od => od.Id);
                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Product)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(od => od.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(od => od.UnitPrice).HasPrecision(18, 2);
                entity.Property(od => od.TotalPrice).HasPrecision(18, 2);
            });
        }
    }
}
