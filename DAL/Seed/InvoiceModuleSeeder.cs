using Common.Enums;
using DAL.Context;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Seed
{
    /// <summary>
    /// Nạp dữ liệu mẫu bằng mã C# cho Module Hóa đơn & Đổi trả.
    /// Không dùng HasData để tránh rác migration và cho phép tái sử dụng linh hoạt.
    /// </summary>
    public static class InvoiceModuleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // 1. Seed Chính sách đổi trả mặc định nếu chưa có
            if (!await context.ReturnPolicies.AnyAsync())
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.RoleId == 1);
                context.ReturnPolicies.Add(new ReturnPolicy
                {
                    ReturnWindowDays = 7,
                    ApprovalThresholdAmount = 5_000_000m,
                    IsActive = true,
                    UpdatedAt = DateTime.Now,
                    UpdatedByUserId = adminUser?.Id
                });
                await context.SaveChangesAsync();
            }

            // 2. Kiểm tra xem đã có hóa đơn mẫu chưa
            if (await context.Invoices.AnyAsync())
            {
                return; // Đã seed rồi thì bỏ qua
            }

            // Đảm bảo có người dùng hợp lệ
            var seller = await context.Users.FirstOrDefaultAsync(u => u.RoleId == 3) // Nhân viên bán hàng
                         ?? await context.Users.FirstAsync();
            var manager = await context.Users.FirstOrDefaultAsync(u => u.RoleId == 2) // Quản lý
                          ?? seller;
            var warehouse = await context.Users.FirstOrDefaultAsync(u => u.RoleId == 4) // Thủ kho
                            ?? seller;

            // Đảm bảo có các khách hàng mẫu theo ảnh mockup
            var c1 = await GetOrCreateCustomerAsync(context, "Nguyễn Minh Anh", "0901234568");
            var c2 = await GetOrCreateCustomerAsync(context, "Trần Quốc Bảo", "0902345679");
            var c3 = await GetOrCreateCustomerAsync(context, "Lê Thị Hoa", "0903456780");
            var c4 = await GetOrCreateCustomerAsync(context, "Phạm Văn Đức", "0904567891");
            var c5 = await GetOrCreateCustomerAsync(context, "Võ Thanh Hà", "0905678902");
            var c6 = await GetOrCreateCustomerAsync(context, "Đinh Công Thành", "0912345671");
            var c7 = await GetOrCreateCustomerAsync(context, "Trịnh Công Sơn", "0912345677");
            var c8 = await GetOrCreateCustomerAsync(context, "Nguyễn Văn A", "0901000001");
            var c9 = await GetOrCreateCustomerAsync(context, "Trần Thị B", "0901000002");
            var c10 = await GetOrCreateCustomerAsync(context, "Khách Lẻ (Vãng lai)", "0000000000");

            // Đảm bảo có các sản phẩm mẫu theo ảnh mockup
            var pHeadphone = await GetOrCreateProductAsync(context, "Tai nghe không dây", 1_490_000m, 30, 3);
            var pCharger = await GetOrCreateProductAsync(context, "Sạc nhanh 65W", 890_000m, 50, 1);
            var pCase = await GetOrCreateProductAsync(context, "Ốp lưng", 350_000m, 100, 5);
            var pAkko = await GetOrCreateProductAsync(context, "Bàn phím cơ Akko 3087", 1_200_000m, 20, 2);
            var pG102 = await GetOrCreateProductAsync(context, "Chuột Logitech G102", 390_000m, 50, 1);
            var pSony = await GetOrCreateProductAsync(context, "Tai nghe Sony WH-1000XM4", 6_200_000m, 10, 3);
            var pSSD = await GetOrCreateProductAsync(context, "Ổ cứng SSD Samsung 980 PRO 1TB PCIe 4.0", 2_300_000m, 40, 6);
            var pBalo = await GetOrCreateProductAsync(context, "Balo Laptop Targus 15.6 inch", 760_000m, 60, 8);
            var pRAM = await GetOrCreateProductAsync(context, "RAM Corsair Vengeance RGB Pro 16GB", 1_450_000m, 50, 6);

            // ==========================================
            // TẠO 10 HÓA ĐƠN MẪU KHỚP VỚI MOCKUP
            // ==========================================

            // 1. HD-000128: 18/10/2026, 2.450.000đ, Chuyển khoản, Đã thanh toán (Mockup 1)
            var inv128 = new Invoice
            {
                InvoiceNo = "HD-000128",
                CustomerId = c1.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Paid,
                Type = InvoiceType.Sale,
                SubTotal = 2_450_000m,
                DiscountAmount = 0m,
                TotalAmount = 2_450_000m,
                CreatedAt = new DateTime(2026, 10, 18, 10, 15, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pRAM.Id, ProductName = pRAM.Name, Qty = 1, UnitPrice = 1_450_000m, DiscountAlloc = 0m, LineTotal = 1_450_000m, ReturnedQty = 0, IsReturnable = true },
                    new() { ProductId = pAkko.Id, ProductName = pAkko.Name, Qty = 1, UnitPrice = 1_000_000m, DiscountAlloc = 0m, LineTotal = 1_000_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Transfer, Amount = 2_450_000m, TransactionCode = "VCB_128_9988", PaidAt = new DateTime(2026, 10, 18, 10, 15, 0) }
                }
            };

            // 2. HD-000127: 17/10/2026, 890.000đ, Tiền mặt, Đã thanh toán (Mockup 1)
            var inv127 = new Invoice
            {
                InvoiceNo = "HD-000127",
                CustomerId = c2.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Paid,
                Type = InvoiceType.Sale,
                SubTotal = 890_000m,
                DiscountAmount = 0m,
                TotalAmount = 890_000m,
                CreatedAt = new DateTime(2026, 10, 17, 14, 20, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pCharger.Id, ProductName = pCharger.Name, Qty = 1, UnitPrice = 890_000m, DiscountAlloc = 0m, LineTotal = 890_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Cash, Amount = 890_000m, PaidAt = new DateTime(2026, 10, 17, 14, 20, 0) }
                }
            };

            // 3. HD-000123: 14/10/2026, 5.180.000đ, Thẻ 3M + Tiền mặt 2.18M, Trả một phần (Mockup 1 & 2)
            var inv123 = new Invoice
            {
                InvoiceNo = "HD-000123",
                CustomerId = c3.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.PartiallyReturned,
                Type = InvoiceType.Sale,
                SubTotal = 5_180_000m,
                DiscountAmount = 0m,
                TotalAmount = 5_180_000m,
                CreatedAt = new DateTime(2026, 10, 14, 9, 0, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pHeadphone.Id, ProductName = "Tai nghe không dây", Qty = 2, UnitPrice = 1_490_000m, DiscountAlloc = 0m, LineTotal = 2_980_000m, ReturnedQty = 0, IsReturnable = true },
                    new() { ProductId = pCharger.Id, ProductName = "Sạc nhanh 65W", Qty = 1, UnitPrice = 890_000m, DiscountAlloc = 0m, LineTotal = 890_000m, ReturnedQty = 1, IsReturnable = true },
                    new() { ProductId = pCase.Id, ProductName = "Ốp lưng", Qty = 1, UnitPrice = 350_000m, DiscountAlloc = 0m, LineTotal = 350_000m, ReturnedQty = 0, IsReturnable = false }, // Không đổi trả
                    new() { ProductId = pAkko.Id, ProductName = pAkko.Name, Qty = 1, UnitPrice = 960_000m, DiscountAlloc = 0m, LineTotal = 960_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Card, Amount = 3_000_000m, TransactionCode = "POS_CARD_123", PaidAt = new DateTime(2026, 10, 14, 9, 0, 0) },
                    new() { Method = PaymentMethod.Cash, Amount = 2_180_000m, PaidAt = new DateTime(2026, 10, 14, 9, 0, 0) }
                }
            };

            // 4. HD-000119: 11/10/2026, 1.320.000đ, Ví điện tử, Đã trả hết (Mockup 1)
            var inv119 = new Invoice
            {
                InvoiceNo = "HD-000119",
                CustomerId = c4.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.FullyReturned,
                Type = InvoiceType.Sale,
                SubTotal = 1_320_000m,
                DiscountAmount = 0m,
                TotalAmount = 1_320_000m,
                CreatedAt = new DateTime(2026, 10, 11, 11, 30, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pAkko.Id, ProductName = pAkko.Name, Qty = 1, UnitPrice = 1_320_000m, DiscountAlloc = 0m, LineTotal = 1_320_000m, ReturnedQty = 1, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.EWallet, Amount = 1_320_000m, TransactionCode = "MOMO_119_382", PaidAt = new DateTime(2026, 10, 11, 11, 30, 0) }
                }
            };

            // 5. HD-000112: 06/10/2026, 760.000đ, Tiền mặt, Đã hủy (Mockup 1)
            var inv112 = new Invoice
            {
                InvoiceNo = "HD-000112",
                CustomerId = c5.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Cancelled,
                Type = InvoiceType.Sale,
                SubTotal = 760_000m,
                DiscountAmount = 0m,
                TotalAmount = 760_000m,
                CancelReason = "Khách hàng đổi ý trước khi nhận hàng",
                CancelledAt = new DateTime(2026, 10, 6, 16, 0, 0),
                CancelledByUserId = manager.Id,
                CreatedAt = new DateTime(2026, 10, 6, 15, 45, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pBalo.Id, ProductName = pBalo.Name, Qty = 1, UnitPrice = 760_000m, DiscountAlloc = 0m, LineTotal = 760_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Cash, Amount = 760_000m, PaidAt = new DateTime(2026, 10, 6, 15, 45, 0) }
                }
            };

            // 6. HD-000110: 10/10/2026, 890.000đ, Yêu cầu đổi trả YC-0042 (Mockup 3 & 4)
            var inv110 = new Invoice
            {
                InvoiceNo = "HD-000110",
                CustomerId = c6.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Paid,
                Type = InvoiceType.Sale,
                SubTotal = 890_000m,
                DiscountAmount = 0m,
                TotalAmount = 890_000m,
                CreatedAt = new DateTime(2026, 10, 10, 13, 0, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pCharger.Id, ProductName = "Sạc nhanh 65W", Qty = 1, UnitPrice = 890_000m, DiscountAlloc = 0m, LineTotal = 890_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Transfer, Amount = 890_000m, TransactionCode = "MB_110_7761", PaidAt = new DateTime(2026, 10, 10, 13, 0, 0) }
                }
            };

            // 7. HD-000117: 12/10/2026, 6.200.000đ (> ngưỡng 5M), Chuyển khoản, Yêu cầu YC-0043 (Mockup 3)
            var inv117 = new Invoice
            {
                InvoiceNo = "HD-000117",
                CustomerId = c7.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Paid,
                Type = InvoiceType.Sale,
                SubTotal = 6_200_000m,
                DiscountAmount = 0m,
                TotalAmount = 6_200_000m,
                CreatedAt = new DateTime(2026, 10, 12, 10, 0, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pSony.Id, ProductName = pSony.Name, Qty = 1, UnitPrice = 6_200_000m, DiscountAlloc = 0m, LineTotal = 6_200_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Transfer, Amount = 6_200_000m, TransactionCode = "BIDV_117_5521", PaidAt = new DateTime(2026, 10, 12, 10, 0, 0) }
                }
            };

            // 8. HD-000121: 15/10/2026, 1.200.000đ, Thẻ, Yêu cầu đổi hàng YC-0044 (Mockup 3)
            var inv121 = new Invoice
            {
                InvoiceNo = "HD-000121",
                CustomerId = c8.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Paid,
                Type = InvoiceType.Sale,
                SubTotal = 1_200_000m,
                DiscountAmount = 0m,
                TotalAmount = 1_200_000m,
                CreatedAt = new DateTime(2026, 10, 15, 14, 0, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pAkko.Id, ProductName = pAkko.Name, Qty = 1, UnitPrice = 1_200_000m, DiscountAlloc = 0m, LineTotal = 1_200_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Card, Amount = 1_200_000m, TransactionCode = "POS_CARD_121", PaidAt = new DateTime(2026, 10, 15, 14, 0, 0) }
                }
            };

            // 9. HD-000124: 16/10/2026, có khuyến mãi phân bổ
            var inv124 = new Invoice
            {
                InvoiceNo = "HD-000124",
                CustomerId = c9.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Paid,
                Type = InvoiceType.Sale,
                SubTotal = 1_590_000m,
                DiscountAmount = 100_000m, // Giảm giá 100k
                TotalAmount = 1_490_000m,
                CreatedAt = new DateTime(2026, 10, 16, 16, 20, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pG102.Id, ProductName = pG102.Name, Qty = 1, UnitPrice = 390_000m, DiscountAlloc = 24_530m, LineTotal = 365_470m, ReturnedQty = 0, IsReturnable = true },
                    new() { ProductId = pAkko.Id, ProductName = pAkko.Name, Qty = 1, UnitPrice = 1_200_000m, DiscountAlloc = 75_470m, LineTotal = 1_124_530m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Cash, Amount = 1_490_000m, PaidAt = new DateTime(2026, 10, 16, 16, 20, 0) }
                }
            };

            // 10. HD-000125: 17/10/2026, khách vãng lai mua SSD Samsung
            var inv125 = new Invoice
            {
                InvoiceNo = "HD-000125",
                CustomerId = c10.Id,
                CreatedByUserId = seller.Id,
                Status = InvoiceStatus.Paid,
                Type = InvoiceType.Sale,
                SubTotal = 2_300_000m,
                DiscountAmount = 0m,
                TotalAmount = 2_300_000m,
                CreatedAt = new DateTime(2026, 10, 17, 18, 0, 0),
                Items = new List<InvoiceItem>
                {
                    new() { ProductId = pSSD.Id, ProductName = pSSD.Name, Qty = 1, UnitPrice = 2_300_000m, DiscountAlloc = 0m, LineTotal = 2_300_000m, ReturnedQty = 0, IsReturnable = true }
                },
                Payments = new List<InvoicePayment>
                {
                    new() { Method = PaymentMethod.Transfer, Amount = 2_300_000m, TransactionCode = "VCB_125_7712", PaidAt = new DateTime(2026, 10, 17, 18, 0, 0) }
                }
            };

            context.Invoices.AddRange(inv128, inv127, inv123, inv119, inv112, inv110, inv117, inv121, inv124, inv125);
            await context.SaveChangesAsync();

            // ==========================================
            // TẠO CÁC YÊU CẦU ĐỔI TRẢ MẪU (KHỚP MOCKUP 3 & 4)
            // ==========================================

            // YC-0045: Hóa đơn HD-000123, Tai nghe không dây x 1, Trả hàng, Tại quầy, Chờ duyệt (Mockup 2 & 3)
            var itemHeadphone123 = inv123.Items.First(i => i.ProductName.Contains("Tai nghe"));
            var req45 = new ReturnRequest
            {
                RequestNo = "YC-0045",
                InvoiceId = inv123.Id,
                Type = ReturnType.Return,
                Channel = ReturnChannel.Counter,
                Status = ReturnStatus.Pending,
                ReasonCode = "DEFECT",
                ReasonNote = "Sản phẩm lỗi - không lên nguồn",
                TotalRefund = 1_490_000m,
                CreatedByUserId = seller.Id,
                CreatedAt = new DateTime(2026, 10, 18, 9, 12, 0),
                Items = new List<ReturnItem>
                {
                    new()
                    {
                        InvoiceItemId = itemHeadphone123.Id,
                        ProductId = pHeadphone.Id,
                        Qty = 1,
                        Condition = ItemCondition.Defective,
                        Restock = false,
                        RefundAmount = 1_490_000m
                    }
                }
            };

            // YC-0044: HD-000121, Đổi hàng, Gửi hàng, Giao sai hàng, 0đ, Chờ duyệt (Mockup 3)
            var itemAkko121 = inv121.Items.First();
            var req44 = new ReturnRequest
            {
                RequestNo = "YC-0044",
                InvoiceId = inv121.Id,
                Type = ReturnType.Exchange,
                Channel = ReturnChannel.Shipping,
                Status = ReturnStatus.Pending,
                ReasonCode = "WRONG_ITEM",
                ReasonNote = "Giao sai mẫu mã sản phẩm",
                TotalRefund = 0m,
                CreatedByUserId = seller.Id,
                CreatedAt = new DateTime(2026, 10, 17, 10, 30, 0),
                Items = new List<ReturnItem>
                {
                    new()
                    {
                        InvoiceItemId = itemAkko121.Id,
                        ProductId = pAkko.Id,
                        Qty = 1,
                        Condition = ItemCondition.Good,
                        Restock = true,
                        RefundAmount = 0m
                    }
                }
            };

            // YC-0043: HD-000117, Trả hàng, Tại quầy, Đổi ý, 6.200.000đ, Cần quản lý duyệt (Mockup 3)
            var itemSony117 = inv117.Items.First();
            var req43 = new ReturnRequest
            {
                RequestNo = "YC-0043",
                InvoiceId = inv117.Id,
                Type = ReturnType.Return,
                Channel = ReturnChannel.Counter,
                Status = ReturnStatus.Pending,
                ReasonCode = "CHANGE_MIND",
                ReasonNote = "Khách hàng đổi ý không còn nhu cầu",
                TotalRefund = 6_200_000m,
                CreatedByUserId = seller.Id,
                CreatedAt = new DateTime(2026, 10, 16, 15, 0, 0),
                Items = new List<ReturnItem>
                {
                    new()
                    {
                        InvoiceItemId = itemSony117.Id,
                        ProductId = pSony.Id,
                        Qty = 1,
                        Condition = ItemCondition.Good,
                        Restock = true,
                        RefundAmount = 6_200_000m
                    }
                }
            };

            // YC-0042: HD-000110, Trả hàng, Gửi hàng, Không đúng mô tả, 890.000đ, Đã duyệt (Mockup 3 & 4)
            var itemCharger110 = inv110.Items.First();
            var req42 = new ReturnRequest
            {
                RequestNo = "YC-0042",
                InvoiceId = inv110.Id,
                Type = ReturnType.Return,
                Channel = ReturnChannel.Shipping,
                Status = ReturnStatus.Approved,
                ReasonCode = "NOT_AS_DESCRIBED",
                ReasonNote = "Không đúng mô tả kích thước",
                TotalRefund = 890_000m,
                CreatedByUserId = seller.Id,
                ApprovedByUserId = manager.Id,
                ApprovedAt = new DateTime(2026, 10, 17, 9, 0, 0),
                CreatedAt = new DateTime(2026, 10, 16, 11, 0, 0),
                Items = new List<ReturnItem>
                {
                    new()
                    {
                        InvoiceItemId = itemCharger110.Id,
                        ProductId = pCharger.Id,
                        Qty = 1,
                        Condition = ItemCondition.Good,
                        Restock = true,
                        RefundAmount = 890_000m
                    }
                }
            };

            context.ReturnRequests.AddRange(req45, req44, req43, req42);
            await context.SaveChangesAsync();

            // ==========================================
            // GHI STOCK MOVEMENT CHO CÁC ĐƠN BÁN HÀNG
            // ==========================================
            var movements = new List<StockMovement>
            {
                new() { ProductId = pRAM.Id, MovementType = StockMovementType.Sale, Quantity = -1, UnitPrice = 1_450_000m, ReferenceType = "Invoice", ReferenceId = inv128.Id, CreatedAt = inv128.CreatedAt, CreatedByUserId = seller.Id, Note = "Xuất bán theo HD-000128" },
                new() { ProductId = pCharger.Id, MovementType = StockMovementType.Sale, Quantity = -1, UnitPrice = 890_000m, ReferenceType = "Invoice", ReferenceId = inv127.Id, CreatedAt = inv127.CreatedAt, CreatedByUserId = seller.Id, Note = "Xuất bán theo HD-000127" },
                new() { ProductId = pHeadphone.Id, MovementType = StockMovementType.Sale, Quantity = -2, UnitPrice = 1_490_000m, ReferenceType = "Invoice", ReferenceId = inv123.Id, CreatedAt = inv123.CreatedAt, CreatedByUserId = seller.Id, Note = "Xuất bán theo HD-000123" },
                new() { ProductId = pCharger.Id, MovementType = StockMovementType.Sale, Quantity = -1, UnitPrice = 890_000m, ReferenceType = "Invoice", ReferenceId = inv123.Id, CreatedAt = inv123.CreatedAt, CreatedByUserId = seller.Id, Note = "Xuất bán theo HD-000123" },
                new() { ProductId = pCase.Id, MovementType = StockMovementType.Sale, Quantity = -1, UnitPrice = 350_000m, ReferenceType = "Invoice", ReferenceId = inv123.Id, CreatedAt = inv123.CreatedAt, CreatedByUserId = seller.Id, Note = "Xuất bán theo HD-000123" }
            };
            context.StockMovements.AddRange(movements);
            await context.SaveChangesAsync();
        }

        private static async Task<Customer> GetOrCreateCustomerAsync(AppDbContext context, string name, string phone)
        {
            var customer = await context.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
            if (customer != null) return customer;

            customer = new Customer { Name = name, Phone = phone, RewardPoints = 50 };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            return customer;
        }

        private static async Task<Product> GetOrCreateProductAsync(AppDbContext context, string name, decimal price, int stock, int? fallbackCategoryId = null)
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Name == name);
            if (product != null) return product;

            // Tra cứu category ID thực tế từ database, không hard-code
            var category = await context.Categories.FirstOrDefaultAsync();
            int catId = category?.Id ?? fallbackCategoryId ?? 1;

            product = new Product
            {
                Name = name,
                Price = price,
                StockQuantity = stock,
                CategoryId = catId,
                Barcode = "880" + Random.Shared.Next(100000000, 999999999)
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return product;
        }
    }
}
