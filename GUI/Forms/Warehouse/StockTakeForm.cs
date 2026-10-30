using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using HopeRoundButton = ReaLTaiizor.Controls.HopeRoundButton;
using ReaLTaiizor.Util;
using System.ComponentModel;

namespace GUI.Forms.Warehouse
{
    /// <summary>Màn hình lập và tra cứu Phiếu kiểm kê: nhập số lượng đếm thực tế, so với tồn trên hệ thống.</summary>
    [DesignerCategory("Code")]
    public class StockTakeForm : WarehouseDocumentForm
    {
        private const int MaxQuantity = 1_000_000;

        private readonly DateTimePicker _dtpDate = new();
        private readonly TextBox _txtNote = new();
        private readonly ComboBox _cboProduct = new();
        private readonly HopeRoundButton _btnAddLine = new();
        private readonly HopeRoundButton _btnLoadAll = new();
        private readonly HopeRoundButton _btnRemoveLine = new();
        private readonly BindingList<StockTakeLineItem> _lines = new();
        private IReadOnlyList<ProductStockDTO> _products = Array.Empty<ProductStockDTO>();

        private const int ColActual = 3;   // vị trí cột "Thực tế" trong LineGrid
        private bool _returnToScan;        // vừa quét mã: nhập xong số thực tế thì trả con trỏ về ô quét

        public StockTakeForm(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
            Build();
        }

        protected override string PageTitle => "Kiểm kê kho";
        protected override string PageSubtitle => "Đối chiếu số lượng đếm thực tế với tồn kho trên hệ thống";
        protected override string EditorTitle => "Lập phiếu kiểm kê mới";
        protected override string SaveButtonText => "Lưu phiếu kiểm kê";
        protected override string HistoryCountFormat => "{0} phiếu kiểm kê";

        // ------------------------------------------------------------ Bảng bên trái
        protected override void ConfigureHistoryColumns(DataGridView grid)
        {
            grid.Columns.Add(TextColumn("Mã", nameof(StockTakeSummaryDTO.Id), width: 44));
            grid.Columns.Add(TextColumn("Ngày kiểm kê", nameof(StockTakeSummaryDTO.TakeDate), width: 125, format: "dd/MM/yyyy HH:mm"));
            grid.Columns.Add(TextColumn("Người lập", nameof(StockTakeSummaryDTO.UserName), fillWeight: 40, minWidth: 85));
            grid.Columns.Add(TextColumn("Số dòng", nameof(StockTakeSummaryDTO.LineCount), width: 75, format: "N0", right: true));
            grid.Columns.Add(TextColumn("Lệch", nameof(StockTakeSummaryDTO.DifferenceCount), width: 65, format: "N0", right: true));
        }

        protected override void ConfigureDetailColumns(DataGridView grid)
        {
            grid.Columns.Add(TextColumn("Sản phẩm", nameof(StockTakeLineDTO.ProductName), fillWeight: 50));
            grid.Columns.Add(TextColumn("Hệ thống", nameof(StockTakeLineDTO.SystemQuantity), width: 90, format: "N0", right: true));
            grid.Columns.Add(TextColumn("Thực tế", nameof(StockTakeLineDTO.ActualQuantity), width: 80, format: "N0", right: true));
            grid.Columns.Add(TextColumn("Chênh lệch", nameof(StockTakeLineDTO.Difference), width: 100, format: "+#,##0;-#,##0;0", right: true));
            grid.CellFormatting += ColorDifference;
        }

        protected override int IdOf(object historyItem) => ((StockTakeSummaryDTO)historyItem).Id;

        protected override async Task<System.Collections.IList> LoadHistoryAsync(DateTime from, DateTime to) =>
            (await Run<IInventoryService, IReadOnlyList<StockTakeSummaryDTO>>(s => s.GetStockTakesAsync(from, to))).ToList();

        protected override async Task<System.Collections.IList> LoadDetailAsync(object historyItem)
        {
            int id = ((StockTakeSummaryDTO)historyItem).Id;
            return (await Run<IInventoryService, IReadOnlyList<StockTakeLineDTO>>(s => s.GetStockTakeLinesAsync(id))).ToList();
        }

