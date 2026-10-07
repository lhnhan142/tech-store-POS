-- ==========================================
-- TẠO CƠ SỞ DỮ LIỆU
-- ==========================================
CREATE DATABASE PhuKienMayTinhDB;
GO

USE PhuKienMayTinhDB;
GO

-- ==========================================
-- 1. TẠO BẢNG (SCHEMA)
-- ==========================================

-- Bảng Roles (Phân quyền)
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Bảng Users (Tài khoản/Nhân viên)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    RoleId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0: Active, 1: Inactive, 2: Blocked
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- Bảng Attendances (Chấm công)
CREATE TABLE Attendances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Date DATETIME2 NOT NULL,
    CheckIn TIME NULL,
    CheckOut TIME NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Attendances_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Bảng Categories (Danh mục sản phẩm)
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Bảng Products (Sản phẩm)
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Barcode NVARCHAR(50) NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(MAX) NULL,
    CategoryId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Bảng Suppliers (Nhà cung cấp)
CREATE TABLE Suppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Bảng InventoryReceipts (Phiếu nhập kho)
CREATE TABLE InventoryReceipts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL,
    UserId INT NOT NULL,
    ReceiptDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_InventoryReceipts_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    CONSTRAINT FK_InventoryReceipts_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Bảng InventoryReceiptDetails (Chi tiết nhập kho)
CREATE TABLE InventoryReceiptDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_InventoryReceiptDetails_Receipts FOREIGN KEY (ReceiptId) REFERENCES InventoryReceipts(Id),
    CONSTRAINT FK_InventoryReceiptDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Bảng Customers (Khách hàng)
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    RewardPoints INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Bảng Orders (Hóa đơn bán hàng)
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NULL,
    UserId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0, -- 0: Pending, 1: Completed, 2: Cancelled, 3: Returned
    PaymentMethod INT NOT NULL DEFAULT 0, -- 0: Cash, 1: Transfer, 2: Card
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- Bảng OrderDetails (Chi tiết hóa đơn)
CREATE TABLE OrderDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- ==========================================
-- 2. CHÈN DỮ LIỆU MẪU (MOCK DATA)
-- ==========================================

-- Thêm Quyền
INSERT INTO Roles (Name) VALUES (N'Admin'), (N'Quản lý'), (N'Nhân viên bán hàng'), (N'Nhân viên kho');

-- Thêm Tài khoản
INSERT INTO Users (Username, PasswordHash, FullName, RoleId, Status) VALUES 
('admin', 'hashed_pw_123', N'Quản trị viên', 1, 0), -- Id: 1
('quanly1', 'hashed_pw_123', N'Trần Văn Quản', 2, 0), -- Id: 2
('banhang1', 'hashed_pw_123', N'Nguyễn Thị Bán', 3, 0), -- Id: 3
('banhang2', 'hashed_pw_123', N'Lê Bán Hàng', 3, 0), -- Id: 4
('kho1', 'hashed_pw_123', N'Phạm Văn Kho', 4, 0), -- Id: 5
('banhang3', 'hashed_pw_123', N'Phạm Hoàng Long', 3, 0), -- Id: 6
('banhang4', 'hashed_pw_123', N'Đỗ Thị Mai', 3, 0), -- Id: 7
('kho2', 'hashed_pw_123', N'Vũ Ngọc Tuấn', 4, 0), -- Id: 8
('kho3', 'hashed_pw_123', N'Bùi Thanh Sơn', 4, 1); -- Id: 9

-- Thêm Danh mục
INSERT INTO Categories (Name) VALUES 
(N'Chuột máy tính'), (N'Bàn phím'), (N'Tai nghe'), (N'Màn hình'), (N'Lót chuột'),
(N'Linh kiện PC (RAM, Ổ cứng)'), (N'Vỏ Case & Tản nhiệt'), (N'Balo & Túi chống sốc'), 
(N'Webcam & Microphone'), (N'Ghế & Bàn Gaming');

