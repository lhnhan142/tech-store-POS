-- ======================================================================================
-- BÀI TẬP LỚN MÔN LẬP TRÌNH TRỰC QUAN (LTTQ) - CỬA HÀNG PHỤ KIỆN MÁY TÍNH & GAMING GEAR
-- HỆ CƠ SỞ DỮ LIỆU: MICROSOFT SQL SERVER
-- TÊN DATABASE: PhuKienMayTinhDb
-- ======================================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PhuKienMayTinhDb')
BEGIN
    CREATE DATABASE [PhuKienMayTinhDb];
END
GO

USE [PhuKienMayTinhDb];
GO

-- 1. BẢNG CẤU HÌNH LƯƠNG (SalaryConfigurations)
IF OBJECT_ID(N'[SalaryConfigurations]', N'U') IS NULL
BEGIN
    CREATE TABLE [SalaryConfigurations] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ConfigName] NVARCHAR(100) NOT NULL,
        [StandardWorkDays] INT NOT NULL DEFAULT 26,
        [StandardWorkHoursPerDay] FLOAT NOT NULL DEFAULT 8.0,
        [OvertimeMultiplier] DECIMAL(5,2) NOT NULL DEFAULT 1.5,
        [LatePenaltyPerMinute] DECIMAL(18,2) NOT NULL DEFAULT 2000,
        [AbsencePenaltyPerDay] DECIMAL(18,2) NOT NULL DEFAULT 200000,
        [DiligenceBonus] DECIMAL(18,2) NOT NULL DEFAULT 500000,
        [TaxExemptionLevel] DECIMAL(18,2) NOT NULL DEFAULT 11000000,
        [InsuranceRate] DECIMAL(5,2) NOT NULL DEFAULT 10.5,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 2. BẢNG NHÂN VIÊN (Employees)
IF OBJECT_ID(N'[Employees]', N'U') IS NULL
BEGIN
    CREATE TABLE [Employees] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeCode] NVARCHAR(20) NOT NULL UNIQUE,
        [FullName] NVARCHAR(100) NOT NULL,
        [Gender] NVARCHAR(10) NOT NULL,
        [DateOfBirth] DATE NOT NULL,
        [IdentityCard] NVARCHAR(20) NULL,
        [PhoneNumber] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [Address] NVARCHAR(250) NULL,
        [Department] NVARCHAR(50) NOT NULL,
        [Position] NVARCHAR(50) NOT NULL,
        [HireDate] DATE NOT NULL,
        [BaseSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [HourlyRate] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [LunchAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TravelAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ResponsibilityAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Status] INT NOT NULL DEFAULT 0, -- 0: Active, 1: OnLeave, 2: Resigned
        [AvatarUrl] NVARCHAR(500) NULL,
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME NULL
    );
END
GO

-- 3. BẢNG CHẤM CÔNG (Attendances)
IF OBJECT_ID(N'[Attendances]', N'U') IS NULL
BEGIN
    CREATE TABLE [Attendances] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeId] INT NOT NULL,
        [WorkDate] DATE NOT NULL,
        [Shift] INT NOT NULL, -- 0: Morning, 1: Afternoon, 2: Evening, 3: FullDay
        [CheckInTime] DATETIME NULL,
        [CheckOutTime] DATETIME NULL,
        [HoursWorked] FLOAT NOT NULL DEFAULT 0,
        [LateMinutes] INT NOT NULL DEFAULT 0,
        [EarlyLeaveMinutes] INT NOT NULL DEFAULT 0,
        [Status] INT NOT NULL DEFAULT 0, -- 0: Present, 1: Late, 2: EarlyLeave, 3: ExcusedAbsence, 4: UnexcusedAbsence, 5: Holiday
        [Note] NVARCHAR(250) NULL,
        [IsAdjusted] BIT NOT NULL DEFAULT 0,
        [AdjustedBy] NVARCHAR(100) NULL,
        [AdjustedAt] DATETIME NULL,
        [AdjustmentReason] NVARCHAR(250) NULL,
        CONSTRAINT [FK_Attendances_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id]) ON DELETE CASCADE
    );
END
GO

-- 4. BẢNG BẢNG LƯƠNG (Payrolls)
IF OBJECT_ID(N'[Payrolls]', N'U') IS NULL
BEGIN
    CREATE TABLE [Payrolls] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PayrollCode] NVARCHAR(30) NOT NULL UNIQUE,
        [Month] INT NOT NULL,
        [Year] INT NOT NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [Status] INT NOT NULL DEFAULT 0, -- 0: Draft, 1: Approved, 2: Paid
        [ApprovedBy] NVARCHAR(100) NULL,
        [ApprovedDate] DATETIME NULL,
        [TotalBaseSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalBonus] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalDeductions] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalNetSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Note] NVARCHAR(500) NULL
    );