        // ------------------------------------------------------------ Form lập phiếu
        protected override void BuildEditor()
        {
            _dtpDate.Format = DateTimePickerFormat.Custom;
            _dtpDate.CustomFormat = "dd/MM/yyyy HH:mm";

            _txtNote.MaxLength = 200;
            _txtNote.PlaceholderText = "Không bắt buộc (vd: Kiểm kê cuối tháng, kệ A)";

            _cboProduct.DropDownStyle = ComboBoxStyle.DropDown;
            _cboProduct.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _cboProduct.AutoCompleteSource = AutoCompleteSource.ListItems;
            _cboProduct.DisplayMember = nameof(ProductStockDTO.Display);
            _cboProduct.ValueMember = nameof(ProductStockDTO.Id);

            AddEditorRow(FieldRow((Field("Ngày kiểm kê *", _dtpDate), 35), (Field("Ghi chú", _txtNote), 65)), 64);
            AddEditorRow(FieldRow((ScanField(OnScan), 45), (Field("Thêm riêng một sản phẩm", _cboProduct), 55)), 64);

            ConfigButton(_btnAddLine, "Thêm sản phẩm", HopeButtonType.Primary, (_, _) => AddSelectedProduct());
            ConfigButton(_btnLoadAll, "Nạp tất cả sản phẩm", HopeButtonType.Primary, (_, _) => LoadAllProducts());
            ConfigButton(_btnRemoveLine, "Xóa dòng", HopeButtonType.Danger, (_, _) => RemoveSelectedLine());
            AddEditorRow(ButtonRow(_btnAddLine, _btnLoadAll, _btnRemoveLine), 50);

            // Chỉ cột "Thực tế" cho sửa trực tiếp trên bảng
            SetupGrid(LineGrid, readOnly: false);
            LineGrid.EditMode = DataGridViewEditMode.EditOnEnter;
            LineGrid.Columns.Add(TextColumn("Sản phẩm", nameof(StockTakeLineItem.ProductName), fillWeight: 50));
            LineGrid.Columns.Add(TextColumn("Mã vạch", nameof(StockTakeLineItem.Barcode), width: 110));
            LineGrid.Columns.Add(TextColumn("Hệ thống", nameof(StockTakeLineItem.SystemQuantity), width: 85, format: "N0", right: true));
            LineGrid.Columns.Add(TextColumn("Thực tế", nameof(StockTakeLineItem.ActualQuantity), width: 85, format: "N0", right: true));
            LineGrid.Columns.Add(TextColumn("Chênh lệch", nameof(StockTakeLineItem.Difference), width: 95, format: "+#,##0;-#,##0;0", right: true));
            foreach (DataGridViewColumn col in LineGrid.Columns)
                col.ReadOnly = col.Index != ColActual;
            LineGrid.Columns[ColActual].HeaderText = "Thực tế (nhập)";
            LineGrid.Columns[ColActual].DefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);

