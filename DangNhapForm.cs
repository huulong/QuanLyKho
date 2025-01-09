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
            btnDangNhap.Click += (sender, e) => BtnDangNhap_Click();
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

        private async void BtnDangNhap_Click()
        {
            // Lấy các giá trị từ TextBox
            TextBox? txtTenDangNhap = this.Controls["txtTenDangNhap"] as TextBox;
            TextBox? txtMatKhau = this.Controls["txtMatKhau"] as TextBox;

            if (txtTenDangNhap == null || txtMatKhau == null)
            {
                MessageBox.Show("Không tìm thấy điều khiển.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Vô hiệu hóa nút đăng nhập để tránh click nhiều lần
            Button? btnDangNhap = this.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Đăng Nhập");
            if (btnDangNhap != null) btnDangNhap.Enabled = false;

            try
            {
                // Thực hiện đăng nhập không đồng bộ
                (bool success, string? tenNhanVien) = await XacThucDangNhapAsync(txtTenDangNhap.Text, txtMatKhau.Text);

                if (success)
                {
                    // Không hiển thị MessageBox, chuyển thẳng sang form chính
                    TrangChuForm trangChuForm = new TrangChuForm(tenNhanVien);
                    trangChuForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác.", 
                        "Lỗi Đăng Nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Kích hoạt lại nút đăng nhập
                if (btnDangNhap != null) btnDangNhap.Enabled = true;
            }
        }

        private async Task<(bool Success, string? TenNhanVien)> XacThucDangNhapAsync(string tenDangNhap, string matKhau)
        {
            // Mã hóa mật khẩu trước khi so sánh
            string hashedPassword = MaHoaMatKhau(matKhau);

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                // Truy vấn kiểm tra tài khoản và mật khẩu
                string query = @"
                    SELECT TOP 1 TenNV 
                    FROM TT_Nhan_Vien NV
                    JOIN Tai_Khoan TK ON NV.MaNV = TK.MaNV
                    WHERE TK.TK = @TenDangNhap AND TK.MatKhau = @MatKhau";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                    command.Parameters.AddWithValue("@MatKhau", hashedPassword);

                    // Thực thi bất đồng bộ
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string? tenNhanVien = reader["TenNV"]?.ToString();
                            return (true, tenNhanVien);
                        }
                        return (false, null);
                    }
                }
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