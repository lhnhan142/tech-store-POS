using System;
using System.Data;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PhuKienMayTinh.Common.Constants;

namespace PhuKienMayTinh.Common.Utilities
{
    public static class PdfHelper
    {
        // Sử dụng Arial hoặc Helvetica mặc định
        private static Font GetFont(float size, int style = Font.NORMAL, BaseColor? color = null)
        {
            color = color ?? BaseColor.Black;
            return FontFactory.GetFont(FontFactory.HELVETICA, size, style, color);
        }

        public static void ExportTableToPdf(DataTable dataTable, string title, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Dùng khổ A4 nằm ngang nếu nhiều cột
            var pageSize = dataTable.Columns.Count > 6 ? PageSize.A4.Rotate() : PageSize.A4;
            var document = new Document(pageSize, 30f, 30f, 30f, 30f);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // 1. Tên cửa hàng & Tiêu đề
                var pStore = new Paragraph(AppConstants.StoreName, GetFont(13, Font.BOLD, new BaseColor(30, 41, 59)))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(pStore);

                var pAddr = new Paragraph($"{AppConstants.StoreAddress} - Hotline: {AppConstants.StorePhone}", GetFont(9, Font.ITALIC, BaseColor.Gray))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15f
                };
                document.Add(pAddr);

                var pTitle = new Paragraph(title.ToUpper(), GetFont(15, Font.BOLD, new BaseColor(37, 99, 235)))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 5f
                };
                document.Add(pTitle);

                var pDate = new Paragraph($"Thoi gian xuat: {DateTime.Now:dd/MM/yyyy HH:mm}", GetFont(9, Font.ITALIC, BaseColor.DarkGray))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15f
                };
                document.Add(pDate);

                // 2. Bảng dữ liệu
                var table = new PdfPTable(dataTable.Columns.Count)
                {
                    WidthPercentage = 100f
                };

                // Headers
                foreach (DataColumn column in dataTable.Columns)
                {
                    var cell = new PdfPCell(new Phrase(column.ColumnName, GetFont(9, Font.BOLD, BaseColor.White)))
                    {
                        BackgroundColor = new BaseColor(30, 41, 59),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6f
                    };
                    table.AddCell(cell);
                }

                // Rows
                int rowIndex = 0;
                foreach (DataRow row in dataTable.Rows)
                {
                    var bgColor = (rowIndex % 2 == 1) ? new BaseColor(248, 250, 252) : BaseColor.White;

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        string text = row[i]?.ToString() ?? "";
                        var cell = new PdfPCell(new Phrase(text, GetFont(8.5f, Font.NORMAL, BaseColor.Black)))
                        {
                            BackgroundColor = bgColor,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Padding = 5f,
                            BorderColor = new BaseColor(226, 232, 240)
                        };

                        // Align numbers right
                        if (decimal.TryParse(text.Replace(",", "").Replace(".", "").Replace("đ", "").Trim(), out _))
                        {
                            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        }

                        table.AddCell(cell);
                    }
                    rowIndex++;
                }

                document.Add(table);

                // 3. Chữ ký
                var signTable = new PdfPTable(2)
                {
                    WidthPercentage = 100f,
                    SpacingBefore = 25f
                };

                var cellNguoiLap = new PdfPCell(new Phrase("Nguoi lap bao cao\n(Ky, ghi ro ho ten)", GetFont(10, Font.BOLD)))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                var cellGiamDoc = new PdfPCell(new Phrase("Nguoi duyet / Giam doc\n(Ky, dong dau)", GetFont(10, Font.BOLD)))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                signTable.AddCell(cellNguoiLap);
                signTable.AddCell(cellGiamDoc);
                document.Add(signTable);

                document.Close();
                writer.Close();
            }
        }
    }
}
