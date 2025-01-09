using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyKho
{
    public partial class QuanLyNguoiDungForm : Form
    {
        private DataGridView dgvNguoiDung;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLuu;
        private TextBox txtMaNV;
        private TextBox txtTenNV;
        private TextBox txtTaiKhoan;
        private TextBox txtMatKhau;
        private ComboBox cboQuyen;
        private Label lblStatus;

        public QuanLyNguoiDungForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Người Dùng";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView
            dgvNguoiDung = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(700, 500),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvNguoiDung.SelectionChanged += DgvNguoiDung_SelectionChanged;
            this.Controls.Add(dgvNguoiDung);

            // Input panel
            var inputPanel = new Panel
            {
                Location = new Point(720, 10),
                Size = new Size(250, 500),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(inputPanel);

            // Labels and TextBoxes
            var y = 20;
            var labelWidth = 80;
            var controlWidth = 150;
            var spacing = 40;

            // Mã NV
            var lblMaNV = new Label { Text = "Mã NV:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtMaNV = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblMaNV, txtMaNV });

            // Tên NV
            y += spacing;
            var lblTenNV = new Label { Text = "Tên NV:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtTenNV = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblTenNV, txtTenNV });

            // Tài khoản
            y += spacing;
            var lblTaiKhoan = new Label { Text = "Tài khoản:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtTaiKhoan = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblTaiKhoan, txtTaiKhoan });

            // Mật khẩu
            y += spacing;
            var lblMatKhau = new Label { Text = "Mật khẩu:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtMatKhau = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25), PasswordChar = '*' };
            inputPanel.Controls.AddRange(new Control[] { lblMatKhau, txtMatKhau });

            // Quyền
            y += spacing;
            var lblQuyen = new Label { Text = "Quyền:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            cboQuyen = new ComboBox { Location = new Point(90, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.AddRange(new Control[] { lblQuyen, cboQuyen });

            // Buttons
            y += spacing + 20;
            var buttonWidth = 100;
            var buttonSpacing = 10;
            var buttonsStartX = (inputPanel.Width - (buttonWidth * 2 + buttonSpacing)) / 2;

            btnThem = new Button { Text = "Thêm", Location = new Point(buttonsStartX, y), Size = new Size(buttonWidth, 30) };
            btnSua = new Button { Text = "Sửa", Location = new Point(buttonsStartX + buttonWidth + buttonSpacing, y), Size = new Size(buttonWidth, 30) };
            
            y += 40;
            btnXoa = new Button { Text = "Xóa", Location = new Point(buttonsStartX, y), Size = new Size(buttonWidth, 30) };
            btnLuu = new Button { Text = "Lưu", Location = new Point(buttonsStartX + buttonWidth + buttonSpacing, y), Size = new Size(buttonWidth, 30), Enabled = false };

            inputPanel.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLuu });

            // Status label
            lblStatus = new Label
            {
                Location = new Point(10, 520),
                AutoSize = true,
                ForeColor = Color.Red
            };
            this.Controls.Add(lblStatus);

            // Events
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLuu.Click += BtnLuu_Click;

            LoadQuyenComboBox();
        }

        private void LoadData()
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        SELECT 
                            nv.MaNV,
                            nv.TenNV,
                            nv.GT,
                            nv.NgaySinh,
                            nv.DiaChi,
                            nv.SDT,
                            nv.Gmail,
                            tk.TK as TaiKhoan,
                            STRING_AGG(qh.TenQuyen, ', ') as Quyen
                        FROM TT_Nhan_Vien nv
                        LEFT JOIN Tai_Khoan tk ON nv.MaNV = tk.MaNV
                        LEFT JOIN Phan_Quyen pq ON nv.MaNV = pq.MaNV
                        LEFT JOIN Quyen_Han qh ON pq.MaQuyen = qh.MaQuyen
                        GROUP BY nv.MaNV, nv.TenNV, nv.GT, nv.NgaySinh, nv.DiaChi, nv.SDT, nv.Gmail, tk.TK
                        ORDER BY nv.MaNV";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvNguoiDung.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadQuyenComboBox()
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = "SELECT MaQuyen, TenQuyen FROM Quyen_Han ORDER BY MaQuyen";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        using (var reader = command.ExecuteReader())
                        {
                            cboQuyen.Items.Clear();
                            while (reader.Read())
                            {
                                cboQuyen.Items.Add(new QuyenItem
                                {
                                    MaQuyen = reader.GetInt32(0),
                                    TenQuyen = reader.GetString(1)
                                });
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách quyền: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvNguoiDung_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNguoiDung.CurrentRow != null)
            {
                var row = dgvNguoiDung.CurrentRow;
                txtMaNV.Text = row.Cells["MaNV"].Value.ToString();
                txtTenNV.Text = row.Cells["TenNV"].Value.ToString();
                txtTaiKhoan.Text = row.Cells["TaiKhoan"].Value?.ToString() ?? "";
                txtMatKhau.Text = "";  // Không hiển thị mật khẩu
                
                string quyen = row.Cells["Quyen"].Value?.ToString() ?? "";
                foreach (QuyenItem item in cboQuyen.Items)
                {
                    if (quyen.Contains(item.TenQuyen))
                    {
                        cboQuyen.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            ClearInputs();
            SetInputMode(true);
            lblStatus.Text = "Đang thêm người dùng mới...";
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvNguoiDung.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn người dùng cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetInputMode(true);
            lblStatus.Text = "Đang sửa thông tin người dùng...";
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNguoiDung.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn người dùng cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var maNV = dgvNguoiDung.CurrentRow.Cells["MaNV"].Value.ToString();
            if (MessageBox.Show($"Bạn có chắc muốn xóa người dùng này?", "Xác nhận", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                    {
                        // Xóa phân quyền
                        using (var cmd = new SqlCommand("DELETE FROM Phan_Quyen WHERE MaNV = @MaNV", connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaNV", maNV);
                            cmd.ExecuteNonQuery();
                        }

                        // Xóa tài khoản
                        using (var cmd = new SqlCommand("DELETE FROM Tai_Khoan WHERE MaNV = @MaNV", connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaNV", maNV);
                            cmd.ExecuteNonQuery();
                        }

                        // Xóa thông tin nhân viên
                        using (var cmd = new SqlCommand("DELETE FROM TT_Nhan_Vien WHERE MaNV = @MaNV", connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaNV", maNV);
                            cmd.ExecuteNonQuery();
                        }
                    });

                    LoadData();
                    ClearInputs();
                    lblStatus.Text = "Đã xóa người dùng thành công!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa người dùng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaNV.Text) || string.IsNullOrEmpty(txtTenNV.Text) || 
                string.IsNullOrEmpty(txtTaiKhoan.Text) || cboQuyen.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    var maNV = int.Parse(txtMaNV.Text);
                    var quyenItem = (QuyenItem)cboQuyen.SelectedItem;

                    // Kiểm tra và thêm/cập nhật thông tin nhân viên
                    string nhanVienQuery = @"
                        IF EXISTS (SELECT 1 FROM TT_Nhan_Vien WHERE MaNV = @MaNV)
                            UPDATE TT_Nhan_Vien SET TenNV = @TenNV WHERE MaNV = @MaNV
                        ELSE
                            INSERT INTO TT_Nhan_Vien (MaNV, TenNV) VALUES (@MaNV, @TenNV)";

                    using (var cmd = new SqlCommand(nhanVienQuery, connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.Parameters.AddWithValue("@TenNV", txtTenNV.Text);
                        cmd.ExecuteNonQuery();
                    }

                    // Kiểm tra và thêm/cập nhật tài khoản
                    string taiKhoanQuery = @"
                        IF EXISTS (SELECT 1 FROM Tai_Khoan WHERE MaNV = @MaNV)
                            UPDATE Tai_Khoan SET TK = @TK WHERE MaNV = @MaNV
                        ELSE
                            INSERT INTO Tai_Khoan (TK, MaNV, MatKhau) VALUES (@TK, @MaNV, @MatKhau)";

                    using (var cmd = new SqlCommand(taiKhoanQuery, connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.Parameters.AddWithValue("@TK", txtTaiKhoan.Text);
                        
                        // Chỉ cập nhật mật khẩu nếu có nhập mật khẩu mới
                        if (!string.IsNullOrEmpty(txtMatKhau.Text))
                        {
                            cmd.Parameters.AddWithValue("@MatKhau", HashPassword(txtMatKhau.Text));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@MatKhau", DBNull.Value);
                        }
                        
                        cmd.ExecuteNonQuery();
                    }

                    // Cập nhật phân quyền
                    using (var cmd = new SqlCommand("DELETE FROM Phan_Quyen WHERE MaNV = @MaNV", connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand("INSERT INTO Phan_Quyen (MaNV, MaQuyen) VALUES (@MaNV, @MaQuyen)", connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.Parameters.AddWithValue("@MaQuyen", quyenItem.MaQuyen);
                        cmd.ExecuteNonQuery();
                    }
                });

                LoadData();
                SetInputMode(false);
                lblStatus.Text = "Đã lưu thông tin thành công!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInputs()
        {
            txtMaNV.Text = "";
            txtTenNV.Text = "";
            txtTaiKhoan.Text = "";
            txtMatKhau.Text = "";
            cboQuyen.SelectedIndex = -1;
        }

        private void SetInputMode(bool isEditing)
        {
            txtMaNV.Enabled = isEditing;
            txtTenNV.Enabled = isEditing;
            txtTaiKhoan.Enabled = isEditing;
            txtMatKhau.Enabled = isEditing;
            cboQuyen.Enabled = isEditing;
            btnLuu.Enabled = isEditing;
            btnThem.Enabled = !isEditing;
            btnSua.Enabled = !isEditing;
            btnXoa.Enabled = !isEditing;
        }

        private class QuyenItem
        {
            public int MaQuyen { get; set; }
            public string TenQuyen { get; set; }

            public override string ToString()
            {
                return TenQuyen;
            }
        }
    }
}
