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
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<InventoryReceipt> InventoryReceipts { get; set; }
        public DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }
        public DbSet<StockIssue> StockIssues { get; set; }
        public DbSet<StockIssueDetail> StockIssueDetails { get; set; }
        public DbSet<StockTake> StockTakes { get; set; }
        public DbSet<StockTakeDetail> StockTakeDetails { get; set; }
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

            // Sản phẩm: không cho xóa danh mục / thương hiệu đang có sản phẩm (khớp FK trong SQL)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // Phiếu xuất / kiểm kê: không xóa người lập hay sản phẩm đã nằm trong phiếu;
            // xóa phiếu thì xóa luôn các dòng chi tiết
            modelBuilder.Entity<StockIssue>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockIssueDetail>()
                .HasOne(d => d.Issue)
                .WithMany(i => i.Details)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockIssueDetail>()
                .HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTake>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTakeDetail>()
                .HasOne(d => d.StockTake)
                .WithMany(t => t.Details)
                .HasForeignKey(d => d.StockTakeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockTakeDetail>()
                .HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}