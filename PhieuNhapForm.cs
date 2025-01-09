using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace QuanLyKho
{
    public partial class PhieuNhapForm : Form
    {
        private DataGridView dgvPhieuNhap;
        private DataGridView dgvChiTietPhieuNhap;
        private ComboBox cboNhaCungCap;
        private TextBox txtMaPhieu;
        private DateTimePicker dtpNgayNhap;
        private TextBox txtGhiChu;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLuu;
        private Label lblTongTien;
        private int maNV;

        public PhieuNhapForm(int maNV)
        {
            this.maNV = maNV;
            InitializeComponent();
            LoadData();
            LoadNhaCungCap();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Phiếu Nhập";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Phiếu nhập panel
            var phieuNhapPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(300, 740),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(phieuNhapPanel);

            var y = 20;
            var labelWidth = 100;
            var controlWidth = 180;
            var spacing = 40;

            // Mã phiếu
            var lblMaPhieu = new Label { Text = "Mã phiếu:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtMaPhieu = new TextBox { Location = new Point(110, y), Size = new Size(controlWidth, 25), Enabled = false };
            phieuNhapPanel.Controls.AddRange(new Control[] { lblMaPhieu, txtMaPhieu });

            // Ngày nhập
            y += spacing;
            var lblNgayNhap = new Label { Text = "Ngày nhập:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            dtpNgayNhap = new DateTimePicker { Location = new Point(110, y), Size = new Size(controlWidth, 25) };
            phieuNhapPanel.Controls.AddRange(new Control[] { lblNgayNhap, dtpNgayNhap });

            // Nhà cung cấp
            y += spacing;
            var lblNhaCungCap = new Label { Text = "Nhà cung cấp:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            cboNhaCungCap = new ComboBox { Location = new Point(110, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            phieuNhapPanel.Controls.AddRange(new Control[] { lblNhaCungCap, cboNhaCungCap });

            // Ghi chú
            y += spacing;
            var lblGhiChu = new Label { Text = "Ghi chú:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtGhiChu = new TextBox { Location = new Point(110, y), Size = new Size(controlWidth, 50), Multiline = true };
            phieuNhapPanel.Controls.AddRange(new Control[] { lblGhiChu, txtGhiChu });

            // Buttons
            y += spacing + 40;
            var buttonWidth = 80;
            btnThem = new Button { Text = "Thêm", Location = new Point(10, y), Size = new Size(buttonWidth, 30) };
            btnSua = new Button { Text = "Sửa", Location = new Point(100, y), Size = new Size(buttonWidth, 30) };
            btnXoa = new Button { Text = "Xóa", Location = new Point(190, y), Size = new Size(buttonWidth, 30) };
            
            y += 40;
            btnLuu = new Button { Text = "Lưu", Location = new Point(100, y), Size = new Size(buttonWidth, 30), Enabled = false };
            
            phieuNhapPanel.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLuu });

            // DataGridViews
            dgvPhieuNhap = new DataGridView
            {
                Location = new Point(320, 10),
                Size = new Size(850, 350),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvChiTietPhieuNhap = new DataGridView
            {
                Location = new Point(320, 400),
                Size = new Size(850, 350),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            this.Controls.AddRange(new Control[] { dgvPhieuNhap, dgvChiTietPhieuNhap });

            // Labels
            lblTongTien = new Label
            {
                Text = "Tổng tiền: 0 VNĐ",
                Location = new Point(320, 760),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            this.Controls.Add(lblTongTien);

            // Events
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLuu.Click += BtnLuu_Click;
            dgvPhieuNhap.SelectionChanged += DgvPhieuNhap_SelectionChanged;
        }

        private void LoadData()
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        SELECT pn.MaPhieu, pn.NgayNhap, ncc.TenNCC, pn.GhiChu, pn.TongTien
                        FROM [dbo].[Phieu_Nhap] pn
                        LEFT JOIN [dbo].[Nha_Cung_Cap] ncc ON pn.MaNCC = ncc.MaNCC
                        ORDER BY pn.NgayNhap DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvPhieuNhap.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadNhaCungCap()
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = "SELECT MaNCC, TenNCC FROM [dbo].[Nha_Cung_Cap] ORDER BY TenNCC";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        
                        cboNhaCungCap.DisplayMember = "TenNCC";
                        cboNhaCungCap.ValueMember = "MaNCC";
                        cboNhaCungCap.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhà cung cấp: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChiTietPhieuNhap(string maPhieu)
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        SELECT ct.MaHang, hh.TenHang, ct.SoLuong, ct.DonGia, ct.ThanhTien
                        FROM [dbo].[Chi_Tiet_Phieu_Nhap] ct
                        JOIN [dbo].[Hang_Hoa] hh ON ct.MaHang = hh.MaHang
                        WHERE ct.MaPhieu = @MaPhieu";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@MaPhieu", maPhieu);
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvChiTietPhieuNhap.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải chi tiết phiếu nhập: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvPhieuNhap_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPhieuNhap.CurrentRow != null)
            {
                var row = dgvPhieuNhap.CurrentRow;
                txtMaPhieu.Text = row.Cells["MaPhieu"].Value.ToString();
                dtpNgayNhap.Value = Convert.ToDateTime(row.Cells["NgayNhap"].Value);
                cboNhaCungCap.Text = row.Cells["TenNCC"].Value?.ToString() ?? "";
                txtGhiChu.Text = row.Cells["GhiChu"].Value?.ToString() ?? "";
                lblTongTien.Text = $"Tổng tiền: {row.Cells["TongTien"].Value:N0} VNĐ";

                LoadChiTietPhieuNhap(txtMaPhieu.Text);
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            ClearInputs();
            SetInputMode(true);
            // Tạo mã phiếu mới
            txtMaPhieu.Text = GenerateNewMaPhieu();
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvPhieuNhap.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu nhập cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetInputMode(true);
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvPhieuNhap.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu nhập cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var maPhieu = dgvPhieuNhap.CurrentRow.Cells["MaPhieu"].Value.ToString();
            if (MessageBox.Show($"Bạn có chắc muốn xóa phiếu nhập này?", "Xác nhận", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                    {
                        // Xóa chi tiết phiếu nhập trước
                        string deleteDetailQuery = "DELETE FROM [dbo].[Chi_Tiet_Phieu_Nhap] WHERE MaPhieu = @MaPhieu";
                        using (var cmd = new SqlCommand(deleteDetailQuery, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmd.ExecuteNonQuery();
                        }

                        // Sau đó xóa phiếu nhập
                        string deleteQuery = "DELETE FROM [dbo].[Phieu_Nhap] WHERE MaPhieu = @MaPhieu";
                        using (var cmd = new SqlCommand(deleteQuery, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmd.ExecuteNonQuery();
                        }
                    });

                    LoadData();
                    ClearInputs();
                    MessageBox.Show("Đã xóa phiếu nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu nhập: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (cboNhaCungCap.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        IF EXISTS (SELECT 1 FROM [dbo].[Phieu_Nhap] WHERE MaPhieu = @MaPhieu)
                            UPDATE [dbo].[Phieu_Nhap]
                            SET NgayNhap = @NgayNhap,
                                MaNCC = @MaNCC,
                                GhiChu = @GhiChu,
                                MaNV = @MaNV
                            WHERE MaPhieu = @MaPhieu
                        ELSE
                            INSERT INTO [dbo].[Phieu_Nhap] (MaPhieu, NgayNhap, MaNCC, GhiChu, MaNV, TongTien)
                            VALUES (@MaPhieu, @NgayNhap, @MaNCC, @GhiChu, @MaNV, 0)";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaPhieu", txtMaPhieu.Text);
                        cmd.Parameters.AddWithValue("@NgayNhap", dtpNgayNhap.Value);
                        cmd.Parameters.AddWithValue("@MaNCC", cboNhaCungCap.SelectedValue);
                        cmd.Parameters.AddWithValue("@GhiChu", string.IsNullOrEmpty(txtGhiChu.Text) ? DBNull.Value : (object)txtGhiChu.Text);
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.ExecuteNonQuery();
                    }
                });

                LoadData();
                SetInputMode(false);
                MessageBox.Show("Đã lưu phiếu nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu nhập: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateNewMaPhieu()
        {
            string newMaPhieu = "";
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        SELECT TOP 1 MaPhieu 
                        FROM [dbo].[Phieu_Nhap]
                        WHERE MaPhieu LIKE 'PN%'
                        ORDER BY MaPhieu DESC";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Transaction = transaction;
                        var lastMaPhieu = cmd.ExecuteScalar()?.ToString();
                        
                        if (string.IsNullOrEmpty(lastMaPhieu))
                        {
                            newMaPhieu = "PN001";
                        }
                        else
                        {
                            int number = int.Parse(lastMaPhieu.Substring(2)) + 1;
                            newMaPhieu = $"PN{number:D3}";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo mã phiếu mới: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newMaPhieu = "PN001";
            }
            return newMaPhieu;
        }

        private void ClearInputs()
        {
            txtMaPhieu.Text = "";
            dtpNgayNhap.Value = DateTime.Now;
            cboNhaCungCap.SelectedIndex = -1;
            txtGhiChu.Text = "";
            lblTongTien.Text = "Tổng tiền: 0 VNĐ";
            dgvChiTietPhieuNhap.DataSource = null;
        }

        private void SetInputMode(bool isEditing)
        {
            dtpNgayNhap.Enabled = isEditing;
            cboNhaCungCap.Enabled = isEditing;
            txtGhiChu.Enabled = isEditing;

            btnThem.Enabled = !isEditing;
            btnSua.Enabled = !isEditing;
            btnXoa.Enabled = !isEditing;
            btnLuu.Enabled = isEditing;
        }
    }
}
