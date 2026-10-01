# HỆ THỐNG QUẢN LÝ BÁN PHỤ KIỆN MÁY TÍNH

## 1\. Thông tin chung

* **Nền tảng:** WinForms .NET 10
* **Công nghệ:** Entity Framework Core (SQL Server), Dependency Injection, Repository Pattern, Realtaiizor cho phần tạo UI.
* **Kiến trúc:** 3-Tier (DAL - BLL - GUI) tách Project độc lập.
* **Mục tiêu:** Quản lý toàn diện cửa hàng phụ kiện máy tính theo quy trình nghiệp vụ khép kín từ nhập kho, bán hàng, xuất hóa đơn, đổi trả đến nhân sự và báo cáo.
* **Nhóm người dùng (4 Roles):** Admin, Quản lý, Nhân viên bán hàng, Nhân viên kho.

\---

## 2\. Chuỗi nghiệp vụ cốt lõi

* **Bán hàng \& Kho:** Nhà cung cấp → Nhập kho → Cập nhật tồn kho → Bán hàng (POS) → Thanh toán \& In hóa đơn → Đổi trả (nếu có) → Điều chỉnh kho/doanh thu → Báo cáo doanh thu.
* **Nhân sự \& Lương:** Nhân viên → Chấm công (Check-in/Check-out) → Tổng hợp giờ/công → Tính lương theo cấu hình → Xuất bảng lương.

\---

## 3\. Cấu trúc Solution chuẩn (Architecture)

