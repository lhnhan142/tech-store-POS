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
('admin', '123456', N'Quản trị viên', 1, 0),
('quanly1', '123456', N'Trần Văn Quản', 2, 0),
('banhang1', '123456', N'Nguyễn Thị Bán', 3, 0),
('banhang2', '123456', N'Lê Bán Hàng', 3, 0),
('kho1', '123456', N'Phạm Văn Kho', 4, 0),
('banhang3', '123456', N'Phạm Hoàng Long', 3, 0),
('banhang4', '123456', N'Đỗ Thị Mai', 3, 0),
('kho2', '123456', N'Vũ Ngọc Tuấn', 4, 0),
('kho3', '123456', N'Bùi Thanh Sơn', 4, 1);

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
GO

-- =====================================================================
-- 4. MODULE QUẢN LÝ NHÂN SỰ, CHẤM CÔNG & TÍNH LƯƠNG (MODULE 8 & 9)
-- =====================================================================

-- Bảng Employees (Hồ sơ nhân viên)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE Employees (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeCode NVARCHAR(20) NOT NULL UNIQUE,
        FullName NVARCHAR(100) NOT NULL,
        DateOfBirth DATETIME2 NULL,
        Gender NVARCHAR(10) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Address NVARCHAR(250) NULL,
        Position NVARCHAR(50) NOT NULL,
        HireDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        BaseSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
        Allowance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status INT NOT NULL DEFAULT 0, -- 0: Active, 1: OnLeave, 2: Resigned
        UserId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Employees_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_Employees_EmployeeCode ON Employees(EmployeeCode);
    CREATE INDEX IX_Employees_Position ON Employees(Position);
    CREATE INDEX IX_Employees_Status ON Employees(Status);
END;
GO

-- Cập nhật cấu trúc bảng Attendances nếu chưa có các trường mở rộng
IF COL_LENGTH('Attendances', 'EmployeeId') IS NULL
BEGIN
    ALTER TABLE Attendances ADD EmployeeId INT NULL;
    ALTER TABLE Attendances ADD HoursWorked DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE Attendances ADD OvertimeHours DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE Attendances ADD Status INT NOT NULL DEFAULT 0; -- 0: Present, 1: Late, 2: EarlyLeave, 3: Overtime, 4: Absent
    ALTER TABLE Attendances ADD Note NVARCHAR(250) NULL;
    ALTER TABLE Attendances ADD CONSTRAINT FK_Attendances_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);
END;
GO

-- Bảng Payrolls (Bảng lương tháng)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payrolls')
BEGIN
    CREATE TABLE Payrolls (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PayrollCode NVARCHAR(30) NOT NULL UNIQUE,
        Month INT NOT NULL,
        Year INT NOT NULL,
        StandardWorkDays INT NOT NULL DEFAULT 26,
        TotalGrossAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalNetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalEmployees INT NOT NULL DEFAULT 0,
        Status INT NOT NULL DEFAULT 0, -- 0: Draft, 1: Finalized
        Note NVARCHAR(250) NULL,
        CreatedByUserId INT NOT NULL,
        FinalizedAt DATETIME2 NULL,
        FinalizedByUserId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Payrolls_CreatedByUser FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
        CONSTRAINT FK_Payrolls_FinalizedByUser FOREIGN KEY (FinalizedByUserId) REFERENCES Users(Id),
        CONSTRAINT UQ_Payrolls_Month_Year UNIQUE (Month, Year)
    );
END;
GO

-- Bảng PayrollDetails (Chi tiết lương nhân viên)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayrollDetails')
BEGIN
    CREATE TABLE PayrollDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PayrollId INT NOT NULL,
        EmployeeId INT NOT NULL,
        EmployeeCode NVARCHAR(20) NOT NULL,
        EmployeeName NVARCHAR(100) NOT NULL,
        Position NVARCHAR(50) NOT NULL,
        BaseSalary DECIMAL(18,2) NOT NULL,
        StandardWorkDays INT NOT NULL DEFAULT 26,
        ActualWorkDays DECIMAL(5,2) NOT NULL DEFAULT 0,
        TotalHoursWorked DECIMAL(6,2) NOT NULL DEFAULT 0,
        OvertimeHours DECIMAL(5,2) NOT NULL DEFAULT 0,
        SalaryByWorkDays DECIMAL(18,2) NOT NULL DEFAULT 0,
        OvertimePay DECIMAL(18,2) NOT NULL DEFAULT 0,
        Allowance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Bonus DECIMAL(18,2) NOT NULL DEFAULT 0,
        Deductions DECIMAL(18,2) NOT NULL DEFAULT 0,
        NetSalary DECIMAL(18,2) NOT NULL DEFAULT 0,
        Note NVARCHAR(250) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_PayrollDetails_Payrolls FOREIGN KEY (PayrollId) REFERENCES Payrolls(Id) ON DELETE CASCADE,
        CONSTRAINT FK_PayrollDetails_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
        CONSTRAINT UQ_PayrollDetails_Payroll_Employee UNIQUE (PayrollId, EmployeeId)
    );
END;
GO

-- =====================================================================
-- 5. MOCK DATA NHÂN VIÊN, CHẤM CÔNG & BẢNG LƯƠNG
-- =====================================================================

