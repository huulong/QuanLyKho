using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace QuanLyKho
{
    public partial class ChiTietPhieuNhapForm : Form
    {
        private string maPhieu;
        private DataGridView dgvChiTietPhieuNhap;
        private ComboBox cboHangHoa;
        private NumericUpDown numSoLuong;
        private NumericUpDown numDonGia;
        private Label lblThanhTien;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLuu;
        private Label lblTongTien;

        public ChiTietPhieuNhapForm(string maPhieu)
        {
            this.maPhieu = maPhieu;
            InitializeComponent();
            LoadData();
            LoadHangHoa();
        }

        private void InitializeComponent()
        {
            this.Text = $"Chi Tiết Phiếu Nhập - {maPhieu}";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Input panel
            var inputPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(300, 540),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(inputPanel);

            var y = 20;
            var labelWidth = 100;
            var controlWidth = 180;
            var spacing = 40;

            // Hàng hóa
            var lblHangHoa = new Label { Text = "Hàng hóa:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            cboHangHoa = new ComboBox { Location = new Point(110, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.AddRange(new Control[] { lblHangHoa, cboHangHoa });

            // Số lượng
            y += spacing;
            var lblSoLuong = new Label { Text = "Số lượng:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            numSoLuong = new NumericUpDown { Location = new Point(110, y), Size = new Size(controlWidth, 25), Minimum = 1, Maximum = 10000 };
            numSoLuong.ValueChanged += UpdateThanhTien;
            inputPanel.Controls.AddRange(new Control[] { lblSoLuong, numSoLuong });

            // Đơn giá
            y += spacing;
            var lblDonGia = new Label { Text = "Đơn giá:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            numDonGia = new NumericUpDown { Location = new Point(110, y), Size = new Size(controlWidth, 25), Maximum = 1000000000, Increment = 1000 };
            numDonGia.ValueChanged += UpdateThanhTien;
            inputPanel.Controls.AddRange(new Control[] { lblDonGia, numDonGia });

            // Thành tiền
            y += spacing;
            lblThanhTien = new Label { Text = "Thành tiền: 0 VNĐ", Location = new Point(10, y), Size = new Size(280, 20), TextAlign = ContentAlignment.MiddleRight };
            inputPanel.Controls.Add(lblThanhTien);

            // Buttons
            y += spacing + 20;
            var buttonWidth = 80;
            btnThem = new Button { Text = "Thêm", Location = new Point(10, y), Size = new Size(buttonWidth, 30) };
            btnSua = new Button { Text = "Sửa", Location = new Point(100, y), Size = new Size(buttonWidth, 30) };
            btnXoa = new Button { Text = "Xóa", Location = new Point(190, y), Size = new Size(buttonWidth, 30) };
            
            y += 40;
            btnLuu = new Button { Text = "Lưu", Location = new Point(100, y), Size = new Size(buttonWidth, 30), Enabled = false };
            
            inputPanel.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLuu });

            // DataGridView
            dgvChiTietPhieuNhap = new DataGridView
            {
                Location = new Point(320, 10),
                Size = new Size(650, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(dgvChiTietPhieuNhap);

            // Tổng tiền
            lblTongTien = new Label
            {
                Text = "Tổng tiền: 0 VNĐ",
                Location = new Point(320, 520),
                Size = new Size(650, 30),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
            };
            this.Controls.Add(lblTongTien);

            // Events
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLuu.Click += BtnLuu_Click;
            dgvChiTietPhieuNhap.SelectionChanged += DgvChiTietPhieuNhap_SelectionChanged;
        }

        private void LoadData()
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

                    // Load tổng tiền
                    query = "SELECT TongTien FROM [dbo].[Phieu_Nhap] WHERE MaPhieu = @MaPhieu";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@MaPhieu", maPhieu);
                        var tongTien = command.ExecuteScalar();
                        lblTongTien.Text = $"Tổng tiền: {Convert.ToDecimal(tongTien):N0} VNĐ";
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHangHoa()
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = "SELECT MaHang, TenHang FROM [dbo].[Hang_Hoa] ORDER BY TenHang";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        
                        cboHangHoa.DisplayMember = "TenHang";
                        cboHangHoa.ValueMember = "MaHang";
                        cboHangHoa.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách hàng hóa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateThanhTien(object sender, EventArgs e)
        {
            decimal thanhTien = numSoLuong.Value * numDonGia.Value;
            lblThanhTien.Text = $"Thành tiền: {thanhTien:N0} VNĐ";
        }

        private void DgvChiTietPhieuNhap_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvChiTietPhieuNhap.CurrentRow != null)
            {
                var row = dgvChiTietPhieuNhap.CurrentRow;
                cboHangHoa.Text = row.Cells["TenHang"].Value.ToString();
                numSoLuong.Value = Convert.ToDecimal(row.Cells["SoLuong"].Value);
                numDonGia.Value = Convert.ToDecimal(row.Cells["DonGia"].Value);
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            ClearInputs();
            SetInputMode(true);
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvChiTietPhieuNhap.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetInputMode(true);
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvChiTietPhieuNhap.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn mặt hàng cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc muốn xóa mặt hàng này?", "Xác nhận", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    var maHang = dgvChiTietPhieuNhap.CurrentRow.Cells["MaHang"].Value.ToString();
                    DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                    {
                        // Xóa chi tiết
                        string deleteQuery = @"
                            DELETE FROM [dbo].[Chi_Tiet_Phieu_Nhap] 
                            WHERE MaPhieu = @MaPhieu AND MaHang = @MaHang";
                        using (var cmd = new SqlCommand(deleteQuery, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmd.Parameters.AddWithValue("@MaHang", maHang);
                            cmd.ExecuteNonQuery();
                        }

                        // Cập nhật tổng tiền
                        UpdateTongTien(connection, transaction);
                    });

                    LoadData();
                    ClearInputs();
                    MessageBox.Show("Đã xóa mặt hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa mặt hàng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (cboHangHoa.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn hàng hóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    // Kiểm tra xem mặt hàng đã tồn tại trong phiếu chưa
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM [dbo].[Chi_Tiet_Phieu_Nhap]
                        WHERE MaPhieu = @MaPhieu AND MaHang = @MaHang";
                    
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                        cmd.Parameters.AddWithValue("@MaHang", cboHangHoa.SelectedValue);
                        int exists = (int)cmd.ExecuteScalar();

                        string query;
                        if (exists > 0)
                        {
                            query = @"
                                UPDATE [dbo].[Chi_Tiet_Phieu_Nhap]
                                SET SoLuong = @SoLuong,
                                    DonGia = @DonGia,
                                    ThanhTien = @ThanhTien
                                WHERE MaPhieu = @MaPhieu AND MaHang = @MaHang";
                        }
                        else
                        {
                            query = @"
                                INSERT INTO [dbo].[Chi_Tiet_Phieu_Nhap] (MaPhieu, MaHang, SoLuong, DonGia, ThanhTien)
                                VALUES (@MaPhieu, @MaHang, @SoLuong, @DonGia, @ThanhTien)";
                        }

                        using (var cmdUpsert = new SqlCommand(query, connection))
                        {
                            cmdUpsert.Transaction = transaction;
                            cmdUpsert.Parameters.AddWithValue("@MaPhieu", maPhieu);
                            cmdUpsert.Parameters.AddWithValue("@MaHang", cboHangHoa.SelectedValue);
                            cmdUpsert.Parameters.AddWithValue("@SoLuong", numSoLuong.Value);
                            cmdUpsert.Parameters.AddWithValue("@DonGia", numDonGia.Value);
                            cmdUpsert.Parameters.AddWithValue("@ThanhTien", numSoLuong.Value * numDonGia.Value);
                            cmdUpsert.ExecuteNonQuery();
                        }
                    }

                    // Cập nhật tổng tiền
                    UpdateTongTien(connection, transaction);
                });

                LoadData();
                SetInputMode(false);
                MessageBox.Show("Đã lưu thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTongTien(SqlConnection connection, SqlTransaction transaction)
        {
            string updateQuery = @"
                UPDATE [dbo].[Phieu_Nhap]
                SET TongTien = (
                    SELECT ISNULL(SUM(ThanhTien), 0)
                    FROM [dbo].[Chi_Tiet_Phieu_Nhap]
                    WHERE MaPhieu = @MaPhieu
                )
                WHERE MaPhieu = @MaPhieu";

            using (var cmd = new SqlCommand(updateQuery, connection))
            {
                cmd.Transaction = transaction;
                cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                cmd.ExecuteNonQuery();
            }
        }

        private void ClearInputs()
        {
            cboHangHoa.SelectedIndex = -1;
            numSoLuong.Value = 1;
            numDonGia.Value = 0;
        }

        private void SetInputMode(bool isEditing)
        {
            cboHangHoa.Enabled = isEditing;
            numSoLuong.Enabled = isEditing;
            numDonGia.Enabled = isEditing;

            btnThem.Enabled = !isEditing;
            btnSua.Enabled = !isEditing;
            btnXoa.Enabled = !isEditing;
            btnLuu.Enabled = isEditing;
        }
    }
}
