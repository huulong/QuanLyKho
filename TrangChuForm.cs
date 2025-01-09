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

        public TrangChuForm(string? tenNhanVien)
        {
            _tenNhanVien = tenNhanVien ?? "Nhân Viên";
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
            SqlConnection connection = null;

            try
            {
                // Tạo kết nối
                connection = DatabaseConnection.GetConnection();
                
                // Mở kết nối một cách rõ ràng
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                // Log thông tin chi tiết về kết nối trước khi thực hiện truy vấn
                System.Diagnostics.Debug.WriteLine($"Connection State: {connection.State}");
                System.Diagnostics.Debug.WriteLine($"Connection String: {connection.ConnectionString}");
                System.Diagnostics.Debug.WriteLine($"Database: {connection.Database}");
                System.Diagnostics.Debug.WriteLine($"Data Source: {connection.DataSource}");

                var queries = new[]
                {
                    "SELECT COUNT(*) FROM [dbo].[VatTu]",
                    "SELECT COUNT(*) FROM [dbo].[Phieu_Nhap] WHERE MONTH(NgayNhapHang) = MONTH(GETDATE()) AND YEAR(NgayNhapHang) = YEAR(GETDATE())",
                    "SELECT COUNT(*) FROM [dbo].[Phieu_Xuat] WHERE MONTH(NgayXuatHang) = MONTH(GETDATE()) AND YEAR(NgayXuatHang) = YEAR(GETDATE())"
                };

                var labels = new[] 
                { 
                    "Tổng Vật Tư", 
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
                            // Đặt timeout cho từng truy vấn
                            cmd.CommandTimeout = 30; // 30 giây

                            // Log chi tiết truy vấn
                            System.Diagnostics.Debug.WriteLine($"Executing query: {queries[i]}");
                            System.Diagnostics.Debug.WriteLine($"Connection State before ExecuteScalar: {connection.State}");
                            
                            var value = Convert.ToInt32(cmd.ExecuteScalar());
                            
                            System.Diagnostics.Debug.WriteLine($"Query result for {labels[i]}: {value}");
                            System.Diagnostics.Debug.WriteLine($"Connection State after ExecuteScalar: {connection.State}");
                            
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thống kê: {ex.Message}");
                stats = new List<(string, int, Color)>
                {
                    ("Tổng Vật Tư", 0, Color.FromArgb(33, 150, 243)),
                    ("Phiếu Nhập Tháng", 0, Color.FromArgb(76, 175, 80)),
                    ("Phiếu Xuất Tháng", 0, Color.FromArgb(255, 152, 0))
                };
            }
            finally
            {
                // Đảm bảo kết nối luôn được đóng
                if (connection != null)
                {
                    try 
                    {
                        if (connection.State != System.Data.ConnectionState.Closed)
                        {
                            connection.Close();
                        }
                        connection.Dispose();
                    }
                    catch (Exception closeEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi đóng kết nối: {closeEx.Message}");
                    }
                }
            }

            return stats;
        }

        // Phương thức hỗ trợ: Ghi log thông tin kết nối
        private static void LogConnectionDetails(SqlConnection connection)
        {
            System.Diagnostics.Debug.WriteLine($"Connection String: {connection.ConnectionString}");
            System.Diagnostics.Debug.WriteLine($"Database: {connection.Database}");
            System.Diagnostics.Debug.WriteLine($"Data Source: {connection.DataSource}");
        }

        // Phương thức hỗ trợ: Ghi log chi tiết truy vấn
        private static void LogQueryDetails(string query, string label)
        {
            System.Diagnostics.Debug.WriteLine($"Executing query for {label}: {query}");
        }

        // Phương thức hỗ trợ: Thực thi truy vấn với ghi log
        private static int ExecuteScalarWithLogging(SqlCommand cmd, string label)
        {
            int value = Convert.ToInt32(cmd.ExecuteScalar());
            System.Diagnostics.Debug.WriteLine($"Query result for {label}: {value}");
            return value;
        }

        // Phương thức hỗ trợ: Ghi log lỗi SQL
        private static void LogSqlError(string label, SqlException sqlEx)
        {
            System.Diagnostics.Debug.WriteLine($"SQL Error in query {label}: {sqlEx.Message}");
        }

        // Phương thức hỗ trợ: Ghi log lỗi chung
        private static void LogGeneralError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi lấy thống kê: {ex.Message}");
        }

        // Phương thức hỗ trợ: Tạo danh sách thống kê mặc định
        private static List<(string, int, Color)> CreateDefaultStats()
        {
            return new List<(string, int, Color)>
            {
                ("Tổng Vật Tư", 0, Color.FromArgb(33, 150, 243)),
                ("Phiếu Nhập Tháng", 0, Color.FromArgb(76, 175, 80)),
                ("Phiếu Xuất Tháng", 0, Color.FromArgb(255, 152, 0))
            };
        }

        private Panel CreateHeaderPanel()
        {
            Panel panel = new Panel
            {
                BackColor = Color.FromArgb(33, 150, 243),
                Height = 80,
                Dock = DockStyle.Fill
            };

            // Logo (placeholder)
            PictureBox logoPictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(100, 60),
                Location = new Point(10, 10),
                BackColor = Color.Transparent,
                Image = CreatePlaceholderLogo()
            };

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

            // Nút đăng xuất
            Button btnLogout = new Button
            {
                Text = "Đăng Xuất",
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(120, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panel.Width - 130, 20)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnDangXuat_Click;

            // Timer để cập nhật thởi gian liên tục
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // Cứ 1 giây cập nhật một lần
            };
            timer.Tick += (sender, e) => 
            {
                lblDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            timer.Start();

            panel.Controls.AddRange(new Control[] { 
                logoPictureBox, 
                lblUserName, 
                lblDateTime, 
                btnLogout 
            });

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

        private void BtnQuanLyNhaCungCap_Click(object? sender, EventArgs e)
        {
            // Tạo form quản lý nhà cung cấp
            Form quanLyNhaCungCapForm = new Form();
            quanLyNhaCungCapForm.Text = "Quản Lý Nhà Cung Cấp";
            quanLyNhaCungCapForm.Size = new Size(800, 600);
            quanLyNhaCungCapForm.StartPosition = FormStartPosition.CenterScreen;
            
            // TODO: Thêm DataGridView để hiển thị danh sách nhà cung cấp
            MessageBox.Show("Chức năng đang phát triển", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnQuanLyPhieuNhap_Click(object? sender, EventArgs e)
        {
            // Tạo form quản lý phiếu nhập
            Form quanLyPhieuNhapForm = new Form();
            quanLyPhieuNhapForm.Text = "Quản Lý Phiếu Nhập";
            quanLyPhieuNhapForm.Size = new Size(1000, 600);
            quanLyPhieuNhapForm.StartPosition = FormStartPosition.CenterScreen;
            
            // TODO: Thêm DataGridView để hiển thị danh sách phiếu nhập
            MessageBox.Show("Chức năng đang phát triển", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnQuanLyPhieuXuat_Click(object? sender, EventArgs e)
        {
            // Tạo form quản lý phiếu xuất
            Form quanLyPhieuXuatForm = new Form();
            quanLyPhieuXuatForm.Text = "Quản Lý Phiếu Xuất";
            quanLyPhieuXuatForm.Size = new Size(1000, 600);
            quanLyPhieuXuatForm.StartPosition = FormStartPosition.CenterScreen;
            
            // TODO: Thêm DataGridView để hiển thị danh sách phiếu xuất
            MessageBox.Show("Chức năng đang phát triển", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
