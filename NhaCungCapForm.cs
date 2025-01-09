using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace QuanLyKho
{
    public partial class NhaCungCapForm : Form
    {
        private DataGridView dgvNhaCungCap;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLuu;
        private TextBox txtMaNCC;
        private TextBox txtTenNCC;
        private TextBox txtDiaChi;
        private TextBox txtSDT;
        private TextBox txtEmail;
        private Label lblStatus;
        private TextBox txtTimKiem;
        private Label lblTimKiem;

        public NhaCungCapForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản Lý Nhà Cung Cấp";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView
            dgvNhaCungCap = new DataGridView
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
            dgvNhaCungCap.SelectionChanged += DgvNhaCungCap_SelectionChanged;
            this.Controls.Add(dgvNhaCungCap);

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

            // Mã NCC
            var lblMaNCC = new Label { Text = "Mã NCC:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtMaNCC = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblMaNCC, txtMaNCC });

            // Tên NCC
            y += spacing;
            var lblTenNCC = new Label { Text = "Tên NCC:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtTenNCC = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblTenNCC, txtTenNCC });

            // Địa chỉ
            y += spacing;
            var lblDiaChi = new Label { Text = "Địa chỉ:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtDiaChi = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblDiaChi, txtDiaChi });

            // SĐT
            y += spacing;
            var lblSDT = new Label { Text = "SĐT:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtSDT = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblSDT, txtSDT });

            // Email
            y += spacing;
            var lblEmail = new Label { Text = "Email:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtEmail = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            inputPanel.Controls.AddRange(new Control[] { lblEmail, txtEmail });

            // Tìm kiếm
            y += spacing;
            lblTimKiem = new Label { Text = "Tìm kiếm:", Location = new Point(10, y), Size = new Size(labelWidth, 20) };
            txtTimKiem = new TextBox { Location = new Point(90, y), Size = new Size(controlWidth, 25) };
            txtTimKiem.TextChanged += TxtTimKiem_TextChanged;
            inputPanel.Controls.AddRange(new Control[] { lblTimKiem, txtTimKiem });

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
        }

        private void LoadData()
        {
            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        SELECT MaNCC, TenNCC, DiaChi, SDT, Email
                        FROM [dbo].[Nha_Cung_Cap]
                        ORDER BY MaNCC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Transaction = transaction;
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvNhaCungCap.DataSource = dataTable;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvNhaCungCap_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNhaCungCap.CurrentRow != null)
            {
                var row = dgvNhaCungCap.CurrentRow;
                txtMaNCC.Text = row.Cells["MaNCC"].Value.ToString();
                txtTenNCC.Text = row.Cells["TenNCC"].Value.ToString();
                txtDiaChi.Text = row.Cells["DiaChi"].Value?.ToString() ?? "";
                txtSDT.Text = row.Cells["SDT"].Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            ClearInputs();
            SetInputMode(true);
            lblStatus.Text = "Đang thêm nhà cung cấp mới...";
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvNhaCungCap.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SetInputMode(true);
            lblStatus.Text = "Đang sửa thông tin nhà cung cấp...";
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhaCungCap.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var maNCC = dgvNhaCungCap.CurrentRow.Cells["MaNCC"].Value.ToString();
            if (MessageBox.Show($"Bạn có chắc muốn xóa nhà cung cấp này?", "Xác nhận", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                    {
                        string query = "DELETE FROM [dbo].[Nha_Cung_Cap] WHERE MaNCC = @MaNCC";
                        using (var cmd = new SqlCommand(query, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@MaNCC", maNCC);
                            cmd.ExecuteNonQuery();
                        }
                    });

                    LoadData();
                    ClearInputs();
                    lblStatus.Text = "Đã xóa nhà cung cấp thành công!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa nhà cung cấp: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaNCC.Text) || string.IsNullOrEmpty(txtTenNCC.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseConnection.ExecuteInTransaction((connection, transaction) =>
                {
                    string query = @"
                        IF EXISTS (SELECT 1 FROM [dbo].[Nha_Cung_Cap] WHERE MaNCC = @MaNCC)
                            UPDATE [dbo].[Nha_Cung_Cap]
                            SET TenNCC = @TenNCC,
                                DiaChi = @DiaChi,
                                SDT = @SDT,
                                Email = @Email
                            WHERE MaNCC = @MaNCC
                        ELSE
                            INSERT INTO [dbo].[Nha_Cung_Cap] (MaNCC, TenNCC, DiaChi, SDT, Email)
                            VALUES (@MaNCC, @TenNCC, @DiaChi, @SDT, @Email)";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@MaNCC", txtMaNCC.Text);
                        cmd.Parameters.AddWithValue("@TenNCC", txtTenNCC.Text);
                        cmd.Parameters.AddWithValue("@DiaChi", string.IsNullOrEmpty(txtDiaChi.Text) ? DBNull.Value : (object)txtDiaChi.Text);
                        cmd.Parameters.AddWithValue("@SDT", string.IsNullOrEmpty(txtSDT.Text) ? DBNull.Value : (object)txtSDT.Text);
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);
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
            txtMaNCC.Text = "";
            txtTenNCC.Text = "";
            txtDiaChi.Text = "";
            txtSDT.Text = "";
            txtEmail.Text = "";
        }

        private void SetInputMode(bool isEditing)
        {
            txtMaNCC.Enabled = isEditing;
            txtTenNCC.Enabled = isEditing;
            txtDiaChi.Enabled = isEditing;
            txtSDT.Enabled = isEditing;
            txtEmail.Enabled = isEditing;

            btnThem.Enabled = !isEditing;
            btnSua.Enabled = !isEditing;
            btnXoa.Enabled = !isEditing;
            btnLuu.Enabled = isEditing;
        }

        private void TxtTimKiem_TextChanged(object sender, EventArgs e)
        {
            string searchValue = txtTimKiem.Text.Trim();

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                string query = @"
                    SELECT * FROM [dbo].[Nha_Cung_Cap]
                    WHERE MaNCC LIKE @SearchValue
                    OR TenNCC LIKE @SearchValue
                    OR DiaChi LIKE @SearchValue
                    OR SDT LIKE @SearchValue
                    OR Email LIKE @SearchValue";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchValue", "%" + searchValue + "%");
                    var dt = new DataTable();
                    dt.Load(command.ExecuteReader());
                    dgvNhaCungCap.DataSource = dt;
                }
            }
        }
    }
}
