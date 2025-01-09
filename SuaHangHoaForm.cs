using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QuanLyKho
{
    public partial class SuaHangHoaForm : Form
    {
        private readonly Container? _components;
        private readonly int _maVatTu;

        public SuaHangHoaForm(int maVatTu)
        {
            _components = new Container();
            _maVatTu = maVatTu;
            InitializeComponent();
            LoadVatTuInfo();
        }

        private void InitializeComponent()
        {
            // Tên Vật Tư
            Label lblTenVT = new Label
            {
                Text = "Tên Vật Tư:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(100, 25)
            };
            this.Controls.Add(lblTenVT);

            TextBox txtTenVT = new TextBox
            {
                Location = new System.Drawing.Point(150, 20),
                Size = new System.Drawing.Size(200, 25),
                Name = "txtTenVT"
            };
            this.Controls.Add(txtTenVT);

            // Đơn Vị Tính
            Label lblDonViTinh = new Label
            {
                Text = "Đơn Vị Tính:",
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(100, 25)
            };
            this.Controls.Add(lblDonViTinh);

            ComboBox cbDonViTinh = new ComboBox
            {
                Location = new System.Drawing.Point(150, 60),
                Size = new System.Drawing.Size(200, 25),
                Name = "cbDonViTinh",
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cbDonViTinh);

            // Nhà Cung Cấp
            Label lblNhaCungCap = new Label
            {
                Text = "Nhà Cung Cấp:",
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(100, 25)
            };
            this.Controls.Add(lblNhaCungCap);

            ComboBox cbNhaCungCap = new ComboBox
            {
                Location = new System.Drawing.Point(150, 100),
                Size = new System.Drawing.Size(200, 25),
                Name = "cbNhaCungCap",
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cbNhaCungCap);

            // Ghi Chú
            Label lblGhiChu = new Label
            {
                Text = "Ghi Chú:",
                Location = new System.Drawing.Point(20, 140),
                Size = new System.Drawing.Size(100, 25)
            };
            this.Controls.Add(lblGhiChu);

            TextBox txtGhiChu = new TextBox
            {
                Location = new System.Drawing.Point(150, 140),
                Size = new System.Drawing.Size(200, 50),
                Multiline = true,
                Name = "txtGhiChu"
            };
            this.Controls.Add(txtGhiChu);

            // Nút Sửa
            Button btnSua = new Button
            {
                Text = "Sửa",
                Location = new System.Drawing.Point(100, 220),
                Size = new System.Drawing.Size(100, 30),
                Name = "btnSua"
            };
            btnSua.Click += BtnSua_Click;
            this.Controls.Add(btnSua);

            // Cấu hình Form
            this.Text = "Sửa Vật Tư";
            this.Size = new System.Drawing.Size(400, 320);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Nạp dữ liệu cho ComboBox
            LoadDonViTinhData();
            LoadNhaCungCapData();
        }

        private void LoadDonViTinhData()
        {
            ComboBox cbDonViTinh = (ComboBox)this.Controls["cbDonViTinh"];
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    string query = "SELECT MaDVT, TenDVT FROM Don_Vi_Tinh";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cbDonViTinh.Items.Add(new KeyValuePair<int, string>(Convert.ToInt32(reader["MaDVT"]), reader["TenDVT"].ToString()));
                            }
                        }
                    }
                }
                cbDonViTinh.DisplayMember = "Value";
                cbDonViTinh.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadNhaCungCapData()
        {
            ComboBox cbNhaCungCap = (ComboBox)this.Controls["cbNhaCungCap"];
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    string query = "SELECT MaNCC, TenNCC FROM Nha_Cung_Cap";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cbNhaCungCap.Items.Add(new KeyValuePair<int, string>(Convert.ToInt32(reader["MaNCC"]), reader["TenNCC"].ToString()));
                            }
                        }
                    }
                }
                cbNhaCungCap.DisplayMember = "Value";
                cbNhaCungCap.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVatTuInfo()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    string query = @"
                        SELECT TenVT, MaDVT, MaNCC, GhiChu 
                        FROM VatTu 
                        WHERE MaVatTu = @MaVatTu";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaVatTu", _maVatTu);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TextBox txtTenVT = (TextBox)this.Controls["txtTenVT"];
                                ComboBox cbDonViTinh = (ComboBox)this.Controls["cbDonViTinh"];
                                ComboBox cbNhaCungCap = (ComboBox)this.Controls["cbNhaCungCap"];
                                TextBox txtGhiChu = (TextBox)this.Controls["txtGhiChu"];

                                txtTenVT.Text = reader["TenVT"].ToString();
                                txtGhiChu.Text = reader["GhiChu"].ToString();

                                // Chọn Đơn Vị Tính
                                int maDVT = Convert.ToInt32(reader["MaDVT"]);
                                foreach (var item in cbDonViTinh.Items)
                                {
                                    var kvp = (KeyValuePair<int, string>)item;
                                    if (kvp.Key == maDVT)
                                    {
                                        cbDonViTinh.SelectedItem = item;
                                        break;
                                    }
                                }

                                // Chọn Nhà Cung Cấp
                                int maNCC = Convert.ToInt32(reader["MaNCC"]);
                                foreach (var item in cbNhaCungCap.Items)
                                {
                                    var kvp = (KeyValuePair<int, string>)item;
                                    if (kvp.Key == maNCC)
                                    {
                                        cbNhaCungCap.SelectedItem = item;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            TextBox txtTenVT = (TextBox)this.Controls["txtTenVT"];
            ComboBox cbDonViTinh = (ComboBox)this.Controls["cbDonViTinh"];
            ComboBox cbNhaCungCap = (ComboBox)this.Controls["cbNhaCungCap"];
            TextBox txtGhiChu = (TextBox)this.Controls["txtGhiChu"];

            if (string.IsNullOrWhiteSpace(txtTenVT.Text))
            {
                MessageBox.Show("Vui lòng nhập tên vật tư.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbDonViTinh.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbNhaCungCap.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    string query = @"
                        UPDATE VatTu 
                        SET TenVT = @TenVT, 
                            MaDVT = @MaDVT, 
                            MaNCC = @MaNCC, 
                            GhiChu = @GhiChu 
                        WHERE MaVatTu = @MaVatTu";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        var selectedDVT = (KeyValuePair<int, string>)cbDonViTinh.SelectedItem;
                        var selectedNCC = (KeyValuePair<int, string>)cbNhaCungCap.SelectedItem;

                        command.Parameters.AddWithValue("@MaVatTu", _maVatTu);
                        command.Parameters.AddWithValue("@TenVT", txtTenVT.Text);
                        command.Parameters.AddWithValue("@MaDVT", selectedDVT.Key);
                        command.Parameters.AddWithValue("@MaNCC", selectedNCC.Key);
                        command.Parameters.AddWithValue("@GhiChu", txtGhiChu.Text);

                        int rowsAffected = command.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Sửa vật tư thành công.", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Không thể sửa vật tư.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
