using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyKho
{
    public partial class DangNhapForm : Form
    {
        public DangNhapForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Tên đăng nhập
            Label lblTenDangNhap = new Label();
            lblTenDangNhap.Text = "Tên Đăng Nhập:";
            lblTenDangNhap.Location = new System.Drawing.Point(20, 20);
            lblTenDangNhap.Size = new System.Drawing.Size(120, 25);
            this.Controls.Add(lblTenDangNhap);

            TextBox txtTenDangNhap = new TextBox();
            txtTenDangNhap.Location = new System.Drawing.Point(150, 20);
            txtTenDangNhap.Size = new System.Drawing.Size(200, 25);
            txtTenDangNhap.Name = "txtTenDangNhap";
            this.Controls.Add(txtTenDangNhap);

            // Mật khẩu
            Label lblMatKhau = new Label();
            lblMatKhau.Text = "Mật Khẩu:";
            lblMatKhau.Location = new System.Drawing.Point(20, 60);
            lblMatKhau.Size = new System.Drawing.Size(120, 25);
            this.Controls.Add(lblMatKhau);

            TextBox txtMatKhau = new TextBox();
            txtMatKhau.Location = new System.Drawing.Point(150, 60);
            txtMatKhau.Size = new System.Drawing.Size(200, 25);
            txtMatKhau.PasswordChar = '*';
            txtMatKhau.Name = "txtMatKhau";
            this.Controls.Add(txtMatKhau);

            // Nút Đăng Nhập
            Button btnDangNhap = new Button();
            btnDangNhap.Text = "Đăng Nhập";
            btnDangNhap.Location = new System.Drawing.Point(100, 120);
            btnDangNhap.Size = new System.Drawing.Size(100, 30);
            btnDangNhap.Click += (sender, e) => btnDangNhap_Click(sender, e);
            this.Controls.Add(btnDangNhap);

            // Nút Đăng Ký
            Button btnDangKy = new Button();
            btnDangKy.Text = "Đăng Ký";
            btnDangKy.Location = new System.Drawing.Point(210, 120);
            btnDangKy.Size = new System.Drawing.Size(100, 30);
            btnDangKy.Click += (sender, e) => BtnDangKy_Click();
            this.Controls.Add(btnDangKy);

            // Cấu hình Form
            this.Text = "Đăng Nhập Hệ Thống";
            this.Size = new System.Drawing.Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private string HashPassword(string matKhau)
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

        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            string taiKhoan = ((TextBox)this.Controls["txtTenDangNhap"]).Text.Trim();
            string matKhau = ((TextBox)this.Controls["txtMatKhau"]).Text;

            if (string.IsNullOrEmpty(taiKhoan) || string.IsNullOrEmpty(matKhau))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ((Button)sender).Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                var (success, tenNhanVien, maNV) = await XacThucDangNhapAsync(taiKhoan, matKhau);

                if (success)
                {
                    // Debug: Hiển thị thông tin đăng nhập
                    MessageBox.Show($"Đăng nhập thành công!\nTên NV: {tenNhanVien}\nMã NV: {maNV}", "Debug Info");
                    
                    TrangChuForm trangChuForm = new TrangChuForm(tenNhanVien, maNV);
                    this.Hide();
                    trangChuForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Tài khoản hoặc mật khẩu không đúng!", "Lỗi đăng nhập", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ((Button)sender).Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        private async Task<(bool success, string? tenNhanVien, int maNV)> XacThucDangNhapAsync(string taiKhoan, string matKhau)
        {
            try
            {
                string hashedPassword = HashPassword(matKhau);
                bool success = false;
                string? tenNhanVien = null;
                int maNV = 0;

                await Task.Run(() =>
                {
                    using (var connection = DatabaseConnection.CreateNewConnection())
                    {
                        string query = @"
                            SELECT nv.TenNV, nv.MaNV
                            FROM [dbo].[Tai_Khoan] tk
                            JOIN [dbo].[TT_Nhan_Vien] nv ON tk.MaNV = nv.MaNV
                            WHERE tk.TK = @TaiKhoan AND tk.MatKhau = @MatKhau";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@TaiKhoan", taiKhoan);
                            command.Parameters.AddWithValue("@MatKhau", hashedPassword);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    success = true;
                                    tenNhanVien = reader.GetString(0);
                                    maNV = reader.GetInt32(1);
                                }
                            }
                        }
                    }
                });

                return (success, tenNhanVien, maNV);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void BtnDangKy_Click()
        {
            // Mở form đăng ký
            DangKyForm dangKyForm = new DangKyForm();
            dangKyForm.ShowDialog();
        }
    }
}