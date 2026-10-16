-- ==========================================================================
-- KAN-20: thêm bảng Phiếu xuất kho + Phiếu kiểm kê cho CSDL ĐÃ TẠO TRƯỚC ĐÓ.
-- Chạy an toàn nhiều lần (bảng nào có rồi thì bỏ qua).
-- CSDL tạo mới từ PhuKienMayTinhDB.sql thì KHÔNG cần chạy file này.
-- ==========================================================================
USE PhuKienMayTinhDB;
GO

IF OBJECT_ID(N'dbo.StockIssues', N'U') IS NULL
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
GO

IF OBJECT_ID(N'dbo.StockIssueDetails', N'U') IS NULL
CREATE TABLE StockIssueDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IssueId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    CONSTRAINT FK_StockIssueDetails_Issues FOREIGN KEY (IssueId) REFERENCES StockIssues(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockIssueDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

IF OBJECT_ID(N'dbo.StockTakes', N'U') IS NULL
CREATE TABLE StockTakes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    TakeDate DATETIME2 NOT NULL,
    Note NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_StockTakes_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

IF OBJECT_ID(N'dbo.StockTakeDetails', N'U') IS NULL
CREATE TABLE StockTakeDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StockTakeId INT NOT NULL,
    ProductId INT NOT NULL,
    SystemQuantity INT NOT NULL,
    ActualQuantity INT NOT NULL,
    CONSTRAINT FK_StockTakeDetails_StockTakes FOREIGN KEY (StockTakeId) REFERENCES StockTakes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockTakeDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO
