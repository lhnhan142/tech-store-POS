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
    /// Màn hình Quản lý yêu cầu đổi trả (lọc theo trạng thái kèm badge số lượng, xem chi tiết,
    /// duyệt, từ chối kèm lý do bắt buộc, hoàn tất theo phân quyền).
    /// </summary>
    [DesignerCategory("Code")]
    public class frmYeuCauDoiTra : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Header & Debug role
        private readonly Panel _pnlHeader = new();
        private readonly Label _lblTitle = new();

        // Tabs bộ lọc trạng thái
        private readonly FlowLayoutPanel _flpTabs = new();
        private readonly Dictionary<string, Button> _tabButtons = new();
        private ReturnStatus? _currentStatusFilter = null;

        // Tìm kiếm & Grid
        private readonly Panel _pnlSearch = new();
        private readonly TextBox _txtTimKiem = new();
        private readonly Button _btnTim = new();
        private readonly Button _btnLamMoi = new();
        private readonly DataGridView _dgvYeuCau = new();

        // Panel chi tiết bên phải
        private readonly Panel _pnlDetail = new();
        private readonly Label _lblDetailTitle = new();
        private readonly Label _lblDetailInfo = new();
        private readonly DataGridView _dgvDetailItems = new();
        private readonly Panel _pnlActions = new();
        private readonly Button _btnDuyet = new();
        private readonly Button _btnTuChoi = new();
        private readonly Button _btnHoanTat = new();
        private readonly Button _btnHuyYeuCau = new();

        private ReturnRequestDto? _selectedRequest;

        public frmYeuCauDoiTra(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            Text = "Xử lý Yêu cầu Đổi trả - Tech Store POS";
            Font = InvoiceTheme.BodyFont;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1300, 760);
            MinimumSize = new Size(1100, 650);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = InvoiceTheme.FormBg;

            BuildUi();

            Load += async (_, _) =>
            {
                await RunSafeAsync(async () =>
                {
                    await RefreshBadgesAsync();
                    await LoadDataAsync();
                });
            };
        }

        private void BuildUi()
        {
            // -------------------------------------------------------------
            // 1. TOP HEADER PANEL
            // -------------------------------------------------------------
            _pnlHeader.Dock = DockStyle.Top;
            _pnlHeader.Height = 65;
            _pnlHeader.BackColor = InvoiceTheme.SidebarDark;
            _pnlHeader.Padding = new Padding(20, 0, 20, 0);

            _lblTitle.Text = "QUẢN LÝ YÊU CẦU ĐỔI TRẢ";
            _lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.White;
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(20, 18);
            _pnlHeader.Controls.Add(_lblTitle);

            Controls.Add(_pnlHeader);

            // -------------------------------------------------------------
            // 2. TABS PANEL (Bộ lọc trạng thái có số lượng badge)
            // -------------------------------------------------------------
            _flpTabs.Dock = DockStyle.Top;
            _flpTabs.Height = 52;
            _flpTabs.BackColor = InvoiceTheme.CardBg;
            _flpTabs.Padding = new Padding(15, 6, 15, 6);

            AddTabButton("Tất cả (0)", null);
            AddTabButton("Chờ duyệt (0)", ReturnStatus.Pending);
            AddTabButton("Đã duyệt (0)", ReturnStatus.Approved);
            AddTabButton("Đã nhận hàng (0)", ReturnStatus.Received);
            AddTabButton("Đã hoàn tất (0)", ReturnStatus.Completed);
            AddTabButton("Đã từ chối (0)", ReturnStatus.Rejected);
            AddTabButton("Đã hủy (0)", ReturnStatus.Cancelled);

            UpdateTabButtonStyles();
            Controls.Add(_flpTabs);

            // -------------------------------------------------------------
            // 3. MAIN SPLIT: BÊN TRÁI (Grid DS Yêu cầu), BÊN PHẢI (Chi tiết & Nút duyệt)
            // -------------------------------------------------------------
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(15)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Bên trái: 60%
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Bên phải: 40%

            // --- VÙNG TRÁI: Tìm kiếm + DataGridView ---
            var pnlLeft = new Panel { Dock = DockStyle.Fill, BackColor = InvoiceTheme.CardBg, Padding = new Padding(12) };

            var flpSearch = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 4, 0, 4),
                WrapContents = false,
                AutoScroll = false
            };

            _txtTimKiem.PlaceholderText = "Tìm theo mã yêu cầu (YC-...), số HĐ, SĐT...";
            _txtTimKiem.Size = new Size(320, 36);
            _txtTimKiem.Margin = new Padding(0, 2, 8, 0);
            _txtTimKiem.Font = InvoiceTheme.BodyFont;

            _btnTim.Text = "Tìm";
            _btnTim.Size = new Size(80, 36);
            _btnTim.Margin = new Padding(0, 1, 8, 0);
            _btnTim.BackColor = InvoiceTheme.Primary;
            _btnTim.ForeColor = Color.White;
            _btnTim.FlatStyle = FlatStyle.Flat;
            _btnTim.FlatAppearance.BorderSize = 0;
            _btnTim.Click += async (_, _) => await RunSafeAsync(LoadDataAsync);

            _btnLamMoi.Text = "Làm mới";
            _btnLamMoi.Size = new Size(95, 36);
            _btnLamMoi.Margin = new Padding(0, 1, 0, 0);
            _btnLamMoi.BackColor = InvoiceTheme.Secondary;
            _btnLamMoi.ForeColor = InvoiceTheme.SecondaryText;
            _btnLamMoi.FlatStyle = FlatStyle.Flat;
            _btnLamMoi.FlatAppearance.BorderSize = 0;
            _btnLamMoi.Click += async (_, _) =>
            {
                _txtTimKiem.Clear();
                await RunSafeAsync(async () =>
                {
                    await RefreshBadgesAsync();
                    await LoadDataAsync();
                });
            };

            flpSearch.Controls.AddRange([_txtTimKiem, _btnTim, _btnLamMoi]);
            pnlLeft.Controls.Add(flpSearch);

            _dgvYeuCau.Dock = DockStyle.Fill;
            UiHelper.ApplyFlatGridStyle(_dgvYeuCau);
            SetupGridColumns();
            _dgvYeuCau.SelectionChanged += async (_, _) => await OnRowSelectedAsync();
            _dgvYeuCau.CellPainting += DgvYeuCau_CellPainting;

            pnlLeft.Controls.Add(_dgvYeuCau);
            _dgvYeuCau.BringToFront();

            split.Controls.Add(pnlLeft, 0, 0);

            // --- VÙNG PHẢI: Panel chi tiết ---
            BuildDetailPanel();
            split.Controls.Add(_pnlDetail, 1, 0);

            Controls.Add(split);
            split.BringToFront();
        }

        private void AddTabButton(string text, ReturnStatus? status)
        {
            string key = status?.ToString() ?? "All";
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                Font = InvoiceTheme.BodyFont,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += async (_, _) =>
            {
                _currentStatusFilter = status;
                UpdateTabButtonStyles();
                await RunSafeAsync(LoadDataAsync);
            };

            _tabButtons[key] = btn;
            _flpTabs.Controls.Add(btn);
        }

        private void UpdateTabButtonStyles()
        {
            string currentKey = _currentStatusFilter?.ToString() ?? "All";
            foreach (var (key, btn) in _tabButtons)
            {
                bool active = key == currentKey;
                btn.BackColor = active ? InvoiceTheme.Primary : Color.Transparent;
                btn.ForeColor = active ? Color.White : InvoiceTheme.SecondaryText;
                btn.Font = active ? InvoiceTheme.BodyBoldFont : InvoiceTheme.BodyFont;
            }
        }

        private void SetupGridColumns()
        {
            _dgvYeuCau.Columns.Clear();

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", DataPropertyName = "Id", Visible = false });

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRequestNo",
                HeaderText = "Mã yêu cầu",
                DataPropertyName = "RequestNo",
                Width = 140
            });

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colInvoiceNo",
                HeaderText = "Số HĐ",
                DataPropertyName = "InvoiceNo",
                Width = 115
            });

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = "Loại",
                DataPropertyName = "TypeText",
                Width = 90
            });

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colChannel",
                HeaderText = "Kênh",
                DataPropertyName = "ChannelText",
                Width = 90
            });

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTotalRefund",
                HeaderText = "Tiền hoàn",
                DataPropertyName = "TotalRefundText",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = InvoiceTheme.BodyBoldFont },
                Width = 140
            });

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "Trạng thái",
                DataPropertyName = "StatusText",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 110
            });

            _dgvYeuCau.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatusRaw",
                DataPropertyName = "StatusRaw",
                Visible = false
            });
        }

        private void BuildDetailPanel()
        {
            _pnlDetail.Dock = DockStyle.Fill;
            _pnlDetail.BackColor = InvoiceTheme.CardBg;
            _pnlDetail.Padding = new Padding(15);

            _lblDetailTitle.Text = "CHI TIẾT YÊU CẦU";
            _lblDetailTitle.UseMnemonic = false;
            _lblDetailTitle.Font = InvoiceTheme.SubHeaderFont;
            _lblDetailTitle.ForeColor = InvoiceTheme.Primary;
            _lblDetailTitle.Dock = DockStyle.Top;
            _lblDetailTitle.Height = 35;

            _lblDetailInfo.Font = InvoiceTheme.BodyFont;
            _lblDetailInfo.ForeColor = Color.FromArgb(30, 41, 59);
            _lblDetailInfo.Dock = DockStyle.Top;
            _lblDetailInfo.Height = 135;
            _lblDetailInfo.Text = "Chọn một yêu cầu bên trái để xem chi tiết...";

            // Bảng sản phẩm trong yêu cầu
            var lblGridTitle = new Label
            {
                Text = "SẢN PHẨM ĐỔI/TRẢ:",
                Font = InvoiceTheme.BodyBoldFont,
                ForeColor = Color.FromArgb(71, 85, 105),
                Dock = DockStyle.Top,
                Height = 30
            };

            _dgvDetailItems.Dock = DockStyle.Fill;
            UiHelper.ApplyFlatGridStyle(_dgvDetailItems);
            _dgvDetailItems.RowTemplate.Height = 36;
            _dgvDetailItems.ColumnHeadersHeight = 36;
            SetupDetailGridColumns();

            // Panel nút tác vụ bên dưới
            _pnlActions.Dock = DockStyle.Bottom;
            _pnlActions.Height = 110;
            _pnlActions.Padding = new Padding(0, 10, 0, 0);

            _btnDuyet.Text = "Phê duyệt";
            _btnDuyet.Size = new Size(130, 42);
            _btnDuyet.Location = new Point(0, 10);
            _btnDuyet.BackColor = InvoiceTheme.Primary;
            _btnDuyet.ForeColor = Color.White;
            _btnDuyet.FlatStyle = FlatStyle.Flat;
            _btnDuyet.FlatAppearance.BorderSize = 0;
            _btnDuyet.Font = InvoiceTheme.BodyBoldFont;
            _btnDuyet.Cursor = Cursors.Hand;
            _btnDuyet.Click += async (_, _) => await ApproveAsync();

            _btnTuChoi.Text = "Từ chối";
            _btnTuChoi.Size = new Size(115, 42);
            _btnTuChoi.Location = new Point(140, 10);
            _btnTuChoi.BackColor = InvoiceTheme.Secondary;
            _btnTuChoi.ForeColor = InvoiceTheme.Danger;
            _btnTuChoi.FlatStyle = FlatStyle.Flat;
            _btnTuChoi.FlatAppearance.BorderColor = InvoiceTheme.Danger;
            _btnTuChoi.Font = InvoiceTheme.BodyBoldFont;
            _btnTuChoi.Cursor = Cursors.Hand;
            _btnTuChoi.Click += async (_, _) => await RejectAsync();

            _btnHuyYeuCau.Text = "Hủy";
            _btnHuyYeuCau.Size = new Size(95, 42);
            _btnHuyYeuCau.Location = new Point(265, 10);
            _btnHuyYeuCau.BackColor = InvoiceTheme.Secondary;
            _btnHuyYeuCau.ForeColor = InvoiceTheme.SecondaryText;
            _btnHuyYeuCau.FlatStyle = FlatStyle.Flat;
            _btnHuyYeuCau.FlatAppearance.BorderSize = 0;
            _btnHuyYeuCau.Font = InvoiceTheme.BodyBoldFont;
            _btnHuyYeuCau.Cursor = Cursors.Hand;
            _btnHuyYeuCau.Click += async (_, _) => await CancelAsync();

            _btnHoanTat.Text = "Hoàn tất xử lý & Hoàn tiền";
            _btnHoanTat.UseMnemonic = false;
            _btnHoanTat.Size = new Size(360, 44);
            _btnHoanTat.Location = new Point(0, 58);
            _btnHoanTat.BackColor = Color.FromArgb(5, 150, 105);
            _btnHoanTat.ForeColor = Color.White;
            _btnHoanTat.FlatStyle = FlatStyle.Flat;
            _btnHoanTat.FlatAppearance.BorderSize = 0;
            _btnHoanTat.Font = InvoiceTheme.BodyBoldFont;
            _btnHoanTat.Cursor = Cursors.Hand;
            _btnHoanTat.Click += async (_, _) => await CompleteAsync();

            _pnlActions.Controls.Clear();
            _pnlActions.Controls.AddRange([_btnDuyet, _btnTuChoi, _btnHuyYeuCau, _btnHoanTat]);

            _pnlDetail.Controls.Clear();
            _pnlDetail.Controls.AddRange([_lblDetailTitle, _lblDetailInfo, lblGridTitle, _pnlActions, _dgvDetailItems]);
            _pnlDetail.Controls.SetChildIndex(_lblDetailTitle, 0);
            _pnlDetail.Controls.SetChildIndex(_lblDetailInfo, 1);
            _pnlDetail.Controls.SetChildIndex(lblGridTitle, 2);
            _pnlDetail.Controls.SetChildIndex(_pnlActions, 3);
            _pnlDetail.Controls.SetChildIndex(_dgvDetailItems, 4);
        }

        private void SetupDetailGridColumns()
        {
            _dgvDetailItems.Columns.Clear();

            _dgvDetailItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colProdName",
                HeaderText = "Sản phẩm",
                DataPropertyName = "ProductName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _dgvDetailItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colQty",
                HeaderText = "SL",
                DataPropertyName = "Qty",
                Width = 50,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _dgvDetailItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCond",
                HeaderText = "Tình trạng",
                DataPropertyName = "ConditionText",
                Width = 100
            });

            _dgvDetailItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRefund",
                HeaderText = "Tiền hoàn",
                DataPropertyName = "RefundAmountText",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Width = 120
            });
        }

        private void DgvYeuCau_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && _dgvYeuCau.Columns["colStatus"] is { } col && e.ColumnIndex == col.Index)
            {
                string statusText = e.Value?.ToString() ?? "";
                string statusCode = _dgvYeuCau.Rows[e.RowIndex].Cells["colStatusRaw"].Value?.ToString() ?? "";
                var (bg, text) = InvoiceTheme.GetStatusColors(statusCode);
                UiHelper.PaintStatusCell(e, statusText, bg, text);
            }
        }

        private async Task RefreshBadgesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();
            var counts = await returnService.GetStatusCountsAsync();

            int total = counts.Values.Sum();
            if (_tabButtons.TryGetValue("All", out var btnAll))
                btnAll.Text = $"Tất cả ({total})";

            if (_tabButtons.TryGetValue("Pending", out var btnPending))
                btnPending.Text = $"Chờ duyệt ({counts.GetValueOrDefault(ReturnStatus.Pending)})";
            if (_tabButtons.TryGetValue("Approved", out var btnAppr))
                btnAppr.Text = $"Đã duyệt ({counts.GetValueOrDefault(ReturnStatus.Approved)})";
            if (_tabButtons.TryGetValue("Received", out var btnRecv))
                btnRecv.Text = $"Đã nhận hàng ({counts.GetValueOrDefault(ReturnStatus.Received)})";
            if (_tabButtons.TryGetValue("Completed", out var btnComp))
                btnComp.Text = $"Đã hoàn tất ({counts.GetValueOrDefault(ReturnStatus.Completed)})";
            if (_tabButtons.TryGetValue("Rejected", out var btnRej))
                btnRej.Text = $"Đã từ chối ({counts.GetValueOrDefault(ReturnStatus.Rejected)})";
            if (_tabButtons.TryGetValue("Cancelled", out var btnCanc))
                btnCanc.Text = $"Đã hủy ({counts.GetValueOrDefault(ReturnStatus.Cancelled)})";
        }

        private async Task LoadDataAsync()
        {
            string? kw = string.IsNullOrWhiteSpace(_txtTimKiem.Text) ? null : _txtTimKiem.Text.Trim();

            using var scope = _scopeFactory.CreateScope();
            var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();

            var (items, _) = await returnService.SearchAsync(_currentStatusFilter, kw, 0, 100);

            var rows = items.Select(x => new
            {
                x.Id,
                x.RequestNo,
                x.InvoiceNo,
                TypeText = x.Type == ReturnType.Return ? "Trả hàng" : "Đổi hàng",
                ChannelText = x.Channel == ReturnChannel.Counter ? "Tại quầy" : "Shipping",
                TotalRefundText = x.TotalRefund.ToString("N0") + "đ",
                StatusText = GetStatusVietnamese(x.Status),
                StatusRaw = x.Status.ToString()
            }).ToList();

            _dgvYeuCau.DataSource = rows;

            if (rows.Count == 0)
            {
                ClearDetail();
            }
        }

        private async Task OnRowSelectedAsync()
        {
            if (_dgvYeuCau.CurrentRow == null || _dgvYeuCau.CurrentRow.Cells["colId"].Value is not int id)
            {
                ClearDetail();
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();
            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

            _selectedRequest = await returnService.GetDetailAsync(id);
            if (_selectedRequest == null)
            {
                ClearDetail();
                return;
            }

            _lblDetailTitle.Text = $"YÊU CẦU {_selectedRequest.RequestNo} - {GetStatusVietnamese(_selectedRequest.Status)}";

            string rejectText = string.IsNullOrWhiteSpace(_selectedRequest.RejectReason)
                ? "" : $"\nLý do từ chối: {_selectedRequest.RejectReason}";

            _lblDetailInfo.Text =
                $"Hóa đơn gốc: {_selectedRequest.InvoiceNo} | Khách: {_selectedRequest.CustomerName ?? "Khách lẻ"}\n" +
                $"Loại: {(_selectedRequest.Type == ReturnType.Return ? "Trả hoàn tiền" : "Đổi sản phẩm")} | Kênh: {(_selectedRequest.Channel == ReturnChannel.Counter ? "Tại quầy" : "Gửi hàng/Vận chuyển")}\n" +
                $"Lý do: {_selectedRequest.ReasonCode} - {_selectedRequest.ReasonNote}\n" +
                $"Người tạo: {_selectedRequest.CreatedByUserName} ({_selectedRequest.CreatedAt:dd/MM/yyyy HH:mm})\n" +
                $"Tổng tiền hoàn: {_selectedRequest.TotalRefund:N0}đ{rejectText}";

            // Grid sản phẩm
            _dgvDetailItems.DataSource = _selectedRequest.Items.Select(i => new
            {
                i.ProductName,
                i.Qty,
                ConditionText = i.Condition switch
                {
                    ItemCondition.Good => "Nguyên vẹn",
                    ItemCondition.Damaged => "Hư hỏng",
                    ItemCondition.Defective => "Lỗi kỹ thuật",
                    _ => i.Condition.ToString()
                },
                RefundAmountText = i.RefundAmount.ToString("N0") + "đ"
            }).ToList();

            // Cập nhật trạng thái các nút theo vai trò và vòng đời
            ApplyActionPermissions(currentUser, _selectedRequest);
        }

        private void ApplyActionPermissions(ICurrentUserService user, ReturnRequestDto req)
        {
            // Mặc định ẩn/tắt hết
            _btnDuyet.Enabled = _btnTuChoi.Enabled = _btnHuyYeuCau.Enabled = _btnHoanTat.Enabled = false;

            if (req.Status == ReturnStatus.Pending)
            {
                // Pending: được Duyệt, Từ chối, Hủy
                _btnHuyYeuCau.Enabled = user.IsAdmin || user.IsManager || req.CreatedByUserId == user.UserId;
                _btnTuChoi.Enabled = user.IsAdmin || user.IsManager;

                // Seller chỉ duyệt được Counter dưới ngưỡng, Quản lý/Admin duyệt được mọi cái
                if (user.IsAdmin || user.IsManager)
                {
                    _btnDuyet.Enabled = true;
                }
                else if (user.IsSeller && req.Channel == ReturnChannel.Counter)
                {
                    _btnDuyet.Enabled = true;
                }
            }
            else if (req.Status == ReturnStatus.Approved)
            {
                // Đã duyệt (thường là kênh Shipping): Từ chối
                _btnTuChoi.Enabled = user.IsAdmin || user.IsManager;
            }
            else if (req.Status == ReturnStatus.Received)
            {
                // Đã nhận hàng: nút HOÀN TẤT sáng lên (Quản lý, Admin hoặc Bán hàng)
                _btnHoanTat.Enabled = !user.IsWarehouse;
            }
        }

        private void ClearDetail()
        {
            _selectedRequest = null;
            _lblDetailTitle.Text = "CHI TIẾT YÊU CẦU";
            _lblDetailInfo.Text = "Chọn một yêu cầu bên trái để xem chi tiết...";
            _dgvDetailItems.DataSource = null;
            _btnDuyet.Enabled = _btnTuChoi.Enabled = _btnHuyYeuCau.Enabled = _btnHoanTat.Enabled = false;
        }

        private async Task ApproveAsync()
        {
            if (_selectedRequest == null) return;

            var confirm = MessageBox.Show(
                $"Xác nhận phê duyệt yêu cầu {_selectedRequest.RequestNo}?",
                "Phê duyệt", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            await RunSafeAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();
                await returnService.ApproveAsync(_selectedRequest.Id);

                MessageBox.Show("Đã phê duyệt yêu cầu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshBadgesAsync();
                await LoadDataAsync();
            });
        }

        private async Task RejectAsync()
        {
            if (_selectedRequest == null) return;

            // Bắt buộc nhập lý do từ chối
            string reason = PromptDialog.Show("Nhập lý do từ chối yêu cầu đổi trả (bắt buộc):", "Từ chối yêu cầu");
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Lý do từ chối là bắt buộc, không được để trống.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RunSafeAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();
                await returnService.RejectAsync(_selectedRequest.Id, reason.Trim());

                MessageBox.Show("Đã từ chối yêu cầu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshBadgesAsync();
                await LoadDataAsync();
            });
        }

        private async Task CancelAsync()
        {
            if (_selectedRequest == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn hủy yêu cầu {_selectedRequest.RequestNo}?",
                "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            await RunSafeAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();
                await returnService.CancelAsync(_selectedRequest.Id);

                MessageBox.Show("Đã hủy yêu cầu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshBadgesAsync();
                await LoadDataAsync();
            });
        }

        private async Task CompleteAsync()
        {
            if (_selectedRequest == null) return;

            var confirm = MessageBox.Show(
                $"Xác nhận hoàn tất yêu cầu {_selectedRequest.RequestNo}?\n" +
                $"Hệ thống sẽ ghi sổ StockMovement, hoàn lại tiền ({_selectedRequest.TotalRefund:N0}đ) " +
                "và cập nhật trạng thái hóa đơn gốc.",
                "Xác nhận hoàn tất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            await RunSafeAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var returnService = scope.ServiceProvider.GetRequiredService<IReturnService>();
                await returnService.CompleteAsync(_selectedRequest.Id);

                MessageBox.Show(
                    $"Hoàn tất yêu cầu {_selectedRequest.RequestNo} thành công!\n" +
                    "Kho hàng và công nợ hoàn trả đã được cập nhật.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                await RefreshBadgesAsync();
                await LoadDataAsync();
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

        private static string GetStatusVietnamese(ReturnStatus status) => status switch
        {
            ReturnStatus.Pending => "Chờ duyệt",
            ReturnStatus.Approved => "Đã duyệt",
            ReturnStatus.Received => "Đã nhận hàng",
            ReturnStatus.Completed => "Hoàn tất",
            ReturnStatus.Rejected => "Đã từ chối",
            ReturnStatus.Cancelled => "Đã hủy",
            _ => status.ToString()
        };

        // Form prompt đơn giản lấy lý do từ chối
        private static class PromptDialog
        {
            public static string Show(string text, string caption)
            {
                var prompt = new Form
                {
                    Width = 460,
                    Height = 220,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterParent,
                    Font = InvoiceTheme.BodyFont
                };

                var textLabel = new Label { Left = 20, Top = 15, Text = text, AutoSize = true, Font = InvoiceTheme.BodyBoldFont };
                var textBox = new TextBox { Left = 20, Top = 45, Width = 400, Height = 60, Multiline = true };
                var confirmation = new Button
                {
                    Text = "Xác nhận",
                    Left = 210,
                    Width = 100,
                    Height = 40,
                    Top = 120,
                    DialogResult = DialogResult.OK,
                    BackColor = InvoiceTheme.Primary,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                var cancel = new Button
                {
                    Text = "Hủy",
                    Left = 320,
                    Width = 100,
                    Height = 40,
                    Top = 120,
                    DialogResult = DialogResult.Cancel,
                    BackColor = InvoiceTheme.Secondary,
                    ForeColor = InvoiceTheme.SecondaryText,
                    FlatStyle = FlatStyle.Flat
                };

                prompt.Controls.AddRange([textLabel, textBox, confirmation, cancel]);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
    }
}
