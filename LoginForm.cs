using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyKho
{
    public partial class LoginForm : Form
    {
        private TextBox txtTenDangNhap;
        private TextBox txtMatKhau;
        private int loginAttempts = 0;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Thiết lập form
            this.Text = "Đăng Nhập Hệ Thống Quản Lý Kho";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Panel chính
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            this.Controls.Add(mainPanel);

            // Logo hoặc icon
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(120, 120),
                Location = new Point(140, 20),
                Image = SystemIcons.Shield.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            mainPanel.Controls.Add(pictureBox);

            // Tiêu đề chào mừng
            Label lblWelcome = new Label
            {
                Text = "CHÀO MỪNG ĐẾN VỚI",
                Location = new Point(30, 150),
                Size = new Size(340, 25),
                Font = new Font("Arial", 12, FontStyle.Regular),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblWelcome);

            // Tiêu đề
            Label lblTitle = new Label
            {
                Text = "HỆ THỐNG QUẢN LÝ KHO",
                Location = new Point(30, 175),
                Size = new Size(340, 35),
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            // Subtitle
            Label lblSubtitle = new Label
            {
                Text = "Đăng nhập để sử dụng hệ thống",
                Location = new Point(30, 210),
                Size = new Size(340, 20),
                Font = new Font("Arial", 10),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblSubtitle);

            // Tên đăng nhập
            Label lblTenDangNhap = new Label
            {
                Text = "Tên đăng nhập:",
                Location = new Point(50, 240),
                Size = new Size(300, 20),
                Font = new Font("Arial", 9.5f)
            };
            mainPanel.Controls.Add(lblTenDangNhap);

            txtTenDangNhap = new TextBox
            {
                Location = new Point(50, 265),
                Size = new Size(300, 30),
                Font = new Font("Arial", 11),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            mainPanel.Controls.Add(txtTenDangNhap);

            // Mật khẩu
            Label lblMatKhau = new Label
            {
                Text = "Mật khẩu:",
                Location = new Point(50, 305),
                Size = new Size(300, 20),
                Font = new Font("Arial", 9.5f)
            };
            mainPanel.Controls.Add(lblMatKhau);

            txtMatKhau = new TextBox
            {
                Location = new Point(50, 330),
                Size = new Size(300, 30),
                Font = new Font("Arial", 11),
                PasswordChar = '•',
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            mainPanel.Controls.Add(txtMatKhau);

            // Nút đăng nhập
            Button btnDangNhap = new Button
            {
                Text = "ĐĂNG NHẬP",
                Location = new Point(50, 380),
                Size = new Size(300, 45),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDangNhap.FlatAppearance.BorderSize = 0;
            btnDangNhap.Click += BtnDangNhap_Click;
            mainPanel.Controls.Add(btnDangNhap);

            // Link đăng ký
            LinkLabel lnkDangKy = new LinkLabel
            {
                Text = "Chưa có tài khoản? Đăng ký ngay",
                Location = new Point(50, 440),
                Size = new Size(300, 25),
                Font = new Font("Arial", 9.5f),
                LinkColor = Color.FromArgb(0, 122, 204),
                ActiveLinkColor = Color.FromArgb(0, 102, 204),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lnkDangKy.Click += LnkDangKy_Click;
            mainPanel.Controls.Add(lnkDangKy);
        }

        private void BtnDangNhap_Click(object sender, EventArgs e)
        {
            // Kiểm tra thông tin đăng nhập
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenDangNhap.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    string hashedPassword = MaHoaMatKhau(txtMatKhau.Text);
                    string query = @"
                        SELECT tk.MaNV, nv.HoTen, nv.ChucVu
                        FROM Tai_Khoan tk
                        JOIN TT_Nhan_Vien nv ON tk.MaNV = nv.MaNV
                        WHERE tk.TK = @TenDangNhap AND tk.MatKhau = @MatKhau";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TenDangNhap", txtTenDangNhap.Text);
                        command.Parameters.AddWithValue("@MatKhau", hashedPassword);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int maNV = reader.GetInt32(0);
                                string hoTen = reader.GetString(1);
                                string chucVu = reader.GetString(2);
                                
                                MessageBox.Show(
                                    $"Xin chào {hoTen}!\nChức vụ: {chucVu}\n\nChúc bạn làm việc hiệu quả!", 
                                    "Đăng nhập thành công ✓", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Information);
                                
                                var mainForm = new TrangChuForm(hoTen, maNV);
                                this.Hide();
                                mainForm.FormClosed += (s, args) => this.Close();
                                mainForm.Show();
                            }
                            else
                            {
                                loginAttempts++;
                                if (loginAttempts >= 3)
                                {
                                    MessageBox.Show(
                                        "Bạn đã nhập sai thông tin đăng nhập quá 3 lần!\nVui lòng thử lại sau.", 
                                        "Cảnh báo bảo mật", 
                                        MessageBoxButtons.OK, 
                                        MessageBoxIcon.Warning);
                                    Application.Exit();
                                }
                                else
                                {
                                    MessageBox.Show(
                                        $"Thông tin đăng nhập không chính xác!\nBạn còn {3 - loginAttempts} lần thử.", 
                                        "Lỗi đăng nhập", 
                                        MessageBoxButtons.OK, 
                                        MessageBoxIcon.Error);
                                    txtMatKhau.Clear();
                                    txtMatKhau.Focus();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Đã xảy ra lỗi khi đăng nhập!\nChi tiết: {ex.Message}", 
                    "Lỗi hệ thống", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void LnkDangKy_Click(object sender, EventArgs e)
        {
            var dangKyForm = new DangKyForm();
            dangKyForm.ShowDialog();
        }

        private string MaHoaMatKhau(string matKhau)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(matKhau));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
