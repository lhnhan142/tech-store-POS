using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;

namespace GUI.Forms.Warehouse
{
    /// <summary>Màn hình lập và tra cứu Phiếu nhập kho.</summary>
    [DesignerCategory("Code")]
    public class ReceiptForm : WarehouseDocumentForm
    {
        private readonly ComboBox _cboSupplier = new();
        private readonly DateTimePicker _dtpDate = new();
        private readonly ComboBox _cboProduct = new();
        private readonly NumericUpDown _nudQty = new();
        private readonly NumericUpDown _nudPrice = new();
        private readonly HopeRoundButton _btnAddLine = new();
        private readonly HopeRoundButton _btnRemoveLine = new();
        private readonly BindingList<ReceiptLineItem> _lines = new();
        private IReadOnlyList<ProductStockDTO> _products = Array.Empty<ProductStockDTO>();

        public ReceiptForm(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
            Build();
        }

        protected override string PageTitle => "Phiếu nhập kho";
        protected override string PageSubtitle => "Lập phiếu nhập hàng từ nhà cung cấp và tra cứu các phiếu đã lập";
        protected override string EditorTitle => "Lập phiếu nhập mới";
        protected override string SaveButtonText => "Lưu phiếu nhập";
        protected override string HistoryCountFormat => "{0} phiếu nhập";

        // ------------------------------------------------------------ Bảng bên trái
        protected override void ConfigureHistoryColumns(DataGridView grid)
        {
            grid.Columns.Add(TextColumn("Mã", nameof(ReceiptSummaryDTO.Id), width: 44));
            grid.Columns.Add(TextColumn("Ngày nhập", nameof(ReceiptSummaryDTO.ReceiptDate), width: 125, format: "dd/MM/yyyy HH:mm"));
            grid.Columns.Add(TextColumn("Nhà cung cấp", nameof(ReceiptSummaryDTO.SupplierName), fillWeight: 40, minWidth: 85));
            grid.Columns.Add(TextColumn("Người lập", nameof(ReceiptSummaryDTO.UserName), fillWeight: 25, minWidth: 78));
            grid.Columns.Add(TextColumn("Tổng tiền", nameof(ReceiptSummaryDTO.TotalAmount), width: 105, format: "N0", right: true));
        }

        protected override void ConfigureDetailColumns(DataGridView grid)
        {
            grid.Columns.Add(TextColumn("Sản phẩm", nameof(ReceiptLineDTO.ProductName), fillWeight: 50));
            grid.Columns.Add(TextColumn("SL", nameof(ReceiptLineDTO.Quantity), width: 60, format: "N0", right: true));
            grid.Columns.Add(TextColumn("Đơn giá", nameof(ReceiptLineDTO.UnitPrice), width: 110, format: "N0", right: true));
            grid.Columns.Add(TextColumn("Thành tiền", nameof(ReceiptLineDTO.LineTotal), width: 120, format: "N0", right: true));
        }

        protected override int IdOf(object historyItem) => ((ReceiptSummaryDTO)historyItem).Id;

        protected override async Task<System.Collections.IList> LoadHistoryAsync(DateTime from, DateTime to) =>
            (await Run<IInventoryService, IReadOnlyList<ReceiptSummaryDTO>>(s => s.GetReceiptsAsync(from, to))).ToList();

        protected override async Task<System.Collections.IList> LoadDetailAsync(object historyItem)
        {
            int id = ((ReceiptSummaryDTO)historyItem).Id;
            return (await Run<IInventoryService, IReadOnlyList<ReceiptLineDTO>>(s => s.GetReceiptLinesAsync(id))).ToList();
        }

        // ------------------------------------------------------------ Form lập phiếu
        protected override void BuildEditor()
        {
            _cboSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboSupplier.DisplayMember = nameof(Supplier.Name);
            _cboSupplier.ValueMember = nameof(Supplier.Id);

            _dtpDate.Format = DateTimePickerFormat.Custom;
            _dtpDate.CustomFormat = "dd/MM/yyyy HH:mm";

            _cboProduct.DropDownStyle = ComboBoxStyle.DropDown;
            _cboProduct.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _cboProduct.AutoCompleteSource = AutoCompleteSource.ListItems;
            _cboProduct.DisplayMember = nameof(ProductStockDTO.Display);
            _cboProduct.ValueMember = nameof(ProductStockDTO.Id);

            _nudQty.Minimum = 1;
            _nudQty.Maximum = 1_000_000;
            _nudQty.Value = 1;
            _nudQty.TextAlign = HorizontalAlignment.Right;

            _nudPrice.Minimum = 0;
            _nudPrice.Maximum = 1_000_000_000;
            _nudPrice.DecimalPlaces = 0;
            _nudPrice.Increment = 1000;
            _nudPrice.ThousandsSeparator = true;
            _nudPrice.TextAlign = HorizontalAlignment.Right;

            AddEditorRow(FieldRow((Field("Nhà cung cấp *", _cboSupplier), 30), (Field("Ngày nhập *", _dtpDate), 40), (ScanField(OnScan), 30)), 64);
            AddEditorRow(FieldRow(
                (Field("Sản phẩm *", _cboProduct), 50),
                (Field("Số lượng *", _nudQty), 18),
                (Field("Đơn giá nhập (₫) *", _nudPrice), 32)), 64);

            // Enter ở ô số lượng / đơn giá = thêm dòng (tiện khi vừa quét mã xong)
            foreach (var nud in new[] { _nudQty, _nudPrice })
                nud.KeyDown += (_, e) =>
                {
                    if (e.KeyCode != Keys.Enter) return;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    AddLine();
                };

            ConfigButton(_btnAddLine, "Thêm vào phiếu", HopeButtonType.Primary, (_, _) => AddLine());
            ConfigButton(_btnRemoveLine, "Xóa dòng đã chọn", HopeButtonType.Danger, (_, _) => RemoveSelectedLine());
            AddEditorRow(ButtonRow(_btnAddLine, _btnRemoveLine), 50);

            SetupGrid(LineGrid, readOnly: true);
            LineGrid.Columns.Add(TextColumn("Sản phẩm", nameof(ReceiptLineItem.ProductName), fillWeight: 50));
            LineGrid.Columns.Add(TextColumn("SL", nameof(ReceiptLineItem.Quantity), width: 60, format: "N0", right: true));
            LineGrid.Columns.Add(TextColumn("Đơn giá", nameof(ReceiptLineItem.UnitPrice), width: 110, format: "N0", right: true));
            LineGrid.Columns.Add(TextColumn("Thành tiền", nameof(ReceiptLineItem.LineTotal), width: 120, format: "N0", right: true));
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
            var suppliers = await Run<ISupplierService, IReadOnlyList<Supplier>>(s => s.GetAllAsync());
            var products = await Run<IInventoryService, IReadOnlyList<ProductStockDTO>>(s => s.GetProductsAsync());

            _products = products;
            _cboSupplier.DataSource = suppliers.ToList();
            _cboSupplier.SelectedIndex = -1;
            _cboProduct.DataSource = products.ToList();
            _cboProduct.SelectedIndex = -1;
        }

