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

-- Bảng Brands (Thương hiệu)
CREATE TABLE Brands (
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
    BrandId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES Brands(Id)
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

-- Bảng StockIssues (Phiếu xuất kho)
CREATE TABLE StockIssues (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    IssueDate DATETIME2 NOT NULL,
    Reason NVARCHAR(100) NOT NULL,
    Note NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_StockIssues_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Bảng StockIssueDetails (Chi tiết xuất kho)
CREATE TABLE StockIssueDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IssueId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    CONSTRAINT FK_StockIssueDetails_Issues FOREIGN KEY (IssueId) REFERENCES StockIssues(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockIssueDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Bảng StockTakes (Phiếu kiểm kê)
CREATE TABLE StockTakes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    TakeDate DATETIME2 NOT NULL,
    Note NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_StockTakes_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Bảng StockTakeDetails (Chi tiết kiểm kê)
CREATE TABLE StockTakeDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StockTakeId INT NOT NULL,
    ProductId INT NOT NULL,
    SystemQuantity INT NOT NULL,
    ActualQuantity INT NOT NULL,
    CONSTRAINT FK_StockTakeDetails_StockTakes FOREIGN KEY (StockTakeId) REFERENCES StockTakes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockTakeDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
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
('admin', 'hashed_pw_123', N'Quản trị viên', 1, 0),
('quanly1', 'hashed_pw_123', N'Trần Văn Quản', 2, 0),
('banhang1', 'hashed_pw_123', N'Nguyễn Thị Bán', 3, 0),
('banhang2', 'hashed_pw_123', N'Lê Bán Hàng', 3, 0),
('kho1', 'hashed_pw_123', N'Phạm Văn Kho', 4, 0),
('banhang3', 'hashed_pw_123', N'Phạm Hoàng Long', 3, 0),
('banhang4', 'hashed_pw_123', N'Đỗ Thị Mai', 3, 0),
('kho2', 'hashed_pw_123', N'Vũ Ngọc Tuấn', 4, 0),
('kho3', 'hashed_pw_123', N'Bùi Thanh Sơn', 4, 1);

-- Thêm Danh mục
INSERT INTO Categories (Name) VALUES 
(N'Chuột máy tính'), (N'Bàn phím'), (N'Tai nghe'), (N'Màn hình'), (N'Lót chuột'),
(N'Linh kiện PC (RAM, Ổ cứng)'), (N'Vỏ Case & Tản nhiệt'), (N'Balo & Túi chống sốc'), 
(N'Webcam & Microphone'), (N'Ghế & Bàn Gaming');

-- Thêm Thương hiệu
INSERT INTO Brands (Name) VALUES
(N'Logitech'), (N'Razer'), (N'Akko'), (N'DareU'), (N'HyperX'), (N'Sony'),
(N'LG'), (N'Corsair'), (N'Samsung'), (N'Targus'), (N'DXRacer');

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

-- Gán BrandId cho sản phẩm 
UPDATE p SET BrandId = b.Id
FROM Products p JOIN Brands b ON p.Name LIKE N'%' + b.Name + N'%';

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
(1, 5, GETDATE(), 19500000), 
(3, 5, DATEADD(day, -1, GETDATE()), 35000000),
(4, 8, DATEADD(day, -2, GETDATE()), 85000000);

-- Thêm Chi tiết nhập kho
INSERT INTO InventoryReceiptDetails (ReceiptId, ProductId, Quantity, UnitPrice) VALUES 
(1, 1, 50, 390000),
(2, 10, 10, 1450000),
(2, 11, 5, 2300000),
(3, 13, 5, 8500000);

-- Thêm Hóa đơn bán hàng (Orders)
INSERT INTO Orders (CustomerId, UserId, OrderDate, TotalAmount, DiscountAmount, Status, PaymentMethod) VALUES 
(1, 3, GETDATE(), 1590000, 0, 1, 0), 
(3, 3, GETDATE(), 3400000, 0, 1, 1), 
(4, 6, GETDATE(), 2500000, 100000, 1, 2), 
(4, 3, DATEADD(day, -1, GETDATE()), 750000, 0, 1, 0), 
(2, 6, DATEADD(day, -1, GETDATE()), 6200000, 200000, 1, 1), 
(5, 7, GETDATE(), 1990000, 0, 0, 1), 
(1, 3, DATEADD(day, -2, GETDATE()), 1450000, 0, 2, 0), 
(3, 6, DATEADD(day, -3, GETDATE()), 1650000, 0, 3, 2); 

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


-- =====================================================================
-- 3. MODULE QUẢN LÝ HÓA ĐƠN & ĐỔI TRẢ (INVOICES & RETURNS)
-- =====================================================================

-- Bảng ReturnPolicies (Chính sách đổi trả cấu hình được)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReturnPolicies')
BEGIN
    CREATE TABLE ReturnPolicies (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ReturnWindowDays INT NOT NULL DEFAULT 7,
        ApprovalThresholdAmount DECIMAL(15,2) NOT NULL DEFAULT 5000000.00,
        IsActive BIT NOT NULL DEFAULT 1,
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedByUserId INT NULL,
        CONSTRAINT FK_ReturnPolicies_Users FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
    );

    INSERT INTO ReturnPolicies (ReturnWindowDays, ApprovalThresholdAmount, IsActive)
    VALUES (7, 5000000.00, 1);
END;

-- Bảng Invoices (Hóa đơn bán lẻ & đổi hàng)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNo NVARCHAR(50) NOT NULL UNIQUE,
        OrderId INT NULL,
        CustomerId INT NULL,
        CreatedByUserId INT NOT NULL,
        Status NVARCHAR(30) NOT NULL,
        Type NVARCHAR(30) NOT NULL,
        ParentInvoiceId INT NULL,
        SubTotal DECIMAL(15,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(15,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(15,2) NOT NULL DEFAULT 0,
        CancelReason NVARCHAR(500) NULL,
        CancelledAt DATETIME2 NULL,
        CancelledByUserId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_Invoices_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Invoices_CreatedByUser FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
        CONSTRAINT FK_Invoices_CancelledByUser FOREIGN KEY (CancelledByUserId) REFERENCES Users(Id),
        CONSTRAINT FK_Invoices_ParentInvoice FOREIGN KEY (ParentInvoiceId) REFERENCES Invoices(Id)
    );

    CREATE INDEX IX_Invoices_CreatedAt ON Invoices(CreatedAt);
    CREATE INDEX IX_Invoices_CustomerId ON Invoices(CustomerId);
    CREATE INDEX IX_Invoices_Status ON Invoices(Status);
END;

-- Bảng InvoiceItems (Chi tiết dòng hàng trong hóa đơn)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceItems')
BEGIN
    CREATE TABLE InvoiceItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceId INT NOT NULL,
        ProductId INT NOT NULL,
        ProductName NVARCHAR(250) NOT NULL,
        Qty INT NOT NULL,
        UnitPrice DECIMAL(15,2) NOT NULL,
        DiscountAlloc DECIMAL(15,2) NOT NULL DEFAULT 0,
        LineTotal DECIMAL(15,2) NOT NULL,
        ReturnedQty INT NOT NULL DEFAULT 0,
        IsReturnable BIT NOT NULL DEFAULT 1,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT CK_InvoiceItem_ReturnedQty CHECK (ReturnedQty <= Qty),
        CONSTRAINT FK_InvoiceItems_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
        CONSTRAINT FK_InvoiceItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
    );
END;

-- Bảng InvoicePayments (Bản ghi thanh toán hóa đơn)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoicePayments')
BEGIN
    CREATE TABLE InvoicePayments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceId INT NOT NULL,
        Method NVARCHAR(30) NOT NULL,
        Amount DECIMAL(15,2) NOT NULL,
        TransactionCode NVARCHAR(100) NULL,
        Note NVARCHAR(500) NULL,
        PaidAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_InvoicePayments_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
    );
END;

-- Bảng ReturnRequests (Yêu cầu đổi trả hàng)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReturnRequests')
BEGIN
    CREATE TABLE ReturnRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RequestNo NVARCHAR(50) NOT NULL UNIQUE,
        InvoiceId INT NOT NULL,
        Type NVARCHAR(30) NOT NULL,
        Channel NVARCHAR(30) NOT NULL,
        Status NVARCHAR(30) NOT NULL,
        ReasonCode NVARCHAR(50) NULL,
        ReasonNote NVARCHAR(500) NULL,
        TotalRefund DECIMAL(15,2) NOT NULL DEFAULT 0,
        ExchangeInvoiceId INT NULL,
        CreatedByUserId INT NOT NULL,
        ApprovedByUserId INT NULL,
        ApprovedAt DATETIME2 NULL,
        ReceivedByUserId INT NULL,
        ReceivedAt DATETIME2 NULL,
        RejectReason NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_ReturnRequests_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ReturnRequests_ExchangeInvoice FOREIGN KEY (ExchangeInvoiceId) REFERENCES Invoices(Id),
        CONSTRAINT FK_ReturnRequests_CreatedByUser FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
        CONSTRAINT FK_ReturnRequests_ApprovedByUser FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id),
        CONSTRAINT FK_ReturnRequests_ReceivedByUser FOREIGN KEY (ReceivedByUserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_ReturnRequests_InvoiceId ON ReturnRequests(InvoiceId);
    CREATE INDEX IX_ReturnRequests_Status ON ReturnRequests(Status);
    CREATE INDEX IX_ReturnRequests_CreatedAt ON ReturnRequests(CreatedAt);
END;

-- Bảng ReturnItems (Chi tiết sản phẩm yêu cầu đổi trả)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReturnItems')
BEGIN
    CREATE TABLE ReturnItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ReturnRequestId INT NOT NULL,
        InvoiceItemId INT NOT NULL,
        ProductId INT NOT NULL,
        Qty INT NOT NULL,
        Condition NVARCHAR(30) NOT NULL,
        Restock BIT NOT NULL DEFAULT 1,
        RefundAmount DECIMAL(15,2) NOT NULL DEFAULT 0,
        CONSTRAINT FK_ReturnItems_ReturnRequests FOREIGN KEY (ReturnRequestId) REFERENCES ReturnRequests(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ReturnItems_InvoiceItems FOREIGN KEY (InvoiceItemId) REFERENCES InvoiceItems(Id),
        CONSTRAINT FK_ReturnItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
    );
END;

-- Bảng ReturnAttachments (File/Ảnh đính kèm yêu cầu)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReturnAttachments')
BEGIN
    CREATE TABLE ReturnAttachments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ReturnRequestId INT NOT NULL,
        FilePath NVARCHAR(MAX) NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        UploadedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ReturnAttachments_ReturnRequests FOREIGN KEY (ReturnRequestId) REFERENCES ReturnRequests(Id) ON DELETE CASCADE
    );
END;

-- Bảng Refunds (Bản ghi hoàn tiền)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Refunds')
BEGIN
    CREATE TABLE Refunds (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ReturnRequestId INT NOT NULL,
        InvoicePaymentId INT NULL,
        Method NVARCHAR(30) NOT NULL,
        Amount DECIMAL(15,2) NOT NULL,
        RefundedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ProcessedByUserId INT NOT NULL,
        Note NVARCHAR(500) NULL,
        CONSTRAINT FK_Refunds_ReturnRequests FOREIGN KEY (ReturnRequestId) REFERENCES ReturnRequests(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Refunds_InvoicePayments FOREIGN KEY (InvoicePaymentId) REFERENCES InvoicePayments(Id),
        CONSTRAINT FK_Refunds_Users FOREIGN KEY (ProcessedByUserId) REFERENCES Users(Id)
    );
END;

-- Bảng StockMovements (Sổ theo dõi biến động kho)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockMovements')
BEGIN
    CREATE TABLE StockMovements (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        MovementType NVARCHAR(30) NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(15,2) NOT NULL DEFAULT 0,
        ReferenceType NVARCHAR(50) NOT NULL,
        ReferenceId INT NOT NULL,
        Note NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedByUserId INT NOT NULL,
        CONSTRAINT FK_StockMovements_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
        CONSTRAINT FK_StockMovements_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_StockMovements_ProductId ON StockMovements(ProductId);
    CREATE INDEX IX_StockMovements_MovementType ON StockMovements(MovementType);
    CREATE INDEX IX_StockMovements_CreatedAt ON StockMovements(CreatedAt);
    CREATE INDEX IX_StockMovements_Reference ON StockMovements(ReferenceType, ReferenceId);
END;

-- Bảng AuditLogs (Nhật ký kiểm toán thao tác)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Action NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(100) NOT NULL,
        EntityId INT NOT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        UserId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_AuditLogs_Entity ON AuditLogs(EntityType, EntityId);
    CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
END;

-- Dữ liệu mẫu ban đầu cho Hóa đơn (Invoices & InvoiceItems)
IF NOT EXISTS (SELECT * FROM Invoices WHERE InvoiceNo = 'HD-2026-0001')
BEGIN
    INSERT INTO Invoices (InvoiceNo, OrderId, CustomerId, CreatedByUserId, Status, Type, SubTotal, DiscountAmount, TotalAmount, CreatedAt)
    VALUES 
    ('HD-2026-0001', 1, 1, 3, 'Paid', 'Sale', 1590000, 0, 1590000, DATEADD(day, -2, GETDATE())),
    ('HD-2026-0002', 2, 3, 3, 'Paid', 'Sale', 3400000, 0, 3400000, DATEADD(day, -1, GETDATE())),
    ('HD-2026-0003', 3, 4, 6, 'Paid', 'Sale', 2500000, 100000, 2400000, GETDATE());

    INSERT INTO InvoiceItems (InvoiceId, ProductId, ProductName, Qty, UnitPrice, DiscountAlloc, LineTotal, ReturnedQty, IsReturnable)
    VALUES
    (1, 1, N'Chuột Logitech G102', 1, 390000, 0, 390000, 0, 1),
    (1, 3, N'Bàn phím cơ Akko 3087', 1, 1200000, 0, 1200000, 0, 1),
    (2, 11, N'Ổ cứng SSD Samsung 980 PRO 1TB PCIe 4.0', 1, 3400000, 0, 3400000, 0, 1),
    (3, 9, N'Chuột không dây Logitech MX Master 3S', 1, 2500000, 100000, 2400000, 0, 1);

    INSERT INTO InvoicePayments (InvoiceId, Method, Amount, PaidAt)
    VALUES
    (1, 'Cash', 1590000, DATEADD(day, -2, GETDATE())),
    (2, 'Card', 3400000, DATEADD(day, -1, GETDATE())),
    (3, 'Transfer', 2400000, GETDATE());
END;
