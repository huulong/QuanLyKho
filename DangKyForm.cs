using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyKho
{
    public partial class DangKyForm : Form
    {
        // Khai báo các TextBox là trường của lớp
        private TextBox txtTenDangNhap;
        private TextBox txtMatKhau;
        private TextBox txtMaNV;

        public DangKyForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Tiêu đề form
            this.Text = "Đăng Ký Tài Khoản";
            this.Size = new System.Drawing.Size(400, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Label chính
            Label lblTitle = new Label
            {
                Text = "ĐĂNG KÝ TÀI KHOẢN MỚI",
                Location = new System.Drawing.Point(30, 20),
                Size = new System.Drawing.Size(340, 30),
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Label hướng dẫn
            Label lblGuide = new Label
            {
                Text = "Vui lòng điền đầy đủ thông tin để đăng ký tài khoản mới",
                Location = new System.Drawing.Point(30, 60),
                Size = new System.Drawing.Size(340, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblGuide);

            // Nhãn Tên Đăng Nhập
            Label lblTenDangNhap = new Label
            {
                Text = "Tên Đăng Nhập:",
                Location = new System.Drawing.Point(30, 100),
                Size = new System.Drawing.Size(120, 25)
            };
            this.Controls.Add(lblTenDangNhap);

            // Ô Nhập Tên Đăng Nhập
            txtTenDangNhap = new TextBox
            {
                Location = new System.Drawing.Point(160, 100),
                Size = new System.Drawing.Size(200, 25),
                Name = "txtTenDangNhap"
            };
            this.Controls.Add(txtTenDangNhap);

            // Nhãn Mật Khẩu
            Label lblMatKhau = new Label
            {
                Text = "Mật Khẩu:",
                Location = new System.Drawing.Point(30, 140),
                Size = new System.Drawing.Size(120, 25)
            };
            this.Controls.Add(lblMatKhau);

            // Ô Nhập Mật Khẩu
            txtMatKhau = new TextBox
            {
                Location = new System.Drawing.Point(160, 140),
                Size = new System.Drawing.Size(200, 25),
                PasswordChar = '*',
                Name = "txtMatKhau"
            };
            this.Controls.Add(txtMatKhau);

            // Nhãn Mã Nhân Viên
            Label lblMaNV = new Label
            {
                Text = "Mã Nhân Viên:",
                Location = new System.Drawing.Point(30, 180),
                Size = new System.Drawing.Size(120, 25)
            };
            this.Controls.Add(lblMaNV);

            // Ô Nhập Mã Nhân Viên
            txtMaNV = new TextBox
            {
                Location = new System.Drawing.Point(160, 180),
                Size = new System.Drawing.Size(200, 25),
                Name = "txtMaNV"
            };
            this.Controls.Add(txtMaNV);

            // Label ghi chú
            Label lblNote = new Label
            {
                Text = "Lưu ý: Mã nhân viên phải là mã hợp lệ trong hệ thống",
                Location = new System.Drawing.Point(30, 220),
                Size = new System.Drawing.Size(340, 20),
                ForeColor = System.Drawing.Color.Red,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblNote);

            // Nút Đăng Ký
            Button btnDangKy = new Button
            {
                Text = "Đăng Ký",
                Location = new System.Drawing.Point(130, 260),
                Size = new System.Drawing.Size(140, 35),
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
            btnDangKy.Click += BtnDangKy_Click;
            this.Controls.Add(btnDangKy);

            // Link quay lại đăng nhập
            LinkLabel lnkBackToLogin = new LinkLabel
            {
                Text = "Đã có tài khoản? Đăng nhập ngay",
                Location = new System.Drawing.Point(100, 310),
                Size = new System.Drawing.Size(200, 25),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            lnkBackToLogin.Click += (sender, e) => this.Close();
            this.Controls.Add(lnkBackToLogin);
        }

        // Phương thức mã hóa mật khẩu đơn giản
        private string MaHoaMatKhau(string matKhau)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Chuyển đổi mật khẩu thành mảng byte
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(matKhau));

                // Chuyển đổi mảng byte thành chuỗi hex
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void BtnDangKy_Click(object sender, EventArgs e)
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text) ||
                string.IsNullOrWhiteSpace(txtMatKhau.Text) ||
                string.IsNullOrWhiteSpace(txtMaNV.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kiểm tra độ dài mật khẩu
            if (txtMatKhau.Text.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    // Kiểm tra xem nhân viên có tồn tại không
                    string checkNhanVienQuery = "SELECT COUNT(*) FROM TT_Nhan_Vien WHERE MaNV = @MaNV";

                    using (SqlCommand checkCommand = new SqlCommand(checkNhanVienQuery, connection))
                    {
                        // Chuyển đổi mã nhân viên sang số nguyên
                        if (!int.TryParse(txtMaNV.Text, out int maNV))
                        {
                            MessageBox.Show("Mã nhân viên phải là số.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        checkCommand.Parameters.AddWithValue("@MaNV", maNV);
                        int nhanVienCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (nhanVienCount == 0)
                        {
                            MessageBox.Show("Mã nhân viên không tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Kiểm tra tài khoản đã tồn tại chưa
                    string checkTaiKhoanQuery = "SELECT COUNT(*) FROM Tai_Khoan WHERE TK = @TenDangNhap";
                    using (SqlCommand checkTKCommand = new SqlCommand(checkTaiKhoanQuery, connection))
                    {
                        checkTKCommand.Parameters.AddWithValue("@TenDangNhap", txtTenDangNhap.Text);
                        int taiKhoanCount = Convert.ToInt32(checkTKCommand.ExecuteScalar());

                        if (taiKhoanCount > 0)
                        {
                            MessageBox.Show("Tên đăng nhập đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Mã hóa mật khẩu
                    string hashedPassword = MaHoaMatKhau(txtMatKhau.Text);

                    // Thêm tài khoản mới
                    string insertQuery = @"
                        INSERT INTO Tai_Khoan (TK, MaNV, MatKhau) 
                        VALUES (@TenDangNhap, @MaNV, @MatKhau)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@TenDangNhap", txtTenDangNhap.Text);
                        command.Parameters.AddWithValue("@MaNV", int.Parse(txtMaNV.Text));
                        command.Parameters.AddWithValue("@MatKhau", hashedPassword);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Đăng ký tài khoản thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Đăng ký không thành công.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Unique key violation
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Lỗi SQL: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