END
GO

-- 5. BẢNG CHI TIẾT BẢNG LƯƠNG (PayrollDetails)
IF OBJECT_ID(N'[PayrollDetails]', N'U') IS NULL
BEGIN
    CREATE TABLE [PayrollDetails] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PayrollId] INT NOT NULL,
        [EmployeeId] INT NOT NULL,
        [StandardDays] FLOAT NOT NULL DEFAULT 26,
        [ActualWorkDays] FLOAT NOT NULL DEFAULT 0,
        [TotalHours] FLOAT NOT NULL DEFAULT 0,
        [TotalLateMinutes] INT NOT NULL DEFAULT 0,
        [TotalLateCount] INT NOT NULL DEFAULT 0,
        [TotalAbsenceCount] INT NOT NULL DEFAULT 0,
        [BaseSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ActualBaseSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [LunchAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TravelAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ResponsibilityAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalAllowance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [DiligenceBonus] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [PerformanceBonus] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalBonus] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [LatePenalty] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [AbsencePenalty] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [InsuranceDeduction] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [OtherDeductions] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalDeductions] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [NetSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Notes] NVARCHAR(250) NULL,
        CONSTRAINT [FK_PayrollDetails_Payrolls] FOREIGN KEY ([PayrollId]) REFERENCES [Payrolls]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PayrollDetails_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id]) ON DELETE NO ACTION
    );
END
GO