        protected override void ResetEditor()
        {
            _lines.Clear();
            _cboSupplier.SelectedIndex = -1;
            _cboProduct.SelectedIndex = -1;
            _cboProduct.Text = string.Empty;
            _dtpDate.Value = DateTime.Now;
            _nudQty.Value = 1;
            _nudPrice.Value = 0;
            UpdateSummary();
            _cboSupplier.Focus();
        }

        // ------------------------------------------------------------ Quét mã
        // Sản phẩm đã có trong phiếu: quét thêm = cộng thêm số lượng đang nhập, giữ đơn giá của dòng đó.
        // Sản phẩm mới: chọn sản phẩm rồi chuyển con trỏ sang ô đơn giá để nhập giá (Enter để thêm dòng).
        private bool OnScan(string code)
        {
            var product = FindByCode(_products, code);
            if (product is null) return false;

            _cboProduct.SelectedItem = product;
            var existing = _lines.FirstOrDefault(l => l.ProductId == product.Id);
            if (existing is not null)
            {
                _nudPrice.Value = existing.UnitPrice;
                AddLine();
                ScanBox.Focus();
            }
            else
            {
                _nudPrice.Value = 0;
                _nudPrice.Focus();
                _nudPrice.Select(0, _nudPrice.Text.Length);
            }
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
            if (_nudPrice.Value <= 0)
            {
                MessageBox.Show("Hãy nhập đơn giá nhập lớn hơn 0.", "Thiếu đơn giá", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nudPrice.Focus();
                return;
            }

            int qty = (int)_nudQty.Value;
            var existing = _lines.FirstOrDefault(l => l.ProductId == product.Id);
            if (existing is not null)
            {
                // Sản phẩm đã có trong phiếu: cộng dồn số lượng, lấy đơn giá mới nhất
                if ((long)existing.Quantity + qty > 1_000_000)
                {
                    MessageBox.Show("Tổng số lượng của một sản phẩm tối đa 1.000.000.", "Vượt giới hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                existing.Quantity += qty;
                existing.UnitPrice = _nudPrice.Value;
                _lines.ResetItem(_lines.IndexOf(existing));
            }
            else
            {
                _lines.Add(new ReceiptLineItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = qty,
                    UnitPrice = _nudPrice.Value
                });
            }

            _cboProduct.SelectedIndex = -1;
            _cboProduct.Text = string.Empty;
            _nudQty.Value = 1;
            _cboProduct.Focus();
        }

        private void RemoveSelectedLine()
        {
            if (LineGrid.CurrentRow?.DataBoundItem is ReceiptLineItem item)
                _lines.Remove(item);
        }

        private void UpdateSummary()
        {
            var total = _lines.Sum(l => l.LineTotal);
            SummaryLabel.Text = $"{_lines.Count} dòng  ·  Tổng tiền: {Money(total)}";
        }

        // ------------------------------------------------------------ Lưu phiếu
        protected override async Task SaveDocumentAsync()
        {
            if (_cboSupplier.SelectedValue is not int supplierId)
            {
                MessageBox.Show("Hãy chọn nhà cung cấp.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboSupplier.Focus();
                return;
            }
            if (_lines.Count == 0)
            {
                MessageBox.Show("Phiếu nhập chưa có sản phẩm nào.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboProduct.Focus();
                return;
            }

            var total = _lines.Sum(l => l.LineTotal);
            var confirm = MessageBox.Show($"Lưu phiếu nhập {_lines.Count} dòng, tổng tiền {Money(total)}?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var userId = AppSession.RequireUserId();
            var date = _dtpDate.Value;
            var inputs = _lines.Select(l => new ReceiptLineInput(l.ProductId, l.Quantity, l.UnitPrice)).ToList();

            var result = await Run<IInventoryService, InventoryResultDTO>(s => s.CreateReceiptAsync(supplierId, userId, date, inputs));
            await AfterSavedAsync(result, $"Đã lưu phiếu nhập #{result.DocumentId} và cộng số lượng vào tồn kho.");
        }

        protected override void SetBusy(bool busy)
        {
            base.SetBusy(busy);
            _btnAddLine.Enabled = _btnRemoveLine.Enabled = !busy;
        }
    }

    // Một dòng của phiếu nhập đang lập (public property để DataGridView gắn cột)
    internal sealed class ReceiptLineItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
