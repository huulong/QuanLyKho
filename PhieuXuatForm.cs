using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace QuanLyKho
{
    public partial class PhieuXuatForm : Form
    {
        private DataGridView dgvPhieuXuat;
        private DataGridView dgvChiTietPhieuXuat;
        private TextBox txtMaPhieu;
        private DateTimePicker dtpNgayXuat;
        private TextBox txtGhiChu;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLuu;
        private Label lblTongTien;
        private int maNV;

        public PhieuXuatForm(int maNV)
        {
            this.maNV = maNV;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Phiếu Xuất";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Phiếu xuất panel
            var phieuXuatPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(300, 740),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(phieuXuatPanel);

            var y = 20;
            var labelWidth = 100;
            var controlWidth = 180;
            var spacing = 40;

            // Mã phiếu
            var lblMaPhieu = new Label { Text = "Mã phiếu:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtMaPhieu = new TextBox { Location = new Point(110, y), Size = new Size(controlWidth, 25), Enabled = false };
            phieuXuatPanel.Controls.AddRange(new Control[] { lblMaPhieu, txtMaPhieu });

            // Ngày xuất
            y += spacing;
            var lblNgayXuat = new Label { Text = "Ngày xuất:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            dtpNgayXuat = new DateTimePicker { Location = new Point(110, y), Size = new Size(controlWidth, 25) };
            phieuXuatPanel.Controls.AddRange(new Control[] { lblNgayXuat, dtpNgayXuat });

            // Ghi chú
            y += spacing;
            var lblGhiChu = new Label { Text = "Ghi chú:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtGhiChu = new TextBox { Location = new Point(110, y), Size = new Size(controlWidth, 50), Multiline = true };
            phieuXuatPanel.Controls.AddRange(new Control[] { lblGhiChu, txtGhiChu });

            // Buttons
            y += spacing + 40;
            var buttonWidth = 80;
            btnThem = new Button { Text = "Thêm", Location = new Point(10, y), Size = new Size(buttonWidth, 30) };
            btnSua = new Button { Text = "Sửa", Location = new Point(100, y), Size = new Size(buttonWidth, 30) };
            btnXoa = new Button { Text = "Xóa", Location = new Point(190, y), Size = new Size(buttonWidth, 30) };
            
            y += 40;
            btnLuu = new Button { Text = "Lưu", Location = new Point(100, y), Size = new Size(buttonWidth, 30), Enabled = false };
            
            phieuXuatPanel.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLuu });

            // DataGridViews
            dgvPhieuXuat = new DataGridView
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

            dgvChiTietPhieuXuat = new DataGridView
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

            this.Controls.AddRange(new Control[] { dgvPhieuXuat, dgvChiTietPhieuXuat });

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
            dgvPhieuXuat.SelectionChanged += DgvPhieuXuat_SelectionChanged;
        }

        private void LoadData()
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        SELECT px.MaPhieu, px.NgayXuat, px.GhiChu, px.TongTien
                        FROM [dbo].[Phieu_Xuat] px
                        ORDER BY px.NgayXuat DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvPhieuXuat.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChiTietPhieuXuat(string maPhieu)
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        SELECT ct.MaHang, hh.TenHang, ct.SoLuong, ct.DonGia, ct.ThanhTien
                        FROM [dbo].[Chi_Tiet_Phieu_Xuat] ct
                        JOIN [dbo].[Hang_Hoa] hh ON ct.MaHang = hh.MaHang
                        WHERE ct.MaPhieu = @MaPhieu";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@MaPhieu", maPhieu);
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvChiTietPhieuXuat.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải chi tiết phiếu xuất: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvPhieuXuat_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPhieuXuat.CurrentRow != null)
            {
                var row = dgvPhieuXuat.CurrentRow;
                txtMaPhieu.Text = row.Cells["MaPhieu"].Value.ToString();
                dtpNgayXuat.Value = Convert.ToDateTime(row.Cells["NgayXuat"].Value);
                txtGhiChu.Text = row.Cells["GhiChu"].Value?.ToString() ?? "";
                lblTongTien.Text = $"Tổng tiền: {row.Cells["TongTien"].Value:N0} VNĐ";

                LoadChiTietPhieuXuat(txtMaPhieu.Text);
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
            if (dgvPhieuXuat.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu xuất cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetInputMode(true);
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvPhieuXuat.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu xuất cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var maPhieu = dgvPhieuXuat.CurrentRow.Cells["MaPhieu"].Value.ToString();
            if (MessageBox.Show($"Bạn có chắc muốn xóa phiếu xuất này?", "Xác nhận", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                    {
                        // Xóa chi tiết phiếu xuất trước
                        string deleteDetailQuery = "DELETE FROM [dbo].[Chi_Tiet_Phieu_Xuat] WHERE MaPhieu = @MaPhieu";
                        using (var cmd = new SqlCommand(deleteDetailQuery, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmd.ExecuteNonQuery();
                        }

                        // Sau đó xóa phiếu xuất
                        string deleteQuery = "DELETE FROM [dbo].[Phieu_Xuat] WHERE MaPhieu = @MaPhieu";
                        using (var cmd = new SqlCommand(deleteQuery, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmd.ExecuteNonQuery();
                        }
                    });

                    LoadData();
                    ClearInputs();
                    MessageBox.Show("Đã xóa phiếu xuất thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phiếu xuất: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        IF EXISTS (SELECT 1 FROM [dbo].[Phieu_Xuat] WHERE MaPhieu = @MaPhieu)
                            UPDATE [dbo].[Phieu_Xuat]
                            SET NgayXuat = @NgayXuat,
                                GhiChu = @GhiChu,
                                MaNV = @MaNV
                            WHERE MaPhieu = @MaPhieu
                        ELSE
                            INSERT INTO [dbo].[Phieu_Xuat] (MaPhieu, NgayXuat, GhiChu, MaNV, TongTien)
                            VALUES (@MaPhieu, @NgayXuat, @GhiChu, @MaNV, 0)";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaPhieu", txtMaPhieu.Text);
                        cmd.Parameters.AddWithValue("@NgayXuat", dtpNgayXuat.Value);
                        cmd.Parameters.AddWithValue("@GhiChu", string.IsNullOrEmpty(txtGhiChu.Text) ? DBNull.Value : (object)txtGhiChu.Text);
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.ExecuteNonQuery();
                    }
                });

                LoadData();
                SetInputMode(false);
                MessageBox.Show("Đã lưu phiếu xuất thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu phiếu xuất: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        FROM [dbo].[Phieu_Xuat]
                        WHERE MaPhieu LIKE 'PX%'
                        ORDER BY MaPhieu DESC";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Transaction = transaction;
                        var lastMaPhieu = cmd.ExecuteScalar()?.ToString();
                        
                        if (string.IsNullOrEmpty(lastMaPhieu))
                        {
                            newMaPhieu = "PX001";
                        }
                        else
                        {
                            int number = int.Parse(lastMaPhieu.Substring(2)) + 1;
                            newMaPhieu = $"PX{number:D3}";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo mã phiếu mới: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                newMaPhieu = "PX001";
            }
            return newMaPhieu;
        }

        private void ClearInputs()
        {
            txtMaPhieu.Text = "";
            dtpNgayXuat.Value = DateTime.Now;
            txtGhiChu.Text = "";
            lblTongTien.Text = "Tổng tiền: 0 VNĐ";
            dgvChiTietPhieuXuat.DataSource = null;
        }

        private void SetInputMode(bool isEditing)
        {
            dtpNgayXuat.Enabled = isEditing;
            txtGhiChu.Enabled = isEditing;

            btnThem.Enabled = !isEditing;
            btnSua.Enabled = !isEditing;
            btnXoa.Enabled = !isEditing;
            btnLuu.Enabled = isEditing;
        }
    }
}