```text
PhuKienMayTinhSolution (Blank Solution)
│
├── 1. DAL (Data Access Layer - Class Library)
│   ├── Entities/                 # Models map bảng CSDL (User, Product, Order, Inventory...)
│   ├── Context/                  # AppDbContext.cs (Cấu hình EF Core \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\& DbSets)
│   ├── Repositories/
│   │   ├── Interfaces/           # IRepository<T>, IProductRepository...
│   │   └── Implementations/      # Repository<T>, ProductRepository...
│   └── Migrations/               # Thư mục sinh tự động khi chạy Add-Migration
│
├── 2. BLL (Business Logic Layer - Class Library)
│   ├── Services/
│   │   ├── Interfaces/           # IAuthService, IProductService, ISalesService...
│   │   └── Implementations/      # AuthService, ProductService, SalesService...
│   ├── DTOs/                     # DTO truyền dữ liệu giữa BLL và GUI (LoginDTO, OrderDTO...)
│   └── Exceptions/               # Custom Exceptions (OutOfStockException, NotFoundException)
│
├── 3. GUI (Windows Forms App - Project chạy chính)
│   ├── Forms/                    # Phân chia form theo vai trò / phân hệ nghiệp vụ
│   │   ├── Auth/                 # LoginForm.cs, ChangePasswordForm.cs
│   │   ├── Main/                 # MainForm.cs (Khung sườn chứa Menu và Panel điều hướng)
│   │   ├── Sales/                # Form/UC Bán hàng (POS), Hóa đơn, Đổi trả
│   │   ├── Warehouse/            # Form/UC Tồn kho, Nhập kho, Xuất kho, Kiểm kê
│   │   ├── Employee/             # Form/UC Nhân viên, Chấm công, Bảng lương
│   │   └── Reports/              # Form/UC Dashboard, Báo cáo thống kê
│   ├── UserControls/             # Các thành phần tái sử dụng (Card sản phẩm, MenuBar...)
│   ├── Program.cs                # Entry point: Cấu hình DI Container, gọi LoginForm
│   └── appsettings.json          # Chuỗi kết nối Database và cấu hình ứng dụng
│
└── 4. Common (Class Library)
    ├── Constants/                # RoleConstants, MessageConstants
    ├── Enums/                    # OrderStatus, AccountStatus, PaymentMethod
    └── Utilities/                # PasswordHasher, ExcelHelper, PdfHelper



Chào bạn, việc chuyển sang sử dụng thư viện **ReaLTaiizor** mang lại rất nhiều bộ giao diện có sẵn (như Material, Crown, Poison, Hope...) giúp ứng dụng WinForms trở nên cực kỳ hiện đại.

Dưới đây là kế hoạch đã được cập nhật, áp dụng ReaLTaiizor và giữ nguyên quy tắc "mỗi người tự làm giao diện cho module của mình":

### 1. Quy trình thiết kế giao diện bằng ReaLTaiizor (Mỗi người tự làm)

* **Thống nhất Theme chung (Guideline):** Cả team cần họp để thống nhất chọn một bộ giao diện cụ thể trong thư viện ReaLTaiizor (ví dụ: chỉ dùng bộ Material hoặc Cyber) để các form không bị lộn xộn về phong cách. Cần chốt trước mã màu chủ đạo, font chữ và kích thước các control cơ bản.
* **Xây dựng khung Main Form:** Tạo một `MainForm.cs` đóng vai trò làm khung sườn chứa Menu bên trái và Panel điều hướng ở giữa. Khung sườn này dùng các control của ReaLTaiizor và được chia sẻ cho cả nhóm để làm nền tảng chung.


* **Phát triển độc lập & Kiểm soát dữ liệu:** Khi mỗi người tự tạo `UserControls` hoặc `Forms` cho module của mình và kéo thả các control ReaLTaiizor, cần chú ý xử lý tốt các sự kiện trên form (form control event handling). Việc áp dụng các bộ lọc phím (keypress filters) trực tiếp trên giao diện và xác thực chặt chẽ dữ liệu đầu vào (input validation) ngay từ các ô nhập liệu sẽ giúp giảm thiểu lỗi hệ thống trước khi dữ liệu được đẩy xuống tầng xử lý nghiệp vụ (BLL) và cơ sở dữ liệu (DAL).



### 2. Phân chia công việc cho 5 người (Fullstack từng Module)

**Thành viên 1: (Core Hệ thống & Tài khoản)**

* Thiết lập kiến trúc Solution chuẩn 3 lớp, cấu hình Entity Framework Core, kết nối SQL Server và cấu hình Dependency Injection.


* Phát triển Module 1: Đăng nhập, phân quyền, đổi mật khẩu và quản lý tài khoản.


* **Nhiệm vụ UI:** Chịu trách nhiệm tạo khung `MainForm.cs` chuẩn. Tự thiết kế các màn hình Đăng nhập và Phân quyền bằng các control của ReaLTaiizor.



**Thành viên 2: Quản lý Sản phẩm & Tiện ích dữ liệu**

* Phát triển Module 2: Quản lý thông tin sản phẩm, danh mục, thương hiệu và hình ảnh.


* Xử lý logic tính năng Import danh sách từ Excel và Export dữ liệu.


* **Nhiệm vụ UI:** Sử dụng ReaLTaiizor để thiết kế các bảng danh sách sản phẩm và các form thêm/sửa chi tiết.

**Thành viên 3: Quản lý Kho & Nhà cung cấp**

* Phát triển Module 3: Quản lý thông tin nhà cung cấp.


* Phát triển Module 4: Quản lý tồn kho, lập phiếu nhập/xuất kho, kiểm kê và thiết lập cảnh báo hàng sắp hết.


* Tích hợp tính năng tạo và tra cứu sản phẩm bằng mã QR/Barcode.


* **Nhiệm vụ UI:** Thiết kế các màn hình nghiệp vụ kho bằng ReaLTaiizor, đảm bảo hiển thị rõ ràng luồng phiếu nhập/xuất.

**Thành viên 4: Bán hàng (POS) & Khách hàng**

* Phát triển Module 6: Quản lý thông tin khách hàng và lịch sử tích điểm.


* Phát triển Module 5: Xây dựng luồng tạo đơn hàng, quản lý giỏ hàng, áp dụng giảm giá và thanh toán.


* Phát triển Module 7: Quản lý hóa đơn và xử lý các yêu cầu đổi trả từ khách hàng.


* **Nhiệm vụ UI:** Thiết kế giao diện POS bán hàng chuyên nghiệp bằng ReaLTaiizor với các khu vực tìm kiếm, giỏ hàng và thanh toán được bố trí hợp lý.



**Thành viên 5: Nhân sự, Lương & Dashboard Báo cáo**

* Phát triển Module 8 & 9: Quản lý danh sách nhân viên, ghi nhận check-in/out chấm công, thiết lập và tính lương.


* Phát triển Module 10: Xử lý số liệu Dashboard (tổng hợp doanh thu, đơn hàng, tồn kho) và xuất báo cáo thống kê định dạng PDF/Excel.


* **Nhiệm vụ UI:** Kết hợp ReaLTaiizor và công nghệ vẽ GDI+ để thiết kế màn hình Dashboard có các biểu đồ trực quan, cùng các form quản lý bảng công và lương nhân viên.