-- 6. BẢNG SẢN PHẨM (Products)
IF OBJECT_ID(N'[Products]', N'U') IS NULL
BEGIN
    CREATE TABLE [Products] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ProductCode] NVARCHAR(30) NOT NULL UNIQUE,
        [Name] NVARCHAR(150) NOT NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [Brand] NVARCHAR(50) NOT NULL,
        [CostPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [SellingPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [StockQuantity] INT NOT NULL DEFAULT 0,
        [Unit] NVARCHAR(20) NOT NULL DEFAULT N'Chiếc',
        [IsActive] BIT NOT NULL DEFAULT 1
    );
END
GO

-- 7. BẢNG ĐƠN HÀNG (Orders)
IF OBJECT_ID(N'[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [Orders] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderCode] NVARCHAR(30) NOT NULL UNIQUE,
        [CustomerName] NVARCHAR(100) NOT NULL,
        [CustomerPhone] NVARCHAR(20) NULL,
        [OrderDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [DiscountAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [FinalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [PaymentMethod] NVARCHAR(50) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [EmployeeId] INT NULL,
        CONSTRAINT [FK_Orders_Employees] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id]) ON DELETE SET NULL
    );
END
GO

-- 8. BẢNG CHI TIẾT ĐƠN HÀNG (OrderDetails)
IF OBJECT_ID(N'[OrderDetails]', N'U') IS NULL
BEGIN
    CREATE TABLE [OrderDetails] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [Quantity] INT NOT NULL DEFAULT 1,
        [UnitPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [TotalPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT [FK_OrderDetails_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE NO ACTION
    );
END
GO

-- ======================================================================================
-- NẠP DỮ LIỆU MẪU BAN ĐẦU (SEED DATA)
-- ======================================================================================

-- 1. Cấu hình lương 2026
IF NOT EXISTS (SELECT 1 FROM [SalaryConfigurations])
BEGIN
    INSERT INTO [SalaryConfigurations] 
    ([ConfigName], [StandardWorkDays], [StandardWorkHoursPerDay], [OvertimeMultiplier], [LatePenaltyPerMinute], [AbsencePenaltyPerDay], [DiligenceBonus], [TaxExemptionLevel], [InsuranceRate], [IsActive], [CreatedAt])
    VALUES 
    (N'Quy chuẩn tính lương 2026', 26, 8.0, 1.5, 2000, 200000, 500000, 11000000, 10.5, 1, GETDATE());
END
GO

-- 2. Danh sách nhân viên
IF NOT EXISTS (SELECT 1 FROM [Employees])
BEGIN
    INSERT INTO [Employees] 
    ([EmployeeCode], [FullName], [Gender], [DateOfBirth], [IdentityCard], [PhoneNumber], [Email], [Address], [Department], [Position], [HireDate], [BaseSalary], [HourlyRate], [LunchAllowance], [TravelAllowance], [ResponsibilityAllowance], [Status], [Notes], [CreatedAt])
    VALUES
    (N'NV001', N'Nguyễn Văn An', N'Nam', '1992-04-15', N'001092004561', N'0982123456', N'an.nguyen@gearcomputervn.com', N'Số 12 Dịch Vọng Hậu, Cầu Giấy, Hà Nội', N'Quản lý', N'Cửa hàng trưởng', '2022-01-10', 18000000, 75000, 1000000, 500000, 3000000, 0, N'Quản lý toàn bộ hoạt động kinh doanh và nhân sự', GETDATE()),
    (N'NV002', N'Trần Thị Mai', N'Nữ', '1995-08-22', N'001195007892', N'0973456789', N'mai.tran@gearcomputervn.com', N'24 Hồ Tùng Mậu, Mai Dịch, Cầu Giấy, Hà Nội', N'Kế toán', N'Kế toán trưởng', '2022-03-01', 13500000, 58000, 800000, 400000, 1500000, 0, N'Phụ trách thu chi, sổ sách và tiền lương', GETDATE()),
    (N'NV003', N'Lê Hoàng Long', N'Nam', '1997-11-05', N'001097003456', N'0912345678', N'long.le@gearcomputervn.com', N'88 Nguyễn Khánh Toàn, Quan Hoa, Cầu Giấy, Hà Nội', N'Bán hàng', N'Trưởng nhóm Bán hàng', '2023-02-15', 10500000, 45000, 750000, 350000, 1000000, 0, N'Quản lý ca bán hàng và hỗ trợ khách hàng VIP', GETDATE()),
    (N'NV004', N'Phạm Quỳnh Trang', N'Nữ', '2000-03-18', N'001200009123', N'0934567890', N'trang.pham@gearcomputervn.com', N'15 Xuân Thủy, Dịch Vọng, Cầu Giấy, Hà Nội', N'Bán hàng', N'Nhân viên Bán hàng (POS)', '2023-06-01', 8000000, 35000, 730000, 300000, 0, 0, N'Nhân viên bán hàng chính tại quầy POS', GETDATE()),
    (N'NV005', N'Vũ Tuấn Kiệt', N'Nam', '1999-07-12', N'001099008234', N'0965678901', N'kiet.vu@gearcomputervn.com', N'45 Chùa Láng, Đống Đa, Hà Nội', N'Bán hàng', N'Nhân viên Bán hàng', '2023-09-10', 7800000, 35000, 730000, 300000, 0, 0, N'Tư vấn cấu hình máy tính và gaming gear', GETDATE()),
    (N'NV006', N'Hoàng Minh Đức', N'Nam', '1996-05-29', N'001096001290', N'0978901234', N'duc.hoang@gearcomputervn.com', N'102 Hoàng Quốc Việt, Cầu Giấy, Hà Nội', N'Kho', N'Thủ kho', '2022-08-20', 10000000, 43000, 800000, 400000, 800000, 0, N'Phụ trách nhập xuất và kiểm kê tồn kho', GETDATE()),
    (N'NV007', N'Đỗ Duy Mạnh', N'Nam', '2001-10-08', N'001201004567', N'0989012345', N'manh.do@gearcomputervn.com', N'67 Cầu Diễn, Bắc Từ Liêm, Hà Nội', N'Kho', N'Nhân viên kho', '2024-01-15', 7500000, 33000, 730000, 300000, 0, 0, N'Sắp xếp và dán tem mã vạch sản phẩm', GETDATE()),
    (N'NV008', N'Bùi Đình Trọng', N'Nam', '1998-02-14', N'001098007654', N'0901234567', N'trong.bui@gearcomputervn.com', N'29 Láng Hạ, Ba Đình, Hà Nội', N'Kỹ thuật', N'Kỹ thuật viên Trưởng', '2022-11-01', 11500000, 50000, 800000, 400000, 1000000, 0, N'Bảo hành, test thiết bị phụ kiện và đổi trả', GETDATE()),
    (N'NV009', N'Ngô Thị Thu Hà', N'Nữ', '2002-06-25', N'001202008912', N'0918765432', N'ha.ngo@gearcomputervn.com', N'78 Trần Thái Tông, Dịch Vọng Hậu, Cầu Giấy, Hà Nội', N'Bán hàng', N'Nhân viên Bán hàng', '2024-03-01', 7500000, 33000, 730000, 300000, 0, 0, N'Nhân viên bán hàng trực tuyến và hỗ trợ hotline', GETDATE()),
    (N'NV010', N'Lý Minh Triết', N'Nam', '1997-09-30', N'001097009876', N'0945671234', N'triet.ly@gearcomputervn.com', N'12 Nguyễn Trãi, Thanh Xuân, Hà Nội', N'Kỹ thuật', N'Kỹ thuật viên', '2023-07-15', 9000000, 40000, 750000, 350000, 0, 0, N'Lắp ráp pad chuột, mod phím cơ và thay switch', GETDATE()),
    (N'NV011', N'Đặng Phương Linh', N'Nữ', '2001-12-10', N'001201006543', N'0936789123', N'linh.dang@gearcomputervn.com', N'56 Thái Hà, Đống Đa, Hà Nội', N'Bán hàng', N'Nhân viên CSKH', '2024-04-10', 7500000, 33000, 730000, 300000, 0, 0, N'Chăm sóc khách hàng sau bán hàng và khảo sát ý kiến', GETDATE()),
    (N'NV012', N'Võ Văn Hùng', N'Nam', '1994-01-20', N'001094002345', N'0923456789', N'hung.vo@gearcomputervn.com', N'33 Kim Mã, Ba Đình, Hà Nội', N'Bán hàng', N'Nhân viên Bán hàng', '2023-05-20', 7500000, 33000, 730000, 300000, 0, 1, N'Tạm nghỉ phép ngắn hạn', GETDATE());
END
GO

-- 3. Danh mục sản phẩm phụ kiện & Gaming gear
IF NOT EXISTS (SELECT 1 FROM [Products])
BEGIN
    INSERT INTO [Products] 
    ([ProductCode], [Name], [Category], [Brand], [CostPrice], [SellingPrice], [StockQuantity], [Unit], [IsActive])
    VALUES
    (N'SP001', N'Chuột Logitech G Pro X Superlight Wireless', N'Chuột', N'Logitech', 2300000, 2990000, 35, N'Chiếc', 1),
    (N'SP002', N'Chuột Razer DeathAdder V3 Pro', N'Chuột', N'Razer', 2700000, 3450000, 22, N'Chiếc', 1),
    (N'SP003', N'Chuột Pulsar X2V2 Wireless', N'Chuột', N'Pulsar', 1800000, 2390000, 18, N'Chiếc', 1),
    (N'SP004', N'Bàn phím cơ Akko 3087 v2 Steam Engine', N'Bàn phím', N'Akko', 950000, 1290000, 45, N'Chiếc', 1),
    (N'SP005', N'Bàn phím cơ FL-Esports CMK75 Dual Core', N'Bàn phím', N'FL-Esports', 1600000, 2190000, 28, N'Chiếc', 1),
    (N'SP006', N'Bàn phím MonsGeek M1W SP V3 RGB', N'Bàn phím', N'MonsGeek', 1900000, 2490000, 15, N'Chiếc', 1),
    (N'SP007', N'Tai nghe HyperX Cloud II Wireless', N'Tai nghe', N'HyperX', 2200000, 2890000, 30, N'Chiếc', 1),
    (N'SP008', N'Tai nghe Razer BlackShark V2 Pro', N'Tai nghe', N'Razer', 3100000, 3990000, 12, N'Chiếc', 1),
    (N'SP009', N'Tai nghe Logitech G733 LIGHTSPEED', N'Tai nghe', N'Logitech', 2400000, 3150000, 25, N'Chiếc', 1),
    (N'SP010', N'Lót chuột SteelSeries QcK Heavy XXL', N'Lót chuột', N'SteelSeries', 450000, 690000, 60, N'Tấm', 1),
    (N'SP011', N'Lót chuột Artisan Hayate Otsu FX Soft', N'Lót chuột', N'Artisan', 1200000, 1650000, 8, N'Tấm', 1),
    (N'SP012', N'Giá treo tai nghe RGB Corsair ST100', N'Phụ kiện', N'Corsair', 900000, 1350000, 6, N'Chiếc', 1),
    (N'SP013', N'Dây cáp bọc dù Custom Type-C Coiled Cable', N'Cáp kết nối', N'Custom Gear', 250000, 390000, 50, N'Sợi', 1),
    (N'SP014', N'Webcam Logitech C920 Pro HD 1080p', N'Webcam', N'Logitech', 1300000, 1790000, 16, N'Chiếc', 1),
    (N'SP015', N'Micro thu âm HyperX QuadCast S RGB', N'Micro', N'HyperX', 2800000, 3690000, 5, N'Chiếc', 1);
END
GO

PRINT N'Cơ sở dữ liệu PhuKienMayTinhDb đã được tạo và nạp dữ liệu thành công!';
GO