-- Thêm Sản phẩm
INSERT INTO Products (Name, Barcode, Price, StockQuantity, ImageUrl, CategoryId) VALUES 
(N'Chuột Logitech G102', '880123456789', 390000, 50, 'g102.jpg', 1),
(N'Chuột Razer DeathAdder Essential', '880123456790', 550000, 30, 'razer_da.jpg', 1),
(N'Bàn phím cơ Akko 3087', '880123456791', 1200000, 20, 'akko3087.jpg', 2),
(N'Bàn phím DareU EK810', '880123456792', 600000, 45, 'dareu.jpg', 2),
(N'Tai nghe HyperX Cloud II', '880123456793', 1800000, 15, 'hyperx.jpg', 3),
(N'Tai nghe Sony WH-1000XM4', '880123456794', 6500000, 10, 'sony.jpg', 3),
(N'Màn hình LG 24MP60G', '880123456795', 3200000, 25, 'lg24.jpg', 4),
(N'Lót chuột Corsair MM300', '880123456796', 350000, 100, 'corsair.jpg', 5),
(N'Chuột không dây Logitech MX Master 3S', '880123456801', 2500000, 15, 'mxmaster3s.jpg', 1),
(N'RAM Corsair Vengeance RGB Pro 16GB (2x8GB)', '880123456809', 1450000, 50, 'ram_corsair.jpg', 6),
(N'Ổ cứng SSD Samsung 980 PRO 1TB PCIe 4.0', '880123456810', 2300000, 40, 'ssd_ss.jpg', 6),
(N'Balo Laptop Targus 15.6 inch', '880123456815', 750000, 60, 'balo.jpg', 8),
(N'Ghế Gaming DXRacer Master Series', '880123456818', 8500000, 5, 'dxracer.jpg', 10);

-- Thêm Nhà cung cấp
INSERT INTO Suppliers (Name, Phone, Address) VALUES 
(N'Công ty TNHH Logitech Việt Nam', '0283123456', N'Quận 1, TP.HCM'),
(N'Đại lý Phân phối Razer', '0909123456', N'Quận 3, TP.HCM'),
(N'Công ty TNHH Máy tính Phong Vũ', '18006868', N'Quận 3, TP.HCM'),
(N'GearVN Cung ứng sỉ', '18006975', N'Tân Bình, TP.HCM');

-- Thêm Khách hàng
INSERT INTO Customers (Name, Phone, RewardPoints) VALUES 
(N'Nguyễn Văn A', '0901000001', 150),
(N'Trần Thị B', '0901000002', 50),
(N'Đinh Công Thành', '0912345671', 1200),
(N'Khách Lẻ (Vãng lai)', '0000000000', 0),
(N'Trịnh Công Sơn', '0912345677', 1500);

-- Thêm Phiếu nhập kho (Receipts)
INSERT INTO InventoryReceipts (SupplierId, UserId, ReceiptDate, TotalAmount) VALUES 
(1, 5, GETDATE(), 19500000), -- Id: 1
(3, 5, DATEADD(day, -1, GETDATE()), 35000000), -- Id: 2
(4, 8, DATEADD(day, -2, GETDATE()), 85000000); -- Id: 3

-- Thêm Chi tiết nhập kho
INSERT INTO InventoryReceiptDetails (ReceiptId, ProductId, Quantity, UnitPrice) VALUES 
(1, 1, 50, 390000),
(2, 10, 10, 1450000),
(2, 11, 5, 2300000),
(3, 13, 5, 8500000);

-- Thêm Hóa đơn bán hàng (Orders)
INSERT INTO Orders (CustomerId, UserId, OrderDate, TotalAmount, DiscountAmount, Status, PaymentMethod) VALUES 
(1, 3, GETDATE(), 1590000, 0, 1, 0), -- Id: 1
(3, 3, GETDATE(), 3400000, 0, 1, 1), -- Id: 2
(4, 6, GETDATE(), 2500000, 100000, 1, 2), -- Id: 3
(4, 3, DATEADD(day, -1, GETDATE()), 750000, 0, 1, 0), -- Id: 4
(2, 6, DATEADD(day, -1, GETDATE()), 6200000, 200000, 1, 1), -- Id: 5
(5, 7, GETDATE(), 1990000, 0, 0, 1), -- Id: 6 (Pending)
(1, 3, DATEADD(day, -2, GETDATE()), 1450000, 0, 2, 0), -- Id: 7 (Cancelled)
(3, 6, DATEADD(day, -3, GETDATE()), 1650000, 0, 3, 2); -- Id: 8 (Returned)

-- Thêm Chi tiết hóa đơn bán hàng
INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice) VALUES 
(1, 1, 1, 390000),
(1, 3, 1, 1200000),
(2, 11, 1, 3400000),
(3, 9, 1, 2500000),
(4, 8, 2, 350000),
(5, 6, 1, 6200000),
(6, 4, 1, 1990000),
(7, 10, 1, 1450000),
(8, 2, 3, 550000);
GO