-- Thêm Hồ sơ nhân viên tương ứng với 9 tài khoản User có sẵn
IF NOT EXISTS (SELECT * FROM Employees WHERE EmployeeCode = 'NV001')
BEGIN
    INSERT INTO Employees (EmployeeCode, FullName, DateOfBirth, Gender, PhoneNumber, Email, Address, Position, HireDate, BaseSalary, Allowance, Status, UserId)
    VALUES
    ('NV001', N'Quản trị viên', '1990-01-15', N'Nam', '0909000001', 'admin@techstore.vn', N'Quận 1, TP.HCM', N'Giám đốc điều hành', '2023-01-01', 25000000, 3000000, 0, 1),
    ('NV002', N'Trần Văn Quản', '1993-05-20', N'Nam', '0909000002', 'quanly1@techstore.vn', N'Quận 3, TP.HCM', N'Quản lý cửa hàng', '2023-03-15', 18000000, 2000000, 0, 2),
    ('NV003', N'Nguyễn Thị Bán', '1998-08-10', N'Nữ', '0909000003', 'banhang1@techstore.vn', N'Bình Thạnh, TP.HCM', N'Nhân viên bán hàng', '2023-06-01', 8500000, 1000000, 0, 3),
    ('NV004', N'Lê Bán Hàng', '1999-11-25', N'Nam', '0909000004', 'banhang2@techstore.vn', N'Phú Nhuận, TP.HCM', N'Nhân viên bán hàng', '2023-08-10', 8000000, 1000000, 0, 4),
    ('NV005', N'Phạm Văn Kho', '1995-02-18', N'Nam', '0909000005', 'kho1@techstore.vn', N'Tân Bình, TP.HCM', N'Thủ kho trưởng', '2023-04-01', 11000000, 1500000, 0, 5),
    ('NV006', N'Phạm Hoàng Long', '1997-07-14', N'Nam', '0909000006', 'banhang3@techstore.vn', N'Gò Vấp, TP.HCM', N'Nhân viên bán hàng', '2023-09-01', 8500000, 1000000, 0, 6),
    ('NV007', N'Đỗ Thị Mai', '2000-03-08', N'Nữ', '0909000007', 'banhang4@techstore.vn', N'Quận 10, TP.HCM', N'Nhân viên thu ngân', '2024-01-15', 8000000, 800000, 0, 7),
    ('NV008', N'Vũ Ngọc Tuấn', '1996-09-30', N'Nam', '0909000008', 'kho2@techstore.vn', N'Quận 12, TP.HCM', N'Nhân viên kho', '2023-10-01', 9000000, 1000000, 0, 8),
    ('NV009', N'Bùi Thanh Sơn', '1994-12-12', N'Nam', '0909000009', 'kho3@techstore.vn', N'Tân Phú, TP.HCM', N'Nhân viên kho', '2023-05-15', 9000000, 1000000, 1, 9);
END;
GO

-- Cập nhật dữ liệu mẫu Chấm công cho tháng hiện tại
IF NOT EXISTS (SELECT * FROM Attendances WHERE EmployeeId IS NOT NULL)
BEGIN
    -- Gán EmployeeId cho các bản ghi cũ nếu có
    UPDATE a SET a.EmployeeId = e.Id
    FROM Attendances a JOIN Employees e ON a.UserId = e.UserId;

    -- Thêm chấm công mẫu cho các ngày gần đây
    INSERT INTO Attendances (UserId, EmployeeId, Date, CheckIn, CheckOut, HoursWorked, OvertimeHours, Status, Note)
    VALUES
    (3, 3, CAST(GETDATE() AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (4, 4, CAST(GETDATE() AS DATE), '08:15', '17:00', 7.75, 0, 1, N'Đi muộn 15p'),
    (5, 5, CAST(GETDATE() AS DATE), '07:55', '19:00', 9.0, 2.0, 3, N'Tăng ca kiểm kho'),
    (6, 6, CAST(GETDATE() AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (7, 7, CAST(GETDATE() AS DATE), '08:05', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (8, 8, CAST(GETDATE() AS DATE), '07:50', '17:00', 8.0, 0, 0, N'Đúng giờ'),

    -- Ngày hôm qua
    (3, 3, CAST(DATEADD(day, -1, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (4, 4, CAST(DATEADD(day, -1, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (5, 5, CAST(DATEADD(day, -1, GETDATE()) AS DATE), '08:00', '17:30', 8.5, 0.5, 3, N'Tăng ca nhập hàng'),
    (6, 6, CAST(DATEADD(day, -1, GETDATE()) AS DATE), '08:00', '16:00', 7.0, 0, 2, N'Về sớm việc gia đình'),
    (7, 7, CAST(DATEADD(day, -1, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (8, 8, CAST(DATEADD(day, -1, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),

    -- Ngày hôm kia
    (3, 3, CAST(DATEADD(day, -2, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (4, 4, CAST(DATEADD(day, -2, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (5, 5, CAST(DATEADD(day, -2, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (6, 6, CAST(DATEADD(day, -2, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (7, 7, CAST(DATEADD(day, -2, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ'),
    (8, 8, CAST(DATEADD(day, -2, GETDATE()) AS DATE), '08:00', '17:00', 8.0, 0, 0, N'Đúng giờ');
END;
GO


