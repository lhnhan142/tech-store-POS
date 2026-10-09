using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DAL.Entities; // Chứa các Models (User, Product, Order...)

namespace DAL.Context
{
    // Bắt buộc kế thừa DbContext từ EntityFrameworkCore
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng dữ liệu (DbSets)
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<InventoryReceipt> InventoryReceipts { get; set; }
        public DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data cơ bản cho Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Quản lý" },
                new Role { Id = 3, Name = "Nhân viên bán hàng" },
                new Role { Id = 4, Name = "Nhân viên kho" }
            );

            // Cấu hình Relationship tránh lỗi vòng lặp cascade delete
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Không cho xóa nhà cung cấp đã có phiếu nhập (khớp FK trong PhuKienMayTinhDB.sql)
            modelBuilder.Entity<InventoryReceipt>()
                .HasOne(r => r.Supplier)
                .WithMany(s => s.InventoryReceipts)
                .HasForeignKey(r => r.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}