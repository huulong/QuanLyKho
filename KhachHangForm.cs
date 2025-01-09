using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuanLyKho
{
    public partial class KhachHangForm : Form
    {
        private SqlConnection connection;

        public KhachHangForm()
        {
            InitializeComponent();
            connection = DatabaseConnection.GetConnection();
            LoadDanhSachKhachHang();
        }

        private void LoadDanhSachKhachHang()
        {
            var query = @"
                SELECT 
                    MaKH as N'Mã KH',
                    TenKH as N'Tên khách hàng',
                    DiaChi as N'Địa chỉ',
                    SDT as N'Điện thoại',
                    Email,
                    GhiChu as N'Ghi chú'
                FROM Khach_Hang
                ORDER BY TenKH";

            using (var cmd = new SqlCommand(query, connection))
            {
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                dgvKhachHang.DataSource = dt;
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenKH.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!");
                return;
            }

            if (!string.IsNullOrEmpty(txtDienThoai.Text) && !IsValidPhoneNumber(txtDienThoai.Text))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!");
                return;
            }

            if (!string.IsNullOrEmpty(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Email không hợp lệ!");
                return;
            }

            var query = @"
                INSERT INTO Khach_Hang (
                    MaKH, TenKH, DiaChi, SDT,
                    Email, GhiChu
                ) VALUES (
                    @MaKH, @TenKH, @DiaChi, @DienThoai,
                    @Email, @GhiChu
                )";

            try
            {
                using (var cmd = new SqlCommand(query, connection))
                {
                    string maKH = GenerateNextMaKH();
                    
                    cmd.Parameters.AddWithValue("@MaKH", int.Parse(maKH));
                    cmd.Parameters.AddWithValue("@TenKH", txtTenKH.Text.Trim());
                    cmd.Parameters.AddWithValue("@DiaChi", 
                        string.IsNullOrEmpty(txtDiaChi.Text) ? DBNull.Value : (object)txtDiaChi.Text);
                    cmd.Parameters.AddWithValue("@DienThoai", 
                        string.IsNullOrEmpty(txtDienThoai.Text) ? DBNull.Value : (object)txtDienThoai.Text);
                    cmd.Parameters.AddWithValue("@Email", 
                        string.IsNullOrEmpty(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);
                    cmd.Parameters.AddWithValue("@GhiChu", 
                        string.IsNullOrEmpty(txtGhiChu.Text) ? DBNull.Value : (object)txtGhiChu.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Thêm khách hàng thành công!");
                    LoadDanhSachKhachHang();
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void BtnCapNhat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaKH.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần sửa!");
                return;
            }

            if (string.IsNullOrEmpty(txtTenKH.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng!");
                return;
            }

            if (!string.IsNullOrEmpty(txtDienThoai.Text) && !IsValidPhoneNumber(txtDienThoai.Text))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!");
                return;
            }

            if (!string.IsNullOrEmpty(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Email không hợp lệ!");
                return;
            }

            if (!int.TryParse(txtMaKH.Text, out int maKH))
            {
                MessageBox.Show("Mã khách hàng phải là số nguyên!");
                return;
            }

            var query = @"
                UPDATE Khach_Hang 
                SET 
                    TenKH = @TenKH,
                    DiaChi = @DiaChi,
                    SDT = @DienThoai,
                    Email = @Email,
                    GhiChu = @GhiChu
                WHERE MaKH = @MaKH";

            try
            {
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@MaKH", maKH);
                    cmd.Parameters.AddWithValue("@TenKH", txtTenKH.Text.Trim());
                    cmd.Parameters.AddWithValue("@DiaChi", 
                        string.IsNullOrEmpty(txtDiaChi.Text) ? DBNull.Value : (object)txtDiaChi.Text);
                    cmd.Parameters.AddWithValue("@DienThoai", 
                        string.IsNullOrEmpty(txtDienThoai.Text) ? DBNull.Value : (object)txtDienThoai.Text);
                    cmd.Parameters.AddWithValue("@Email", 
                        string.IsNullOrEmpty(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);
                    cmd.Parameters.AddWithValue("@GhiChu", 
                        string.IsNullOrEmpty(txtGhiChu.Text) ? DBNull.Value : (object)txtGhiChu.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Cập nhật thông tin khách hàng thành công!");
                    LoadDanhSachKhachHang();
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaKH.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!");
                return;
            }

            if (!int.TryParse(txtMaKH.Text, out int maKH))
            {
                MessageBox.Show("Mã khách hàng phải là số nguyên!");
                return;
            }

            if (HasTransactions(maKH.ToString()))
            {
                MessageBox.Show("Không thể xóa khách hàng đã có giao dịch!");
                return;
            }

            if (MessageBox.Show(
                "Bạn có chắc chắn muốn xóa khách hàng này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            var query = "DELETE FROM Khach_Hang WHERE MaKH = @MaKH";
            
            try
            {
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@MaKH", maKH);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Xóa khách hàng thành công!");
                    LoadDanhSachKhachHang();
                    ResetForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }

        private void TxtTimKiem_TextChanged(object sender, EventArgs e)
        {
            var searchText = txtTimKiem.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadDanhSachKhachHang();
                return;
            }

            var query = @"
                SELECT 
                    MaKH as N'Mã KH',
                    TenKH as N'Tên khách hàng',
                    DiaChi as N'Địa chỉ',
                    SDT as N'Điện thoại',
                    Email,
                    GhiChu as N'Ghi chú'
                FROM Khach_Hang
                WHERE 
                    MaKH LIKE @Search
                    OR TenKH LIKE @Search
                    OR SDT LIKE @Search
                    OR Email LIKE @Search
                ORDER BY TenKH";

            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Search", $"%{searchText}%");
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                dgvKhachHang.DataSource = dt;
            }
        }

        private void DgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvKhachHang.Rows[e.RowIndex];
                txtMaKH.Text = row.Cells["Mã KH"].Value.ToString();
                txtTenKH.Text = row.Cells["Tên khách hàng"].Value.ToString();
                txtDiaChi.Text = row.Cells["Địa chỉ"].Value?.ToString();
                txtDienThoai.Text = row.Cells["Điện thoại"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                txtGhiChu.Text = row.Cells["Ghi chú"].Value?.ToString();
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^0\d{9,10}$");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateNextMaKH()
        {
            var query = @"
                SELECT COUNT(*)
                FROM Khach_Hang";

            using (var cmd = new SqlCommand(query, connection))
            {
                var count = (int)cmd.ExecuteScalar();
                return (count + 1).ToString();
            }
        }

        private bool HasTransactions(string maKH)
        {
            var query = @"
                SELECT COUNT(*) 
                FROM Phieu_Xuat 
                WHERE MaNV = @MaNV";

            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@MaNV", int.Parse(maKH));
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private void ResetForm()
        {
            txtMaKH.Clear();
            txtTenKH.Clear();
            txtDiaChi.Clear();
            txtDienThoai.Clear();
            txtEmail.Clear();
            txtGhiChu.Clear();
            txtTenKH.Focus();
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (connection != null && connection.State == ConnectionState.Open)
                connection.Close();
        }
    }
}
