using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using PhuKienMayTinh.Common.Constants;

namespace PhuKienMayTinh.Common.Utilities
{
    public static class ExcelHelper
    {
        public static void ExportToExcel(DataTable dataTable, string title, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(title.Length > 30 ? title.Substring(0, 30) : title);

                int currentRow = 1;

                // 1. Tên cửa hàng
                worksheet.Cell(currentRow, 1).Value = AppConstants.StoreName;
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 13;
                worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.FromArgb(30, 41, 59);
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = $"Địa chỉ: {AppConstants.StoreAddress} | SĐT: {AppConstants.StorePhone}";
                worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 10;
                worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Gray;
                currentRow += 2;

                // 2. Tiêu đề báo cáo
                int totalColumns = Math.Max(dataTable.Columns.Count, 1);
                worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge();
                var titleCell = worksheet.Cell(currentRow, 1);
                titleCell.Value = title.ToUpper();
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.FontSize = 15;
                titleCell.Style.Font.FontColor = XLColor.FromArgb(37, 99, 235);
                titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                currentRow++;

                // Ngày xuất
                worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge();
                var dateCell = worksheet.Cell(currentRow, 1);
                dateCell.Value = $"Ngày lập báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                dateCell.Style.Font.Italic = true;
                dateCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dateCell.Style.Font.FontColor = XLColor.DarkGray;
                currentRow += 2;

                // 3. Header bảng
                int headerRow = currentRow;
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    var cell = worksheet.Cell(headerRow, col + 1);
                    cell.Value = dataTable.Columns[col].ColumnName;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(30, 41, 59);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = XLColor.FromArgb(203, 213, 225);
                }
                worksheet.Row(headerRow).Height = 25;
                currentRow++;

                // 4. Dữ liệu
                int startDataRow = currentRow;
                for (int r = 0; r < dataTable.Rows.Count; r++)
                {
                    for (int c = 0; c < dataTable.Columns.Count; c++)
                    {
                        var cell = worksheet.Cell(currentRow, c + 1);
                        object val = dataTable.Rows[r][c];
                        if (val != DBNull.Value && val != null)
                        {
                            if (val is decimal dec)
                            {
                                cell.Value = dec;
                                cell.Style.NumberFormat.Format = "#,##0";
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                            else if (val is double dbl)
                            {
                                cell.Value = dbl;
                                cell.Style.NumberFormat.Format = "#,##0.0";
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                            else if (val is int i)
                            {
                                cell.Value = i;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                            else if (val is DateTime dt)
                            {
                                cell.Value = dt.ToString("dd/MM/yyyy HH:mm");
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }
                            else
                            {
                                cell.Value = val.ToString();
                            }
                        }
                        else
                        {
                            cell.Value = "";
                        }

                        // Zebra striping
                        if (r % 2 == 1)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(248, 250, 252);
                        }

                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.OutsideBorderColor = XLColor.FromArgb(226, 232, 240);
                    }
                    worksheet.Row(currentRow).Height = 20;
                    currentRow++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Save
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                workbook.SaveAs(filePath);
            }
        }
    }
}