            LineGrid.DataSource = _lines;
            LineGrid.CellValidating += LineGrid_CellValidating;
            LineGrid.CellEndEdit += (_, e) =>
            {
                if (e.RowIndex >= 0) LineGrid.InvalidateRow(e.RowIndex);   // vẽ lại cột Chênh lệch
                UpdateSummary();
                if (_returnToScan) { _returnToScan = false; ScanBox.Focus(); }
            };
            LineGrid.CellFormatting += ColorDifference;
            LineGrid.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Delete && !LineGrid.IsCurrentCellInEditMode) { RemoveSelectedLine(); e.Handled = true; }
            };
            _lines.ListChanged += (_, _) => UpdateSummary();
            AddEditorFill(LineGrid);
        }

        protected override async Task LoadLookupsAsync()
        {
            _products = await Run<IInventoryService, IReadOnlyList<ProductStockDTO>>(s => s.GetProductsAsync());
            _cboProduct.DataSource = _products.ToList();
            _cboProduct.SelectedIndex = -1;
        }

        protected override void ResetEditor()
        {
            _lines.Clear();
            _txtNote.Clear();
            _cboProduct.SelectedIndex = -1;
            _cboProduct.Text = string.Empty;
            _dtpDate.Value = DateTime.Now;
            UpdateSummary();
            _btnLoadAll.Focus();
        }

        // ------------------------------------------------------------ Thao tác dòng
        private void LoadAllProducts()
        {
            if (_products.Count == 0)
            {
                MessageBox.Show("Chưa có sản phẩm nào để kiểm kê.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_lines.Count > 0)
            {
                var ask = MessageBox.Show("Danh sách hiện tại sẽ bị thay bằng toàn bộ sản phẩm. Tiếp tục?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes) return;
            }

            _lines.RaiseListChangedEvents = false;
            _lines.Clear();
            foreach (var p in _products)
                _lines.Add(NewLine(p));
            _lines.RaiseListChangedEvents = true;
            _lines.ResetBindings();
        }

        // Quét mã: tìm dòng của sản phẩm trong phiếu (chưa có thì thêm), nhảy tới ô "Thực tế" để gõ số đếm được.
        private bool OnScan(string code)
        {
            var product = FindByCode(_products, code);
            if (product is null) return false;

            var line = _lines.FirstOrDefault(l => l.ProductId == product.Id);
            if (line is null)
            {
                line = NewLine(product);
                _lines.Add(line);
            }

            int index = _lines.IndexOf(line);
            LineGrid.ClearSelection();
            LineGrid.CurrentCell = LineGrid.Rows[index].Cells[ColActual];
            LineGrid.Rows[index].Selected = true;
            _returnToScan = true;
            LineGrid.Focus();
            LineGrid.BeginEdit(true);
            return true;
        }

        private void AddSelectedProduct()
        {
            if (_cboProduct.SelectedItem is not ProductStockDTO product)
            {
                MessageBox.Show("Hãy chọn một sản phẩm trong danh sách.", "Chưa chọn sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboProduct.Focus();
                return;
            }
            if (_lines.Any(l => l.ProductId == product.Id))
            {
                MessageBox.Show($"\"{product.Name}\" đã có trong phiếu kiểm kê.", "Trùng sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _lines.Add(NewLine(product));
            _cboProduct.SelectedIndex = -1;
            _cboProduct.Text = string.Empty;
        }

        // Mặc định số thực tế = số hệ thống, người kiểm chỉ sửa những dòng bị lệch
        private static StockTakeLineItem NewLine(ProductStockDTO p) => new()
        {
            ProductId = p.Id,
            ProductName = p.Name,
            Barcode = p.Barcode ?? "",
            SystemQuantity = p.StockQuantity,
            ActualQuantity = p.StockQuantity
        };

        private void RemoveSelectedLine()
        {
            if (LineGrid.CurrentRow?.DataBoundItem is StockTakeLineItem item)
                _lines.Remove(item);
        }

        private void LineGrid_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != ColActual) return;

            var text = (e.FormattedValue?.ToString() ?? "").Trim();
            if (!int.TryParse(text, System.Globalization.NumberStyles.AllowThousands, Vi, out int value)
                || value < 0 || value > MaxQuantity)
            {
                e.Cancel = true;
                MessageBox.Show($"Số lượng thực tế phải là số nguyên từ 0 đến {MaxQuantity:N0}.", "Giá trị không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Tô đỏ chênh lệch âm (thiếu), cam chênh lệch dương (thừa)
        private void ColorDifference(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView grid || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Columns[e.ColumnIndex].DataPropertyName != "Difference") return;

            int diff = grid.Rows[e.RowIndex].DataBoundItem switch
            {
                StockTakeLineItem l => l.Difference,
                StockTakeLineDTO d => d.Difference,
                _ => 0
            };
            if (diff < 0) e.CellStyle!.ForeColor = DangerText;
            else if (diff > 0) e.CellStyle!.ForeColor = Color.FromArgb(200, 120, 0);
        }

        private void UpdateSummary()
        {
            int diffCount = _lines.Count(l => l.Difference != 0);
            SummaryLabel.Text = $"{_lines.Count} dòng  ·  {diffCount} dòng lệch";
            SummaryLabel.ForeColor = diffCount > 0 ? DangerText : AppTheme.TextDark;
        }

        // ------------------------------------------------------------ Lưu phiếu
        protected override async Task SaveDocumentAsync()
        {
            LineGrid.EndEdit();   // chốt ô đang gõ dở

            if (_lines.Count == 0)
            {
                MessageBox.Show("Phiếu kiểm kê chưa có sản phẩm nào. Hãy nạp tất cả hoặc thêm từng sản phẩm.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int diffCount = _lines.Count(l => l.Difference != 0);
            var confirm = MessageBox.Show($"Lưu phiếu kiểm kê {_lines.Count} dòng, trong đó {diffCount} dòng lệch? Tồn kho các dòng lệch sẽ được cập nhật bằng số thực tế.", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var userId = AppSession.RequireUserId();
            var date = _dtpDate.Value;
            var note = _txtNote.Text;
            var inputs = _lines.Select(l => new StockTakeLineInput(l.ProductId, l.ActualQuantity)).ToList();

            var result = await Run<IInventoryService, InventoryResultDTO>(s => s.CreateStockTakeAsync(userId, date, note, inputs));
            await AfterSavedAsync(result, $"Đã lưu phiếu kiểm kê #{result.DocumentId}. Tồn kho đã được điều chỉnh theo số thực tế.");
        }

        protected override void SetBusy(bool busy)
        {
            base.SetBusy(busy);
            _btnAddLine.Enabled = _btnLoadAll.Enabled = _btnRemoveLine.Enabled = !busy;
        }
    }

    // Một dòng của phiếu kiểm kê đang lập
    internal sealed class StockTakeLineItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Barcode { get; set; } = "";
        public int SystemQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int Difference => ActualQuantity - SystemQuantity;
    }
}
