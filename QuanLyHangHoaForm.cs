using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKho
{
    public partial class QuanLyHangHoaForm : Form
    {
        private readonly Container? _components;
        private readonly DataGridView _dataGridViewHangHoa;
        private readonly Button _btnThem;
        private readonly Button _btnSua;
        private readonly Button _btnXoa;
        private readonly Button _btnDangXuat;
        private TextBox txtTimKiem;
        private Label lblTimKiem;

        public QuanLyHangHoaForm()
        {
            _components = new Container();
            _dataGridViewHangHoa = InitializeDataGridView();
            _btnThem = InitializeButton("Thêm Hàng Hóa", 20, 360, BtnThem_Click);
            _btnSua = InitializeButton("Sửa Hàng Hóa", 150, 360, BtnSua_Click);
            _btnXoa = InitializeButton("Xóa Hàng Hóa", 280, 360, BtnXoa_Click);
            _btnDangXuat = InitializeButton("Đăng Xuất", 500, 360, BtnDangXuat_Click);

            txtTimKiem = new TextBox();
            txtTimKiem.Location = new Point(20, 330);
            txtTimKiem.Size = new Size(200, 30);
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;

            lblTimKiem = new Label();
            lblTimKiem.Text = "Tìm kiếm sản phẩm:";
            lblTimKiem.Location = new Point(20, 300);
            lblTimKiem.Size = new Size(120, 30);

            InitializeComponent();
            LoadDanhSachHangHoa();
        }

        private DataGridView InitializeDataGridView()
        {
            var dataGridView = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(600, 300),
                Name = "dataGridViewHangHoa",
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            return dataGridView;
        }

        private Button InitializeButton(string text, int x, int y, EventHandler clickHandler)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 30),
                Name = text.Replace(" ", ""),
                Tag = text
            };
        }

        private void InitializeComponent()
        {
            // Cấu hình Form
            Text = "Quản Lý Hàng Hóa";
            this.ClientSize = new System.Drawing.Size(900, 600); // Thay đổi kích thước cửa sổ
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // Thêm các điều khiển vào Form
            Controls.Add(_dataGridViewHangHoa);
            Controls.Add(_btnThem);
            Controls.Add(_btnSua);
            Controls.Add(_btnXoa);
            Controls.Add(_btnDangXuat);
            Controls.Add(txtTimKiem);
            Controls.Add(lblTimKiem);

            // Gán sự kiện Click
            _btnThem.Click += BtnThem_Click;
            _btnSua.Click += BtnSua_Click;
            _btnXoa.Click += BtnXoa_Click;
            _btnDangXuat.Click += BtnDangXuat_Click;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void LoadDanhSachHangHoa()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    string query = @"
                        SELECT 
                            VT.MaVatTu, 
                            VT.TenVT, 
                            DVT.TenDVT AS DonViTinh, 
                            NCC.TenNCC AS NhaCungCap, 
                            VT.GhiChu
                        FROM 
                            VatTu VT
                            INNER JOIN Don_Vi_Tinh DVT ON VT.MaDVT = DVT.MaDVT
                            INNER JOIN Nha_Cung_Cap NCC ON VT.MaNCC = NCC.MaNCC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dtHangHoa = new DataTable();
                    adapter.Fill(dtHangHoa);

                    _dataGridViewHangHoa.DataSource = dtHangHoa;

                    // Đặt tên cột tiếng Việt
                    _dataGridViewHangHoa.Columns["MaVatTu"].HeaderText = "Mã Vật Tư";
                    _dataGridViewHangHoa.Columns["TenVT"].HeaderText = "Tên Vật Tư";
                    _dataGridViewHangHoa.Columns["DonViTinh"].HeaderText = "Đơn Vị Tính";
                    _dataGridViewHangHoa.Columns["NhaCungCap"].HeaderText = "Nhà Cung Cấp";
                    _dataGridViewHangHoa.Columns["GhiChu"].HeaderText = "Ghi Chú";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtTimKiem_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtTimKiem.Text.Trim();
            // Kiểm tra nếu không có giá trị tìm kiếm
            if (string.IsNullOrEmpty(searchText))
            {
                LoadDanhSachHangHoa(); // Tải lại danh sách hàng hóa
                return;
            }

            // Câu lệnh SQL để tìm kiếm
            var query = @"
                SELECT 
                    VT.MaVatTu, 
                    VT.TenVT, 
                    DVT.TenDVT AS DonViTinh, 
                    NCC.TenNCC AS NhaCungCap, 
                    VT.GhiChu
                FROM 
                    VatTu VT
                    INNER JOIN Don_Vi_Tinh DVT ON VT.MaDVT = DVT.MaDVT
                    INNER JOIN Nha_Cung_Cap NCC ON VT.MaNCC = NCC.MaNCC
                WHERE VT.MaVatTu LIKE @Search OR VT.TenVT LIKE @Search
            ";

            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchText + "%");
                        var dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        _dataGridViewHangHoa.DataSource = dt; // Cập nhật DataGridView

                        // Đặt tên cột tiếng Việt
                        _dataGridViewHangHoa.Columns["MaVatTu"].HeaderText = "Mã Vật Tư";
                        _dataGridViewHangHoa.Columns["TenVT"].HeaderText = "Tên Vật Tư";
                        _dataGridViewHangHoa.Columns["DonViTinh"].HeaderText = "Đơn Vị Tính";
                        _dataGridViewHangHoa.Columns["NhaCungCap"].HeaderText = "Nhà Cung Cấp";
                        _dataGridViewHangHoa.Columns["GhiChu"].HeaderText = "Ghi Chú";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            ThemHangHoaForm themForm = new ThemHangHoaForm();
            themForm.ShowDialog();
            LoadDanhSachHangHoa(); // Tải lại danh sách sau khi thêm
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_dataGridViewHangHoa.SelectedRows.Count > 0)
            {
                int maVatTu = Convert.ToInt32(_dataGridViewHangHoa.SelectedRows[0].Cells["MaVatTu"].Value);
                SuaHangHoaForm suaForm = new SuaHangHoaForm(maVatTu);
                suaForm.ShowDialog();
                LoadDanhSachHangHoa(); // Tải lại danh sách sau khi sửa
            }
            else
            {
                MessageBox.Show("Vui lòng chọn hàng hóa để sửa.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_dataGridViewHangHoa.SelectedRows.Count > 0)
            {
                int maVatTu = Convert.ToInt32(_dataGridViewHangHoa.SelectedRows[0].Cells["MaVatTu"].Value);
                
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa hàng hóa này?", "Xác Nhận Xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (SqlConnection connection = DatabaseConnection.GetConnection())
                        {
                            string query = "DELETE FROM VatTu WHERE MaVatTu = @MaVatTu";
                            
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@MaVatTu", maVatTu);
                                int rowsAffected = command.ExecuteNonQuery();
                                
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Xóa hàng hóa thành công.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadDanhSachHangHoa(); // Tải lại danh sách sau khi xóa
                                }
                                else
                                {
                                    MessageBox.Show("Không thể xóa hàng hóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn hàng hóa để xóa.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDangXuat_Click(object? sender, EventArgs e)
        {
            // Quay lại form đăng nhập
            DangNhapForm dangNhapForm = new DangNhapForm();
            dangNhapForm.Show();
            Close();
        }
    }
}
