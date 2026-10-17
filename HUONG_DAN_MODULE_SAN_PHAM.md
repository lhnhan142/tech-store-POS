# Module 2: Quản lý Sản phẩm & Tiện ích dữ liệu (KAN-6)

## Làm gì
- **Sản phẩm**: xem / tìm (không dấu) / lọc theo danh mục + thương hiệu, thêm, sửa, xóa, chọn ảnh.
- **Danh mục** và **Thương hiệu**: thêm, sửa, xóa (chặn xóa nếu còn sản phẩm).
- **Nhập Excel (KAN-17)**: kiểm tra từng dòng, dòng lỗi bị bỏ qua + báo "Dòng X: lý do", dòng đúng vẫn được lưu.
- **Xuất Excel (KAN-18)** + **Tải file mẫu**.

## Cài đặt (làm theo thứ tự)
1. **CSDL mới**: chạy `PhuKienMayTinhDB.sql` (đã có bảng Brands + cột Products.BrandId).
   **CSDL cũ đã có dữ liệu**: chạy `Upgrade_KAN-Product_Brands.sql` (chạy lại nhiều lần vẫn an toàn).
2. Mở solution, build lại (Visual Studio tự restore package **ClosedXML** cho project Common).
3. Chạy app -> menu **Kho** -> Sản phẩm / Danh mục / Thương hiệu.

## File Excel nhập (dòng 1 là tiêu đề)
| Tên sản phẩm* | Mã vạch | Danh mục* | Thương hiệu | Giá bán* | Hình ảnh |
|---|---|---|---|---|---|
- Danh mục / Thương hiệu phải **đã có sẵn** (tên không phân biệt hoa thường).
- Giá nhận: `350000`, `350,000`, `350.000`, `1.500.000đ`.
- Hình ảnh: chỉ là **tên file** (vd `g304.jpg`) nằm trong `Images\Products`.
- Tồn kho **không** nhập ở đây (thuộc module Kho): sản phẩm mới có tồn = 0.

## File dùng chung đã sửa (có thể đụng code của bạn khác)
- `GUI/Program.cs`: thêm DI cho repository/service/form mới (xem các dòng có Product/Brand/Category).
- `GUI/Forms/Main/MainForm.cs`: dòng "Sản phẩm" giờ mở ProductForm + thêm 2 trang Danh mục, Thương hiệu.
- `DAL/.../IRepository.cs` + `Repository.cs`: thêm `AddRangeAsync`.
- `DAL/Entities/Product.cs`, `DAL/Context/AppDbContext.cs`: thêm Brand.
- `DTO/Common.csproj`: thêm package ClosedXML.

## Lưu ý
- Ảnh chọn từ máy được chép vào `Images\Products` **cạnh file .exe** (bin\Debug\...), cùng chỗ POS đọc ảnh.
  Muốn giữ ảnh khi clean/build lại: chép thêm vào thư mục `GUI/Images/Products` của project.
- Chưa có tài khoản đăng nhập thật nên chưa phân quyền màn hình này (sau này gắn theo role Admin/Quản lý).
