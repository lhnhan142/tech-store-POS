using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;

namespace GUI.Forms.Warehouse
{
    /// <summary>Màn hình lập và tra cứu Phiếu xuất kho (hàng hỏng, trả nhà cung cấp, dùng nội bộ...).</summary>
    [DesignerCategory("Code")]
    public class IssueForm : WarehouseDocumentForm
    {
        private static readonly string[] Reasons =
        {
            "Hàng hỏng / lỗi",
            "Trả hàng cho nhà cung cấp",
            "Dùng nội bộ / trưng bày",
            "Thất thoát / mất mát",
            "Khác"
        };

        private readonly DateTimePicker _dtpDate = new();
        private readonly ComboBox _cboReason = new();
        private readonly TextBox _txtNote = new();
        private readonly ComboBox _cboProduct = new();
        private readonly NumericUpDown _nudQty = new();
        private readonly HopeRoundButton _btnAddLine = new();
        private readonly HopeRoundButton _btnRemoveLine = new();
        private readonly BindingList<IssueLineItem> _lines = new();
        private IReadOnlyList<ProductStockDTO> _products = Array.Empty<ProductStockDTO>();

        public IssueForm(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
            Build();
        }

        protected override string PageTitle => "Phiếu xuất kho";
        protected override string PageSubtitle => "Xuất hàng hỏng, trả nhà cung cấp, dùng nội bộ... và tra cứu các phiếu đã lập";
        protected override string EditorTitle => "Lập phiếu xuất mới";
        protected override string SaveButtonText => "Lưu phiếu xuất";
        protected override string HistoryCountFormat => "{0} phiếu xuất";

        // ------------------------------------------------------------ Bảng bên trái
        protected override void ConfigureHistoryColumns(DataGridView grid)
        {
            grid.Columns.Add(TextColumn("Mã", nameof(IssueSummaryDTO.Id), width: 44));
            grid.Columns.Add(TextColumn("Ngày xuất", nameof(IssueSummaryDTO.IssueDate), width: 125, format: "dd/MM/yyyy HH:mm"));
            grid.Columns.Add(TextColumn("Lý do", nameof(IssueSummaryDTO.Reason), fillWeight: 40, minWidth: 85));
            grid.Columns.Add(TextColumn("Người lập", nameof(IssueSummaryDTO.UserName), fillWeight: 25, minWidth: 78));
            grid.Columns.Add(TextColumn("Tổng SL", nameof(IssueSummaryDTO.TotalQuantity), width: 80, format: "N0", right: true));
        }

        protected override void ConfigureDetailColumns(DataGridView grid)
        {
            grid.Columns.Add(TextColumn("Sản phẩm", nameof(IssueLineDTO.ProductName), fillWeight: 70));
            grid.Columns.Add(TextColumn("Số lượng xuất", nameof(IssueLineDTO.Quantity), width: 120, format: "N0", right: true));
        }

        protected override int IdOf(object historyItem) => ((IssueSummaryDTO)historyItem).Id;

        protected override async Task<System.Collections.IList> LoadHistoryAsync(DateTime from, DateTime to) =>
            (await Run<IInventoryService, IReadOnlyList<IssueSummaryDTO>>(s => s.GetIssuesAsync(from, to))).ToList();

        protected override async Task<System.Collections.IList> LoadDetailAsync(object historyItem)
        {
            int id = ((IssueSummaryDTO)historyItem).Id;
            return (await Run<IInventoryService, IReadOnlyList<IssueLineDTO>>(s => s.GetIssueLinesAsync(id))).ToList();
        }

