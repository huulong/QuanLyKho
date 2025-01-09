using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace QuanLyKho
{
    public partial class TrangChuForm : Form
    {
        private readonly string _tenNhanVien;
        private readonly int _maNV;

        // Lớp hỗ trợ để quản lý thông tin menu
        private class MenuItemInfo
        {
            public string Text { get; }
            public string Icon { get; }
            public Color Color { get; }
            public EventHandler Action { get; }

            public MenuItemInfo(string text, string icon, Color color, EventHandler action)
            {
                Text = text;
                Icon = icon;
                Color = color;
                Action = action;
            }
        }

        public TrangChuForm(string? tenNhanVien, int maNV)
        {
            _tenNhanVien = tenNhanVien ?? "Nhân Viên";
            _maNV = maNV;
            InitializeComponent();
            ConfigureForm();
        }

        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            
            SuspendLayout();
            
            // Cấu hình cơ bản của Form
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1300, 800);
            Name = "TrangChuForm";
            Text = $"Quản Lý Kho - {_tenNhanVien}";
            
            // Thêm nút Quản lý người dùng
            Button btnQuanLyNguoiDung = new Button
            {
                Text = "Quản Lý\nNgười Dùng",
                Size = new Size(100, 50),
                Location = new Point(20, 420), 
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnQuanLyNguoiDung.Click += BtnQuanLyNguoiDung_Click;
            this.Controls.Add(btnQuanLyNguoiDung);

            ResumeLayout(false);
        }

        private void ConfigureForm()
        {
            Size = new Size(1300, 800);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(250, 250, 255);
            MinimumSize = new Size(1100, 700);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                BackColor = Color.FromArgb(240, 242, 245)
            };
            
            // Cấu hình cột
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));  // Menu trái
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));  // Nội dung chính
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));  // Thống kê
            
            // Cấu hình dòng
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));       // Header
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));        // Nội dung chính
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));        // Footer

            // Header
            var headerPanel = CreateHeaderPanel();
            mainPanel.Controls.Add(headerPanel, 0, 0);
            mainPanel.SetColumnSpan(headerPanel, 3);

            // Menu trái
            var menuPanel = CreateMenuPanel();
            mainPanel.Controls.Add(menuPanel, 0, 1);
            mainPanel.SetRowSpan(menuPanel, 2);

            // Nội dung chính
            var contentPanel = CreateContentPanel();
            mainPanel.Controls.Add(contentPanel, 1, 1);
            mainPanel.SetRowSpan(contentPanel, 2);

            // Thống kê
            var statisticPanel = CreateStatisticPanel();
            mainPanel.Controls.Add(statisticPanel, 2, 1);
            mainPanel.SetRowSpan(statisticPanel, 2);

            // Footer
            var footerPanel = CreateFooterPanel();
            mainPanel.Controls.Add(footerPanel, 0, 2);
            mainPanel.SetColumnSpan(footerPanel, 3);

            Controls.Add(mainPanel);
        }

        private Bitmap CreatePlaceholderLogo()
        {
            var logo = new Bitmap(100, 60);
            using (var g = Graphics.FromImage(logo))
            {
                g.Clear(Color.White);
                using var font = new Font("Segoe UI", 10, FontStyle.Bold);
                using var brush = new SolidBrush(Color.FromArgb(33, 150, 243));
                
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("UTT", font, brush, new RectangleF(0, 0, logo.Width, logo.Height), sf);
            }
            return logo;
        }

        private static List<(string label, int value, Color color)> GetWarehouseStatistics()
        {
            var stats = new List<(string, int, Color)>();
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    var queries = new[]
                    {
                        "SELECT COUNT(*) FROM [dbo].[Hang_Hoa]",
                        "SELECT COUNT(*) FROM [dbo].[Phieu_Nhap] WHERE MONTH(NgayNhap) = MONTH(GETDATE()) AND YEAR(NgayNhap) = YEAR(GETDATE())",
                        "SELECT COUNT(*) FROM [dbo].[Phieu_Xuat] WHERE MONTH(NgayXuat) = MONTH(GETDATE()) AND YEAR(NgayXuat) = YEAR(GETDATE())"
                    };

                    var labels = new[] 
                    { 
                        "Tổng Hàng Hóa", 
                        "Phiếu Nhập Tháng", 
                        "Phiếu Xuất Tháng" 
                    };

                    var colors = new[]
                    {
                        Color.FromArgb(33, 150, 243),
                        Color.FromArgb(76, 175, 80),
                        Color.FromArgb(255, 152, 0)
                    };

                    for (int i = 0; i < queries.Length; i++)
                    {
                        try
                        {
                            using (var cmd = new SqlCommand(queries[i], connection))
                            {
                                var value = Convert.ToInt32(cmd.ExecuteScalar());
                                stats.Add((labels[i], value, colors[i]));
                            }
                        }
                        catch (SqlException sqlEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"SQL Error in query {labels[i]}: {sqlEx.Message}");
                            stats.Add((labels[i], 0, colors[i]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thống kê: {ex.Message}");
                stats = new List<(string, int, Color)>
                {
                    ("Tổng Hàng Hóa", 0, Color.FromArgb(33, 150, 243)),
                    ("Phiếu Nhập Tháng", 0, Color.FromArgb(76, 175, 80)),
                    ("Phiếu Xuất Tháng", 0, Color.FromArgb(255, 152, 0))
                };
            }
            return stats;
        }

        private Panel CreateHeaderPanel()
        {
            Panel panel = new Panel
            {
                BackColor = Color.FromArgb(33, 150, 243),
                Height = 80,
                Dock = DockStyle.Fill
            };

            // Logo
            PictureBox logoPictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(100, 60),
                Location = new Point(10, 10),
                BackColor = Color.Transparent,
                Image = CreatePlaceholderLogo()
            };
            panel.Controls.Add(logoPictureBox);

            // Tên người dùng
            Label lblUserName = new Label
            {
                Text = $"Xin chào, {_tenNhanVien}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(150, 25),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblUserName);

            // Thời gian
            Label lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(500, 30),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblDateTime);

            // Timer để cập nhật thởi gian
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            timer.Tick += (sender, e) =>
            {
                lblDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            timer.Start();

            // Báo cáo buttons
            var btnThongKe = new Button
            {
                Text = "Thống Kê Nhập/Xuất",
                Size = new Size(150, 40),
                Location = new Point(650, 20),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 150, 243),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThongKe.FlatAppearance.BorderSize = 0;
            btnThongKe.Click += BtnThongKeNhapXuat_Click;
            panel.Controls.Add(btnThongKe);

            var btnTonKho = new Button
            {
                Text = "Báo Cáo Tồn Kho",
                Size = new Size(150, 40),
                Location = new Point(810, 20),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 150, 243),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTonKho.FlatAppearance.BorderSize = 0;
            btnTonKho.Click += BtnBaoCaoTonKho_Click;
            panel.Controls.Add(btnTonKho);

            var btnDoanhThu = new Button
            {
                Text = "Báo Cáo Doanh Thu",
                Size = new Size(150, 40),
                Location = new Point(970, 20),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 150, 243),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDoanhThu.FlatAppearance.BorderSize = 0;
            btnDoanhThu.Click += BtnBaoCaoDoanhThu_Click;
            panel.Controls.Add(btnDoanhThu);

            // Nút đăng xuất
            Button btnLogout = new Button
            {
                Text = "Đăng Xuất",
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(1130, 20),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnDangXuat_Click;
            panel.Controls.Add(btnLogout);

            return panel;
        }

        private Panel CreateMenuPanel()
        {
            Panel panel = new Panel
            {
                BackColor = Color.White,
                Width = 250,
                Dock = DockStyle.Fill
            };

            FlowLayoutPanel menuContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10)
            };

            var menuItems = new[]
            {
                new MenuItemInfo("Vật Tư", "", Color.FromArgb(0, 188, 212), BtnQuanLyVatTu_Click),
                new MenuItemInfo("Nhà Cung Cấp", "", Color.FromArgb(76, 175, 80), BtnQuanLyNhaCungCap_Click),
                new MenuItemInfo("Phiếu Nhập", "", Color.FromArgb(33, 150, 243), BtnQuanLyPhieuNhap_Click),
                new MenuItemInfo("Phiếu Xuất", "", Color.FromArgb(255, 152, 0), BtnQuanLyPhieuXuat_Click)
            };

            foreach (var item in menuItems)
            {
                Button btn = new Button
                {
                    Size = new Size(230, 60),
                    BackColor = item.Color,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    Text = $"{item.Icon} {item.Text}",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(5)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += item.Action;

                menuContainer.Controls.Add(btn);
            }

            panel.Controls.Add(menuContainer);
            return panel;
        }

        private Panel CreateContentPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var hangHoaGrid = CreateHangHoaOverviewGrid();
            hangHoaGrid.Dock = DockStyle.Fill;
            panel.Controls.Add(hangHoaGrid);

            return panel;
        }

        private DataGridView CreateHangHoaOverviewGrid()
        {
            var gridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            gridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MaVatTu",
                HeaderText = "Mã Vật Tư",
                Width = 100
            });

            gridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TenVatTu",
                HeaderText = "Tên Vật Tư",
                Width = 300
            });

            return gridView;
        }

        private static Panel CreateStatisticPanel()
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            FlowLayoutPanel statsContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown
            };

            var warehouseStats = GetWarehouseStatistics();

            foreach (var (label, value, color) in warehouseStats)
            {
                Panel statItem = new Panel
                {
                    Size = new Size(250, 100),
                    BackColor = color,
                    Margin = new Padding(10)
                };

                Label lblValue = new Label
                {
                    Text = value.ToString(),
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                Label lblLabel = new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 12, FontStyle.Regular),
                    ForeColor = Color.White,
                    AutoSize = false,
                    TextAlign = ContentAlignment.BottomCenter,
                    Dock = DockStyle.Bottom,
                    Height = 30
                };

                statItem.Controls.Add(lblValue);
                statItem.Controls.Add(lblLabel);

                statsContainer.Controls.Add(statItem);
            }

            panel.Controls.Add(statsContainer);
            return panel;
        }

        private static Panel CreateFooterPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            return panel;
        }

        private void BtnDangXuat_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất?", 
                "Xác Nhận Đăng Xuất", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Close();
                new DangNhapForm().Show();
            }
        }

        private void BtnQuanLyVatTu_Click(object? sender, EventArgs e)
        {
            QuanLyHangHoaForm quanLyHangHoaForm = new QuanLyHangHoaForm();
            quanLyHangHoaForm.Show();
        }

        private void BtnQuanLyNhaCungCap_Click(object sender, EventArgs e)
        {
            var nhaCungCapForm = new NhaCungCapForm();
            nhaCungCapForm.ShowDialog();
        }

        private void BtnQuanLyPhieuNhap_Click(object sender, EventArgs e)
        {
            var phieuNhapForm = new PhieuNhapForm(_maNV);
            phieuNhapForm.ShowDialog();
        }

        private void BtnQuanLyPhieuXuat_Click(object sender, EventArgs e)
        {
            var phieuXuatForm = new PhieuXuatForm(_maNV);
            phieuXuatForm.ShowDialog();
        }

        private void BtnThongKeNhapXuat_Click(object sender, EventArgs e)
        {
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    var query = @"
                        WITH ThongKeNhap AS (
                            SELECT 
                                pn.MaPhieu,
                                SUM(ISNULL(ct.SoLuong, 0)) as TongSoLuong,
                                SUM(ISNULL(pn.TongTien, 0)) as TongGiaTri
                            FROM Phieu_Nhap pn
                            LEFT JOIN Chi_Tiet_Phieu_Nhap ct ON pn.MaPhieu = ct.MaPhieu
                            WHERE pn.NgayNhap BETWEEN @StartDate AND @EndDate
                            GROUP BY pn.MaPhieu
                        ),
                        ThongKeXuat AS (
                            SELECT 
                                px.MaPhieu,
                                SUM(ISNULL(ct.SoLuong, 0)) as TongSoLuong,
                                SUM(ISNULL(px.TongTien, 0)) as TongGiaTri
                            FROM Phieu_Xuat px
                            LEFT JOIN Chi_Tiet_Phieu_Xuat ct ON px.MaPhieu = ct.MaPhieu
                            WHERE px.NgayXuat BETWEEN @StartDate AND @EndDate
                            GROUP BY px.MaPhieu
                        )
                        SELECT 
                            'Nhập' as LoaiPhieu,
                            COUNT(*) as SoLuongPhieu,
                            SUM(TongSoLuong) as TongSoLuong,
                            SUM(TongGiaTri) as TongGiaTri
                        FROM ThongKeNhap
                        UNION ALL
                        SELECT 
                            'Xuất' as LoaiPhieu,
                            COUNT(*) as SoLuongPhieu,
                            SUM(TongSoLuong) as TongSoLuong,
                            SUM(TongGiaTri) as TongGiaTri
                        FROM ThongKeXuat";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);

                        using (var reader = command.ExecuteReader())
                        {
                            string message = $"Thống kê từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}\n\n";
                            while (reader.Read())
                            {
                                string loaiPhieu = reader["LoaiPhieu"].ToString();
                                int soLuongPhieu = Convert.ToInt32(reader["SoLuongPhieu"]);
                                int tongSoLuong = reader["TongSoLuong"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TongSoLuong"]);
                                decimal tongGiaTri = reader["TongGiaTri"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TongGiaTri"]);

                                message += $"Phiếu {loaiPhieu}:\n";
                                message += $"- Số lượng phiếu: {soLuongPhieu:N0}\n";
                                message += $"- Tổng số lượng: {tongSoLuong:N0}\n";
                                message += $"- Tổng giá trị: {tongGiaTri:N0} VNĐ\n\n";
                            }
                            MessageBox.Show(message, "Thống Kê Nhập/Xuất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy thống kê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBaoCaoTonKho_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    var query = @"
                        SELECT 
                            hh.MaHang,
                            hh.TenHang,
                            dvt.TenDVT,
                            hh.SoLuongTon,
                            hh.GiaNhap,
                            hh.GiaXuat,
                            (hh.SoLuongTon * hh.GiaNhap) as GiaTriTon
                        FROM Hang_Hoa hh
                        LEFT JOIN Don_Vi_Tinh dvt ON hh.MaDVT = dvt.MaDVT
                        WHERE hh.SoLuongTon > 0
                        ORDER BY hh.SoLuongTon DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        using (var reader = command.ExecuteReader())
                        {
                            string message = "BÁO CÁO TỒN KHO\n\n";
                            decimal tongGiaTriTon = 0;
                            int stt = 1;

                            while (reader.Read())
                            {
                                string maHang = reader["MaHang"].ToString();
                                string tenHang = reader["TenHang"].ToString();
                                string dvt = reader["TenDVT"].ToString();
                                int soLuongTon = Convert.ToInt32(reader["SoLuongTon"]);
                                decimal giaNhap = Convert.ToDecimal(reader["GiaNhap"]);
                                decimal giaTriTon = Convert.ToDecimal(reader["GiaTriTon"]);

                                message += $"{stt}. {tenHang} ({maHang})\n";
                                message += $"   - Đơn vị tính: {dvt}\n";
                                message += $"   - Số lượng tồn: {soLuongTon:N0}\n";
                                message += $"   - Giá nhập: {giaNhap:N0} VNĐ\n";
                                message += $"   - Giá trị tồn: {giaTriTon:N0} VNĐ\n\n";

                                tongGiaTriTon += giaTriTon;
                                stt++;
                            }

                            message += $"Tổng giá trị tồn kho: {tongGiaTriTon:N0} VNĐ";
                            MessageBox.Show(message, "Báo Cáo Tồn Kho", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy báo cáo tồn kho: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBaoCaoDoanhThu_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    var query = @"
                        WITH DoanhThuThang AS (
                            SELECT 
                                MONTH(NgayXuat) as Thang,
                                YEAR(NgayXuat) as Nam,
                                COUNT(DISTINCT MaPhieu) as SoPhieuXuat,
                                SUM(TongTien) as DoanhThu
                            FROM Phieu_Xuat
                            WHERE YEAR(NgayXuat) = YEAR(GETDATE())
                            GROUP BY MONTH(NgayXuat), YEAR(NgayXuat)
                        )
                        SELECT 
                            Thang,
                            Nam,
                            SoPhieuXuat,
                            DoanhThu,
                            SUM(DoanhThu) OVER (ORDER BY Nam, Thang) as DoanhThuLuyKe
                        FROM DoanhThuThang
                        ORDER BY Nam, Thang";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        using (var reader = command.ExecuteReader())
                        {
                            string message = $"BÁO CÁO DOANH THU NĂM {DateTime.Now.Year}\n\n";
                            decimal tongDoanhThu = 0;

                            while (reader.Read())
                            {
                                int thang = Convert.ToInt32(reader["Thang"]);
                                int soPhieuXuat = Convert.ToInt32(reader["SoPhieuXuat"]);
                                decimal doanhThu = Convert.ToDecimal(reader["DoanhThu"]);
                                decimal doanhThuLuyKe = Convert.ToDecimal(reader["DoanhThuLuyKe"]);

                                message += $"Tháng {thang}:\n";
                                message += $"- Số phiếu xuất: {soPhieuXuat:N0}\n";
                                message += $"- Doanh thu: {doanhThu:N0} VNĐ\n";
                                message += $"- Doanh thu lũy kế: {doanhThuLuyKe:N0} VNĐ\n\n";

                                tongDoanhThu = doanhThuLuyKe;
                            }

                            message += $"Tổng doanh thu năm: {tongDoanhThu:N0} VNĐ";
                            MessageBox.Show(message, "Báo Cáo Doanh Thu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy báo cáo doanh thu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnQuanLyNguoiDung_Click(object sender, EventArgs e)
        {
            try
            {
                bool isAdmin = false;
                using (var connection = DatabaseConnection.CreateNewConnection())
                {
                    // Debug: Kiểm tra chi tiết quyền
                    string debugQuery = @"
                        SELECT pq.MaNV, pq.MaQuyen, qh.TenQuyen, qh.MoTa,
                               LEN(qh.TenQuyen) as Length,
                               CONCAT('|', qh.TenQuyen, '|') as ExactValue
                        FROM [dbo].[Phan_Quyen] pq
                        JOIN [dbo].[Quyen_Han] qh ON pq.MaQuyen = qh.MaQuyen
                        WHERE pq.MaNV = @MaNV";

                    using (var debugCmd = new SqlCommand(debugQuery, connection))
                    {
                        debugCmd.Parameters.AddWithValue("@MaNV", _maNV);
                        using (var reader = debugCmd.ExecuteReader())
                        {
                            string debugInfo = "";
                            while (reader.Read())
                            {
                                debugInfo += $"\nMaNV: {reader["MaNV"]}, " +
                                           $"MaQuyen: {reader["MaQuyen"]}, " +
                                           $"TenQuyen: {reader["ExactValue"]}, " +
                                           $"Length: {reader["Length"]}, " +
                                           $"MoTa: {reader["MoTa"]}";
                            }
                            MessageBox.Show($"Debug Details:{debugInfo}", "Debug Details");
                        }
                    }

                    // Kiểm tra quyền ADMIN
                    string query = @"
                        SELECT COUNT(1)
                        FROM [dbo].[Phan_Quyen] pq
                        JOIN [dbo].[Quyen_Han] qh ON pq.MaQuyen = qh.MaQuyen
                        WHERE pq.MaNV = @MaNV 
                        AND TRIM(qh.TenQuyen) = 'ADMIN'";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaNV", _maNV);
                        int count = (int)command.ExecuteScalar();
                        isAdmin = count > 0;
                        
                        // Debug thông tin query
                        MessageBox.Show($"Query Debug:\n" +
                                     $"MaNV: {_maNV} (Type: {_maNV.GetType().Name})\n" +
                                     $"Count: {count}\n" +
                                     $"IsAdmin: {isAdmin}", 
                                     "Query Debug");
                    }
                }

                if (!isAdmin)
                {
                    MessageBox.Show("Bạn không có quyền truy cập chức năng này!", 
                        "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var quanLyNguoiDungForm = new QuanLyNguoiDungForm();
                quanLyNguoiDungForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}\nStack: {ex.StackTrace}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
