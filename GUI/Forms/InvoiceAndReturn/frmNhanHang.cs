using BLL.DTOs.Return;
using BLL.Services.Interfaces;
using Common.Enums;
using GUI.Themes;
using GUI.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace GUI.Forms.InvoiceAndReturn
{
    /// <summary>
    /// Màn hình Nhận hàng dành cho Nhân viên kho:
    /// Chỉ hiện các yêu cầu kênh Shipping đã duyệt (Approved),
    /// nhân viên kho kiểm tra tình trạng từng món (Nguyên vẹn, Hư hỏng, Lỗi kỹ thuật),
    /// chọn nhập lại kho bán hay kho lỗi, và xác nhận nhận hàng.
    /// </summary>
    [DesignerCategory("Code")]
    public class frmNhanHang : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Header
        private readonly Panel _pnlHeader = new();
        private readonly Label _lblTitle = new();

        // Left Panel: Danh sách yêu cầu Shipping đã duyệt
        private readonly Panel _pnlLeft = new();
        private readonly Label _lblListTitle = new();
        private readonly DataGridView _dgvShippingRequests = new();

        // Right Panel: Kiểm tra tình trạng sản phẩm & Xác nhận
        private readonly Panel _pnlRight = new();
        private readonly Label _lblDetailTitle = new();
        private readonly Label _lblRequestInfo = new();
        private readonly DataGridView _dgvItemsToReceive = new();
        private readonly Button _btnXacNhanNhanHang = new();

        private ReturnRequestDto? _selectedRequest;

        public frmNhanHang(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Xác nhận nhận hàng đổi trả (Kho hàng) - Tech Store POS";
            Font = InvoiceTheme.BodyFont;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1220, 700);
            MinimumSize = new Size(1000, 600);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = InvoiceTheme.FormBg;

            BuildUi();

            Load += async (_, _) => await RunSafeAsync(LoadShippingRequestsAsync);
        }

        private void BuildUi()
        {
            // -------------------------------------------------------------
            // 1. TOP HEADER
            // -------------------------------------------------------------
            _pnlHeader.Dock = DockStyle.Top;
            _pnlHeader.Height = 65;
            _pnlHeader.BackColor = InvoiceTheme.SidebarDark;
            _pnlHeader.Padding = new Padding(20, 0, 20, 0);

            _lblTitle.Text = "KHO HÀNG: XÁC NHẬN NHẬN HÀNG ĐỔI TRẢ (KÊNH SHIPPING)";
            _lblTitle.UseMnemonic = false;
            _lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.White;
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(20, 18);
            _pnlHeader.Controls.Add(_lblTitle);

            Controls.Add(_pnlHeader);

            // -------------------------------------------------------------
            // 2. MAIN SPLIT: BÊN TRÁI (Yêu cầu chờ nhận) | BÊN PHẢI (Kiểm tra hàng)
            // -------------------------------------------------------------
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45)); // Trái: 45%
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55)); // Phải: 55%

            // Left
            _pnlLeft.Dock = DockStyle.Fill;
            _pnlLeft.BackColor = InvoiceTheme.CardBg;
            _pnlLeft.Padding = new Padding(15);

            var pnlLeftTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38
            };

            _lblListTitle.Text = "YÊU CẦU SHIPPING ĐÃ DUYỆT (CHỜ KHO NHẬN):";
            _lblListTitle.UseMnemonic = false;
            _lblListTitle.Font = InvoiceTheme.BodyBoldFont;
            _lblListTitle.ForeColor = InvoiceTheme.Primary;
            _lblListTitle.Dock = DockStyle.Fill;
            _lblListTitle.TextAlign = ContentAlignment.MiddleLeft;

            var btnRefresh = new Button
            {
                Text = "🔄 Làm mới",
                Size = new Size(100, 32),
                Dock = DockStyle.Right,
                BackColor = InvoiceTheme.Secondary,
                ForeColor = InvoiceTheme.SecondaryText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = InvoiceTheme.BodyBoldFont
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (_, _) => await RunSafeAsync(LoadShippingRequestsAsync);

            pnlLeftTop.Controls.Add(_lblListTitle);
            pnlLeftTop.Controls.Add(btnRefresh);

            _dgvShippingRequests.Dock = DockStyle.Fill;
            UiHelper.ApplyFlatGridStyle(_dgvShippingRequests);
            SetupShippingGridColumns();
            _dgvShippingRequests.SelectionChanged += async (_, _) => await OnShippingSelectedAsync();

            _pnlLeft.Controls.Clear();
            _pnlLeft.Controls.AddRange([pnlLeftTop, _dgvShippingRequests]);
            _pnlLeft.Controls.SetChildIndex(pnlLeftTop, 0);
            _pnlLeft.Controls.SetChildIndex(_dgvShippingRequests, 1);

            // Right
            _pnlRight.Dock = DockStyle.Fill;
            _pnlRight.BackColor = InvoiceTheme.CardBg;
            _pnlRight.Padding = new Padding(15);

            _lblDetailTitle.Text = "KIỂM TRA TÌNH TRẠNG VÀ PHÂN LOẠI KHO:";
            _lblDetailTitle.UseMnemonic = false;
            _lblDetailTitle.Font = InvoiceTheme.BodyBoldFont;
            _lblDetailTitle.ForeColor = InvoiceTheme.Primary;
            _lblDetailTitle.Dock = DockStyle.Top;
            _lblDetailTitle.Height = 35;

            _lblRequestInfo.Font = InvoiceTheme.BodyFont;
            _lblRequestInfo.UseMnemonic = false;
            _lblRequestInfo.ForeColor = Color.FromArgb(71, 85, 105);
            _lblRequestInfo.Dock = DockStyle.Top;
            _lblRequestInfo.Height = 55;
            _lblRequestInfo.Text = "Chọn một yêu cầu bên trái để kiểm hàng...";

            _dgvItemsToReceive.Dock = DockStyle.Fill;
            UiHelper.ApplyFlatGridStyle(_dgvItemsToReceive);
            _dgvItemsToReceive.RowTemplate.Height = 44;
            SetupItemsToReceiveGridColumns();
            _dgvItemsToReceive.CellValueChanged += DgvItemsToReceive_CellValueChanged;
            _dgvItemsToReceive.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_dgvItemsToReceive.IsCurrentCellDirty)
                    _dgvItemsToReceive.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            var pnlRightBottom = new Panel { Dock = DockStyle.Bottom, Height = 65, Padding = new Padding(0, 15, 0, 0) };
            _btnXacNhanNhanHang.Text = "XÁC NHẬN ĐÃ NHẬN ĐỦ HÀNG";
            _btnXacNhanNhanHang.Dock = DockStyle.Fill;
            _btnXacNhanNhanHang.BackColor = InvoiceTheme.Primary;
            _btnXacNhanNhanHang.ForeColor = Color.White;
            _btnXacNhanNhanHang.FlatStyle = FlatStyle.Flat;
            _btnXacNhanNhanHang.FlatAppearance.BorderSize = 0;
            _btnXacNhanNhanHang.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _btnXacNhanNhanHang.Cursor = Cursors.Hand;
            _btnXacNhanNhanHang.Enabled = false;
            _btnXacNhanNhanHang.Click += async (_, _) => await ReceiveAsync();
            pnlRightBottom.Controls.Add(_btnXacNhanNhanHang);

            _pnlRight.Controls.Clear();
            _pnlRight.Controls.AddRange([_lblDetailTitle, _lblRequestInfo, pnlRightBottom, _dgvItemsToReceive]);
            _pnlRight.Controls.SetChildIndex(_lblDetailTitle, 0);
            _pnlRight.Controls.SetChildIndex(_lblRequestInfo, 1);
            _pnlRight.Controls.SetChildIndex(pnlRightBottom, 2);
            _pnlRight.Controls.SetChildIndex(_dgvItemsToReceive, 3);

            split.Controls.Add(_pnlLeft, 0, 0);
            split.Controls.Add(_pnlRight, 1, 0);

            Controls.Add(split);
            split.BringToFront();
        }

        private void SetupShippingGridColumns()
        {
            _dgvShippingRequests.Columns.Clear();
            _dgvShippingRequests.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            _dgvShippingRequests.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", DataPropertyName = "Id", Visible = false });

            _dgvShippingRequests.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReqNo",
                HeaderText = "Mã YC",
                DataPropertyName = "RequestNo",
                Width = 135
            });

            _dgvShippingRequests.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colInvoiceNo",
                HeaderText = "Hóa đơn",
                DataPropertyName = "InvoiceNo",
                Width = 120
            });

            _dgvShippingRequests.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCustomer",
                HeaderText = "Khách hàng",
                DataPropertyName = "CustomerName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 140
            });

            _dgvShippingRequests.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCreatedAt",
                HeaderText = "Ngày tạo",
                DataPropertyName = "CreatedAt",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "dd/MM/yyyy" },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                Width = 115
            });
        }

        private void SetupItemsToReceiveGridColumns()
        {
            _dgvItemsToReceive.Columns.Clear();
            _dgvItemsToReceive.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            _dgvItemsToReceive.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReturnItemId",
                DataPropertyName = "ReturnItemId",
                Visible = false
            });

            _dgvItemsToReceive.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colProductName",
                HeaderText = "Sản phẩm",
                DataPropertyName = "ProductName",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 160
            });

            _dgvItemsToReceive.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colQty",
                HeaderText = "SL",
                DataPropertyName = "Qty",
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                Width = 60
            });

            // ComboBox chọn tình trạng: Good, Defective, Damaged
            var cboCondCol = new DataGridViewComboBoxColumn
            {
                Name = "colCondition",
                HeaderText = "Tình trạng hàng",
                DataPropertyName = "Condition",
                Width = 180
            };
            cboCondCol.Items.AddRange("Nguyên vẹn (Good)", "Lỗi kỹ thuật (Defective)", "Hư hỏng (Damaged)");
            _dgvItemsToReceive.Columns.Add(cboCondCol);

            // Checkbox Nhập lại kho bán (chỉ tick được khi Good)
            var chkRestock = new DataGridViewCheckBoxColumn
            {
                Name = "colRestock",
                HeaderText = "Nhập lại kho bán",
                DataPropertyName = "Restock",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                Width = 150
            };
            _dgvItemsToReceive.Columns.Add(chkRestock);
        }

        private async Task LoadShippingRequestsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();

            // Lấy các yêu cầu ở trạng thái Approved
            var (items, _) = await returnService.SearchAsync(ReturnStatus.Approved, null, 0, 100);

            // Chỉ lấy kênh Shipping
            var shippingApproved = items.Where(x => x.Channel == ReturnChannel.Shipping).ToList();

            _dgvShippingRequests.DataSource = shippingApproved.Select(x => new
            {
                x.Id,
                x.RequestNo,
                x.InvoiceNo,
                CustomerName = x.CustomerName ?? "Khách lẻ",
                x.CreatedAt
            }).ToList();

            if (shippingApproved.Count == 0)
            {
                _lblRequestInfo.Text = "Hiện không có yêu cầu Shipping nào đang chờ nhập kho.";
                _dgvItemsToReceive.DataSource = null;
                _btnXacNhanNhanHang.Enabled = false;
            }
        }

        private async Task OnShippingSelectedAsync()
        {
            if (_dgvShippingRequests.CurrentRow == null) return;
            var cellValue = _dgvShippingRequests.CurrentRow.Cells["colId"].Value;
            if (cellValue == null || cellValue == DBNull.Value) return;
            int id = (int)cellValue;

            using var scope = _scopeFactory.CreateScope();
            var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();

            _selectedRequest = await returnService.GetDetailAsync(id);
            if (_selectedRequest == null) return;

            _lblRequestInfo.Text =
                $"Mã YC: {_selectedRequest.RequestNo} | Hóa đơn: {_selectedRequest.InvoiceNo} | Khách: {_selectedRequest.CustomerName}\n" +
                $"Lý do đổi trả: {_selectedRequest.ReasonCode} - {_selectedRequest.ReasonNote}";

            var rows = _selectedRequest.Items.Select(i => new ReceiveItemRow
            {
                ReturnItemId = i.Id,
                ProductName = i.ProductName,
                Qty = i.Qty,
                Condition = i.Condition switch
                {
                    ItemCondition.Good => "Nguyên vẹn (Good)",
                    ItemCondition.Defective => "Lỗi kỹ thuật (Defective)",
                    ItemCondition.Damaged => "Hư hỏng (Damaged)",
                    _ => "Nguyên vẹn (Good)"
                },
                Restock = i.Restock
            }).ToList();

            _dgvItemsToReceive.DataSource = rows;
            _btnXacNhanNhanHang.Enabled = true;
        }

        private void DgvItemsToReceive_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _dgvItemsToReceive.Rows[e.RowIndex].DataBoundItem as ReceiveItemRow;
            if (row == null) return;

            // Nếu tình trạng không phải Good -> Tự động bỏ tick Nhập lại kho bán
            if (_dgvItemsToReceive.Columns["colCondition"] is { } col && e.ColumnIndex == col.Index)
            {
                if (row.Condition != "Nguyên vẹn (Good)")
                {
                    row.Restock = false;
                }
                else
                {
                    row.Restock = true;
                }
                _dgvItemsToReceive.InvalidateRow(e.RowIndex);
            }
        }

        private async Task ReceiveAsync()
        {
            if (_selectedRequest == null) return;
            if (_dgvItemsToReceive.DataSource is not List<ReceiveItemRow> rows) return;

            var itemsDto = rows.Select(r => new ReceiveReturnItemDto
            {
                ReturnItemId = r.ReturnItemId,
                Condition = r.Condition switch
                {
                    "Nguyên vẹn (Good)" => ItemCondition.Good,
                    "Lỗi kỹ thuật (Defective)" => ItemCondition.Defective,
                    "Hư hỏng (Damaged)" => ItemCondition.Damaged,
                    _ => ItemCondition.Good
                },
                Restock = r.Restock
            }).ToList();

            var confirm = MessageBox.Show(
                $"Xác nhận đã nhận và phân loại xong hàng cho yêu cầu {_selectedRequest.RequestNo}?\n" +
                "Yêu cầu sẽ được chuyển sang trạng thái 'Đã nhận hàng' (Received).",
                "Xác nhận nhận hàng", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            await RunSafeAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();
                await returnService.ReceiveAsync(_selectedRequest.Id, itemsDto);

                MessageBox.Show("Xác nhận nhận hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadShippingRequestsAsync();
            });
        }

        private async Task RunSafeAsync(Func<Task> action)
        {
            Cursor = Cursors.WaitCursor;
            Enabled = false;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                UiHelper.HandleException(ex);
            }
            finally
            {
                Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private sealed class ReceiveItemRow
        {
            public int ReturnItemId { get; set; }
            public string ProductName { get; set; } = "";
            public int Qty { get; set; }
            public string Condition { get; set; } = "Nguyên vẹn (Good)";
            public bool Restock { get; set; } = true;
        }
    }
}