        // ------------------------------------------------------------ Form lập phiếu
        protected override void BuildEditor()
        {
            _dtpDate.Format = DateTimePickerFormat.Custom;
            _dtpDate.CustomFormat = "dd/MM/yyyy HH:mm";

            _cboReason.DropDownStyle = ComboBoxStyle.DropDown;   // chọn mẫu hoặc tự gõ lý do khác
            _cboReason.MaxLength = 100;
            _cboReason.Items.AddRange(Reasons);

            _txtNote.MaxLength = 200;
            _txtNote.PlaceholderText = "Không bắt buộc";

            _cboProduct.DropDownStyle = ComboBoxStyle.DropDown;
            _cboProduct.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _cboProduct.AutoCompleteSource = AutoCompleteSource.ListItems;
            _cboProduct.DisplayMember = nameof(ProductStockDTO.Display);
            _cboProduct.ValueMember = nameof(ProductStockDTO.Id);

            _nudQty.Minimum = 1;
            _nudQty.Maximum = 1_000_000;
            _nudQty.Value = 1;
            _nudQty.TextAlign = HorizontalAlignment.Right;

            AddEditorRow(FieldRow((Field("Lý do xuất kho *", _cboReason), 60), (Field("Ngày xuất *", _dtpDate), 40)), 64);
            AddEditorRow(FieldRow((Field("Ghi chú", _txtNote), 50), (ScanField(OnScan), 50)), 64);
            AddEditorRow(FieldRow((Field("Sản phẩm *", _cboProduct), 75), (Field("Số lượng *", _nudQty), 25)), 64);

            ConfigButton(_btnAddLine, "Thêm vào phiếu", HopeButtonType.Primary, (_, _) => AddLine());
            ConfigButton(_btnRemoveLine, "Xóa dòng đã chọn", HopeButtonType.Danger, (_, _) => RemoveSelectedLine());
            AddEditorRow(ButtonRow(_btnAddLine, _btnRemoveLine), 50);

            SetupGrid(LineGrid, readOnly: true);
            LineGrid.Columns.Add(TextColumn("Sản phẩm", nameof(IssueLineItem.ProductName), fillWeight: 50));
            LineGrid.Columns.Add(TextColumn("SL xuất", nameof(IssueLineItem.Quantity), width: 80, format: "N0", right: true));
            LineGrid.Columns.Add(TextColumn("Tồn hiện tại", nameof(IssueLineItem.Stock), width: 100, format: "N0", right: true));
            LineGrid.Columns.Add(TextColumn("Còn lại", nameof(IssueLineItem.Remaining), width: 90, format: "N0", right: true));
            LineGrid.DataSource = _lines;
            LineGrid.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Delete) { RemoveSelectedLine(); e.Handled = true; }
            };
            _lines.ListChanged += (_, _) => UpdateSummary();
            AddEditorFill(LineGrid);
        }

        protected override async Task LoadLookupsAsync()
        {
            var products = await Run<IInventoryService, IReadOnlyList<ProductStockDTO>>(s => s.GetProductsAsync());
            _products = products;
            _cboProduct.DataSource = products.ToList();
            _cboProduct.SelectedIndex = -1;
        }

        protected override void ResetEditor()
        {
            _lines.Clear();
            _cboReason.SelectedIndex = -1;
            _cboReason.Text = string.Empty;
            _txtNote.Clear();
            _cboProduct.SelectedIndex = -1;
            _cboProduct.Text = string.Empty;
            _dtpDate.Value = DateTime.Now;
            _nudQty.Value = 1;
            UpdateSummary();
            _cboReason.Focus();
        }

        // ------------------------------------------------------------ Quét mã
        // Mỗi lần quét = thêm số lượng đang nhập (mặc định 1) của sản phẩm đó vào phiếu;
        // quét lại cùng sản phẩm thì cộng dồn. AddLine() vẫn kiểm tra vượt tồn kho.
        private bool OnScan(string code)
        {
            var product = FindByCode(_products, code);
            if (product is null) return false;

            _cboProduct.SelectedItem = product;
            AddLine();
            ScanBox.Focus();
            return true;
        }

        // ------------------------------------------------------------ Thao tác dòng
        private void AddLine()
        {
            if (_cboProduct.SelectedItem is not ProductStockDTO product)
            {
                MessageBox.Show("Hãy chọn một sản phẩm trong danh sách.", "Chưa chọn sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboProduct.Focus();
                return;
            }

            int qty = (int)_nudQty.Value;
            var existing = _lines.FirstOrDefault(l => l.ProductId == product.Id);
            int totalQty = qty + (existing?.Quantity ?? 0);

            // Kiểm tra nhanh theo tồn kho đang hiển thị; BLL vẫn kiểm tra lại khi lưu phiếu
            if (totalQty > product.StockQuantity)
            {
                MessageBox.Show($"\"{product.Name}\" chỉ còn {product.StockQuantity} trong kho, không thể xuất {totalQty}.",
                    "Vượt tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (existing is not null)
            {
                existing.Quantity = totalQty;
                _lines.ResetItem(_lines.IndexOf(existing));
            }
            else
            {
                _lines.Add(new IssueLineItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = qty,
                    Stock = product.StockQuantity
                });
            }

            _cboProduct.SelectedIndex = -1;
            _cboProduct.Text = string.Empty;
            _nudQty.Value = 1;
            _cboProduct.Focus();
        }

        private void RemoveSelectedLine()
        {
            if (LineGrid.CurrentRow?.DataBoundItem is IssueLineItem item)
                _lines.Remove(item);
        }

        private void UpdateSummary()
        {
            SummaryLabel.Text = $"{_lines.Count} dòng  ·  Tổng số lượng xuất: {_lines.Sum(l => l.Quantity):N0}";
        }

        // ------------------------------------------------------------ Lưu phiếu
        protected override async Task SaveDocumentAsync()
        {
            var reason = _cboReason.Text.Trim();
            if (reason.Length == 0)
            {
                MessageBox.Show("Hãy chọn hoặc nhập lý do xuất kho.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboReason.Focus();
                return;
            }
            if (_lines.Count == 0)
            {
                MessageBox.Show("Phiếu xuất chưa có sản phẩm nào.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboProduct.Focus();
                return;
            }

            var confirm = MessageBox.Show($"Lưu phiếu xuất {_lines.Count} dòng, tổng {_lines.Sum(l => l.Quantity):N0} sản phẩm? Tồn kho sẽ bị trừ tương ứng.", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var userId = AppSession.RequireUserId();
            var date = _dtpDate.Value;
            var note = _txtNote.Text;
            var inputs = _lines.Select(l => new IssueLineInput(l.ProductId, l.Quantity)).ToList();

            var result = await Run<IInventoryService, InventoryResultDTO>(s => s.CreateIssueAsync(userId, date, reason, note, inputs));
            await AfterSavedAsync(result, $"Đã lưu phiếu xuất #{result.DocumentId} và trừ số lượng khỏi tồn kho.");
        }

        protected override void SetBusy(bool busy)
        {
            base.SetBusy(busy);
            _btnAddLine.Enabled = _btnRemoveLine.Enabled = !busy;
        }
    }

    // Một dòng của phiếu xuất đang lập
    internal sealed class IssueLineItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public int Stock { get; set; }          // tồn kho lúc thêm dòng
        public int Remaining => Stock - Quantity;
    }
}
