-- ==========================================================================
-- Module Sản phẩm: thêm bảng Brands (Thương hiệu) + cột Products.BrandId
-- cho CSDL ĐÃ TẠO TRƯỚC ĐÓ. Chạy an toàn nhiều lần.
-- CSDL tạo mới từ PhuKienMayTinhDB.sql thì KHÔNG cần chạy file này.
-- ==========================================================================
USE PhuKienMayTinhDB;
GO

IF OBJECT_ID(N'dbo.Brands', N'U') IS NULL
CREATE TABLE Brands (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);
GO

IF COL_LENGTH('dbo.Products', 'BrandId') IS NULL
    ALTER TABLE Products ADD BrandId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_Brands')
    ALTER TABLE Products ADD CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES Brands(Id);
GO

-- Dữ liệu mẫu (chỉ thêm thương hiệu chưa có)
INSERT INTO Brands (Name)
SELECT v.Name FROM (VALUES
 (N'Logitech'), (N'Razer'), (N'Akko'), (N'DareU'), (N'HyperX'), (N'Sony'),
 (N'LG'), (N'Corsair'), (N'Samsung'), (N'Targus'), (N'DXRacer')) AS v(Name)
WHERE NOT EXISTS (SELECT 1 FROM Brands b WHERE b.Name = v.Name);
GO

-- Gán thương hiệu cho sản phẩm chưa có thương hiệu (theo tên sản phẩm)
UPDATE p SET BrandId = b.Id
FROM Products p JOIN Brands b ON p.Name LIKE N'%' + b.Name + N'%'
WHERE p.BrandId IS NULL;
GO
