# Quản Lý Kho - Hướng Dẫn Chi Tiết

## 1. Cấu Trúc Cơ Sở Dữ Liệu

### 1.1 Bảng Phiếu Nhập (Phieu_Nhap)
- `MaPhieu`: Khóa chính, định danh duy nhất cho mỗi phiếu nhập
- `NgayNhap`: Thời gian tạo phiếu nhập
- `MaNCC`: Mã nhà cung cấp (liên kết với bảng Nha_Cung_Cap)
- `MaNV`: Mã nhân viên tạo phiếu
- `GhiChu`: Ghi chú thêm về phiếu nhập
- `TongTien`: Tổng giá trị của phiếu nhập

### 1.2 Bảng Phiếu Xuất (Phieu_Xuat)
- `MaPhieu`: Khóa chính, định danh duy nhất cho mỗi phiếu xuất
- `NgayXuat`: Thời gian tạo phiếu xuất
- `MaNV`: Mã nhân viên tạo phiếu
- `GhiChu`: Ghi chú thêm về phiếu xuất
- `TongTien`: Tổng giá trị của phiếu xuất

### 1.3 Bảng Chi Tiết Phiếu Nhập/Xuất
- `MaPhieu`: Khóa ngoại liên kết với phiếu tương ứng
- `MaHang`: Mã hàng hóa
- `SoLuong`: Số lượng nhập/xuất
- `DonGia`: Đơn giá
- `ThanhTien`: Thành tiền (SoLuong * DonGia)

## 2. Chức Năng Thống Kê và Báo Cáo

### 2.1 Thống Kê Nhập/Xuất (BtnThongKeNhapXuat_Click)
```csharp
private void BtnThongKeNhapXuat_Click(object sender, EventArgs e)
{
    // Lấy thởi gian thống kê (1 tháng gần nhất)
    var startDate = DateTime.Now.AddMonths(-1);
    var endDate = DateTime.Now;

    // Sử dụng Common Table Expression (CTE) để tính toán
    var query = @"
        WITH ThongKeNhap AS (
            // Thống kê phiếu nhập
            SELECT 
                pn.MaPhieu,
                SUM(ISNULL(ct.SoLuong, 0)) as TongSoLuong,
                SUM(ISNULL(pn.TongTien, 0)) as TongGiaTri
            FROM Phieu_Nhap pn
            LEFT JOIN Chi_Tiet_Phieu_Nhap ct ON pn.MaPhieu = ct.MaPhieu
            WHERE pn.NgayNhap BETWEEN @StartDate AND @EndDate
            GROUP BY pn.MaPhieu
        ),
        ThongKeXuat AS (
            // Thống kê phiếu xuất
            SELECT 
                px.MaPhieu,
                SUM(ISNULL(ct.SoLuong, 0)) as TongSoLuong,
                SUM(ISNULL(px.TongTien, 0)) as TongGiaTri
            FROM Phieu_Xuat px
            LEFT JOIN Chi_Tiet_Phieu_Xuat ct ON px.MaPhieu = ct.MaPhieu
            WHERE px.NgayXuat BETWEEN @StartDate AND @EndDate
            GROUP BY px.MaPhieu
        )
        // Tổng hợp kết quả
        SELECT 
            'Nhập' as LoaiPhieu,
            COUNT(*) as SoLuongPhieu,
            SUM(TongSoLuong) as TongSoLuong,
            SUM(TongGiaTri) as TongGiaTri
        FROM ThongKeNhap
        UNION ALL
        SELECT 
            'Xuất' as LoaiPhieu,
            COUNT(*) as SoLuongPhieu,
            SUM(TongSoLuong) as TongSoLuong,
            SUM(TongGiaTri) as TongGiaTri
        FROM ThongKeXuat";

    // Xử lý NULL values từ database
    while (reader.Read())
    {
        string loaiPhieu = reader["LoaiPhieu"].ToString();
        int soLuongPhieu = Convert.ToInt32(reader["SoLuongPhieu"]);
        int tongSoLuong = reader["TongSoLuong"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TongSoLuong"]);
        decimal tongGiaTri = reader["TongGiaTri"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TongGiaTri"]);
        // Hiển thị kết quả
        message += $"Phiếu {loaiPhieu}:\n";
        message += $"- Số lượng phiếu: {soLuongPhieu:N0}\n";
        message += $"- Tổng số lượng: {tongSoLuong:N0}\n";
        message += $"- Tổng giá trị: {tongGiaTri:N0} VNĐ\n\n";
    }
}
```

### 2.2 Báo Cáo Tồn Kho (BtnBaoCaoTonKho_Click)
```csharp
private void BtnBaoCaoTonKho_Click(object sender, EventArgs e)
{
    var query = @"
        // Lấy thông tin hàng tồn kho
        SELECT 
            hh.MaHang,
            hh.TenHang,
            dvt.TenDVT,
            hh.SoLuongTon,
            hh.GiaNhap,
            hh.GiaXuat,
            (hh.SoLuongTon * hh.GiaNhap) as GiaTriTon
        FROM Hang_Hoa hh
        LEFT JOIN Don_Vi_Tinh dvt ON hh.MaDVT = dvt.MaDVT
        WHERE hh.SoLuongTon > 0
        ORDER BY hh.SoLuongTon DESC";

    // Hiển thị thông tin từng mặt hàng
    while (reader.Read())
    {
        string maHang = reader["MaHang"].ToString();
        string tenHang = reader["TenHang"].ToString();
        string dvt = reader["TenDVT"].ToString();
        int soLuongTon = Convert.ToInt32(reader["SoLuongTon"]);
        decimal giaNhap = Convert.ToDecimal(reader["GiaNhap"]);
        decimal giaTriTon = Convert.ToDecimal(reader["GiaTriTon"]);

        message += $"{stt}. {tenHang} ({maHang})\n";
        message += $"   - Đơn vị tính: {dvt}\n";
        message += $"   - Số lượng tồn: {soLuongTon:N0}\n";
        message += $"   - Giá nhập: {giaNhap:N0} VNĐ\n";
        message += $"   - Giá trị tồn: {giaTriTon:N0} VNĐ\n\n";
    }
}
```

### 2.3 Báo Cáo Doanh Thu (BtnBaoCaoDoanhThu_Click)
```csharp
private void BtnBaoCaoDoanhThu_Click(object sender, EventArgs e)
{
    var query = @"
        WITH DoanhThuThang AS (
            // Tính doanh thu theo tháng
            SELECT 
                MONTH(NgayXuat) as Thang,
                YEAR(NgayXuat) as Nam,
                COUNT(DISTINCT MaPhieu) as SoPhieuXuat,
                SUM(TongTien) as DoanhThu
            FROM Phieu_Xuat
            WHERE YEAR(NgayXuat) = YEAR(GETDATE())
            GROUP BY MONTH(NgayXuat), YEAR(NgayXuat)
        )
        // Tính doanh thu lũy kế
        SELECT 
            Thang,
            Nam,
            SoPhieuXuat,
            DoanhThu,
            SUM(DoanhThu) OVER (ORDER BY Nam, Thang) as DoanhThuLuyKe
        FROM DoanhThuThang
        ORDER BY Nam, Thang";

    // Hiển thị báo cáo doanh thu theo tháng
    while (reader.Read())
    {
        int thang = Convert.ToInt32(reader["Thang"]);
        int soPhieuXuat = Convert.ToInt32(reader["SoPhieuXuat"]);
        decimal doanhThu = Convert.ToDecimal(reader["DoanhThu"]);
        decimal doanhThuLuyKe = Convert.ToDecimal(reader["DoanhThuLuyKe"]);

        message += $"Tháng {thang:D2}:\n";
        message += $"- Số phiếu xuất: {soPhieuXuat:N0}\n";
        message += $"- Doanh thu: {doanhThu:N0} VNĐ\n";
        message += $"- Doanh thu lũy kế: {doanhThuLuyKe:N0} VNĐ\n\n";
    }
}
```

## 3. Giải Thích Chi Tiết

### 3.1 Xử Lý NULL Values
- Sử dụng `ISNULL()` trong SQL để xử lý giá trị NULL từ database
- Kiểm tra `DBNull.Value` trong C# khi đọc dữ liệu
- Chuyển đổi an toàn sang các kiểu dữ liệu tương ứng

### 3.2 Tối Ưu Hiệu Suất
- Sử dụng Common Table Expression (CTE) để tổ chức code SQL rõ ràng
- Tận dụng các chỉ mục có sẵn trên các khóa chính/khóa ngoại
- Giới hạn dữ liệu bằng điều kiện thời gian

### 3.3 Định Dạng Hiển Thị
- Sử dụng `:N0` để định dạng số có dấu phân cách hàng nghìn
- Sử dụng `:D2` để định dạng số tháng có 2 chữ số
- Thêm đơn vị tiền tệ (VNĐ) cho các giá trị tiền

### 3.4 Xử Lý Lỗi
- Sử dụng khối try-catch để bắt và xử lý các ngoại lệ
- Hiển thị thông báo lỗi rõ ràng cho người dùng
- Đóng kết nối database an toàn trong khối finally

## 4. Các Tính Năng Có Thể Phát Triển Thêm

1. **Xuất Báo Cáo**
   - Xuất ra file Excel/PDF
   - Tạo mẫu báo cáo tùy chỉnh

2. **Biểu Đồ Trực Quan**
   - Thêm biểu đồ cột/đường cho doanh thu
   - Biểu đồ tròn cho cơ cấu hàng tồn

3. **Tùy Chỉnh Thời Gian**
   - Cho phép chọn khoảng thời gian báo cáo
   - Lưu tùy chọn mặc định

4. **Phân Tích Nâng Cao**
   - Dự báo xu hướng
   - Phân tích ABC
   - Tính toán các chỉ số tồn kho

## 5. Chức Năng Đăng Nhập và Phân Quyền

### 5.1 Đăng Nhập (LoginForm.cs)
```csharp
private void BtnDangNhap_Click(object sender, EventArgs e)
{
    // Kiểm tra thông tin đăng nhập
    var query = @"
        SELECT nv.*, cv.TenChucVu, cv.Quyen
        FROM Nhan_Vien nv
        JOIN Chuc_Vu cv ON nv.MaChucVu = cv.MaChucVu
        WHERE nv.TaiKhoan = @TaiKhoan AND nv.MatKhau = @MatKhau";

    // Mã hóa mật khẩu trước khi so sánh
    string hashedPassword = HashPassword(txtMatKhau.Text);
    
    // Lưu thông tin người dùng vào session
    if (reader.Read())
    {
        CurrentUser.MaNV = reader["MaNV"].ToString();
        CurrentUser.TenNV = reader["TenNV"].ToString();
        CurrentUser.ChucVu = reader["TenChucVu"].ToString();
        CurrentUser.Quyen = Convert.ToInt32(reader["Quyen"]);
        
        // Mở form chính với quyền tương ứng
        var mainForm = new TrangChuForm();
        mainForm.Show();
        this.Hide();
    }
    else
    {
        MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
    }
}

// Hàm mã hóa mật khẩu
private string HashPassword(string password)
{
    using (var sha256 = SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
```

### 5.2 Đăng Xuất
```csharp
private void BtnDangXuat_Click(object sender, EventArgs e)
{
    // Xóa thông tin người dùng
    CurrentUser.Reset();
    
    // Đóng form hiện tại, mở form đăng nhập
    var loginForm = new LoginForm();
    loginForm.Show();
    this.Close();
}
```

### 5.3 Phân Quyền Người Dùng
```csharp
public static class CurrentUser
{
    public static string MaNV { get; set; }
    public static string TenNV { get; set; }
    public static string ChucVu { get; set; }
    public static int Quyen { get; set; }
    
    // Kiểm tra quyền truy cập
    public static bool HasPermission(int requiredPermission)
    {
        return (Quyen & requiredPermission) == requiredPermission;
    }
    
    // Reset thông tin người dùng
    public static void Reset()
    {
        MaNV = null;
        TenNV = null;
        ChucVu = null;
        Quyen = 0;
    }
}
```

## 6. Quản Lý Hàng Hóa

### 6.1 Hiển Thị Danh Sách (HangHoaForm.cs)
```csharp
private void LoadDanhSachHangHoa()
{
    var query = @"
        SELECT 
            hh.MaHang,
            hh.TenHang,
            dvt.TenDVT,
            lh.TenLoaiHang,
            hh.SoLuongTon,
            hh.GiaNhap,
            hh.GiaXuat
        FROM Hang_Hoa hh
        JOIN Don_Vi_Tinh dvt ON hh.MaDVT = dvt.MaDVT
        JOIN Loai_Hang lh ON hh.MaLoaiHang = lh.MaLoaiHang
        ORDER BY hh.TenHang";

    // Hiển thị dữ liệu lên DataGridView
    using (var cmd = new SqlCommand(query, connection))
    {
        var dt = new DataTable();
        dt.Load(cmd.ExecuteReader());
        dgvHangHoa.DataSource = dt;
    }
    
    // Định dạng các cột
    dgvHangHoa.Columns["GiaNhap"].DefaultCellStyle.Format = "N0";
    dgvHangHoa.Columns["GiaXuat"].DefaultCellStyle.Format = "N0";
}
```

### 6.2 Thêm Hàng Hóa Mới
```csharp
private void BtnThem_Click(object sender, EventArgs e)
{
    // Kiểm tra quyền thêm hàng hóa
    if (!CurrentUser.HasPermission(Permissions.ThemHangHoa))
    {
        MessageBox.Show("Bạn không có quyền thêm hàng hóa!");
        return;
    }

    // Validate dữ liệu đầu vào
    if (string.IsNullOrEmpty(txtTenHang.Text))
    {
        MessageBox.Show("Vui lòng nhập tên hàng!");
        return;
    }

    var query = @"
        INSERT INTO Hang_Hoa (
            MaHang, TenHang, MaDVT, MaLoaiHang,
            SoLuongTon, GiaNhap, GiaXuat
        ) VALUES (
            @MaHang, @TenHang, @MaDVT, @MaLoaiHang,
            0, @GiaNhap, @GiaXuat
        )";

    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            // Tạo mã hàng tự động
            cmd.Parameters.AddWithValue("@MaHang", GenerateNextMaHang());
            cmd.Parameters.AddWithValue("@TenHang", txtTenHang.Text);
            cmd.Parameters.AddWithValue("@MaDVT", cbbDonViTinh.SelectedValue);
            cmd.Parameters.AddWithValue("@MaLoaiHang", cbbLoaiHang.SelectedValue);
            cmd.Parameters.AddWithValue("@GiaNhap", decimal.Parse(txtGiaNhap.Text));
            cmd.Parameters.AddWithValue("@GiaXuat", decimal.Parse(txtGiaXuat.Text));
            
            cmd.ExecuteNonQuery();
            MessageBox.Show("Thêm hàng hóa thành công!");
            LoadDanhSachHangHoa();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi: {ex.Message}");
    }
}
```

### 6.3 Cập Nhật Hàng Hóa
```csharp
private void BtnCapNhat_Click(object sender, EventArgs e)
{
    if (!CurrentUser.HasPermission(Permissions.SuaHangHoa))
    {
        MessageBox.Show("Bạn không có quyền sửa hàng hóa!");
        return;
    }

    if (string.IsNullOrEmpty(txtMaHang.Text))
    {
        MessageBox.Show("Vui lòng chọn hàng hóa cần sửa!");
        return;
    }

    // Kiểm tra trùng tên với hàng hóa khác
    var query = @"
        SELECT COUNT(*) 
        FROM Hang_Hoa 
        WHERE TenHang = @TenHang AND MaHang != @MaHang";

    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@TenHang", txtTenHang.Text);
        cmd.Parameters.AddWithValue("@MaHang", txtMaHang.Text);
        if ((int)cmd.ExecuteScalar() > 0)
        {
            MessageBox.Show("Hàng hóa này đã tồn tại!");
            return;
        }
    }

    // Cập nhật thông tin
    query = @"
        UPDATE Hang_Hoa 
        SET TenHang = @TenHang, MaDVT = @MaDVT, MaLoaiHang = @MaLoaiHang,
            GiaNhap = @GiaNhap, GiaXuat = @GiaXuat
        WHERE MaHang = @MaHang";

    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            cmd.Parameters.AddWithValue("@MaHang", txtMaHang.Text);
            cmd.Parameters.AddWithValue("@TenHang", txtTenHang.Text);
            cmd.Parameters.AddWithValue("@MaDVT", cbbDonViTinh.SelectedValue);
            cmd.Parameters.AddWithValue("@MaLoaiHang", cbbLoaiHang.SelectedValue);
            cmd.Parameters.AddWithValue("@GiaNhap", decimal.Parse(txtGiaNhap.Text));
            cmd.Parameters.AddWithValue("@GiaXuat", decimal.Parse(txtGiaXuat.Text));
            
            cmd.ExecuteNonQuery();
            MessageBox.Show("Cập nhật hàng hóa thành công!");
            LoadDanhSachHangHoa();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi: {ex.Message}");
    }
}
```

### 6.4 Xóa Hàng Hóa
```csharp
private void BtnXoa_Click(object sender, EventArgs e)
{
    if (!CurrentUser.HasPermission(Permissions.XoaHangHoa))
    {
        MessageBox.Show("Bạn không có quyền xóa hàng hóa!");
        return;
    }

    if (string.IsNullOrEmpty(txtMaHang.Text))
    {
        MessageBox.Show("Vui lòng chọn hàng hóa cần xóa!");
        return;
    }

    // Kiểm tra hàng hóa đã được sử dụng
    if (HasRelatedRecords(txtMaHang.Text))
    {
        MessageBox.Show("Không thể xóa hàng hóa đã có giao dịch!");
        return;
    }

    if (MessageBox.Show(
        "Bạn có chắc chắn muốn xóa hàng hóa này?",
        "Xác nhận xóa",
        MessageBoxButtons.YesNo) != DialogResult.Yes)
    {
        return;
    }

    var query = "DELETE FROM Hang_Hoa WHERE MaHang = @MaHang";
    
    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            cmd.Parameters.AddWithValue("@MaHang", txtMaHang.Text);
            cmd.ExecuteNonQuery();
            MessageBox.Show("Xóa hàng hóa thành công!");
            LoadDanhSachHangHoa();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi: {ex.Message}");
    }
}

// Kiểm tra hàng hóa đã được sử dụng
private bool HasRelatedRecords(string maHang)
{
    var query = @"
        SELECT COUNT(*)
        FROM (
            SELECT MaHang FROM Chi_Tiet_Phieu_Nhap
            WHERE MaHang = @MaHang
            UNION ALL
            SELECT MaHang FROM Chi_Tiet_Phieu_Xuat
            WHERE MaHang = @MaHang
        ) t";

    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@MaHang", maHang);
        return (int)cmd.ExecuteScalar() > 0;
    }
}
```

### 6.5 Tìm Kiếm Hàng Hóa
```csharp
private void TxtTimKiem_TextChanged(object sender, EventArgs e)
{
    var searchText = txtTimKiem.Text.Trim();
    if (string.IsNullOrEmpty(searchText))
    {
        LoadDanhSachHangHoa();
        return;
    }

    var query = @"
        SELECT 
            hh.MaHang,
            hh.TenHang,
            dvt.TenDVT,
            lh.TenLoaiHang,
            hh.SoLuongTon,
            hh.GiaNhap,
            hh.GiaXuat
        FROM Hang_Hoa hh
        JOIN Don_Vi_Tinh dvt ON hh.MaDVT = dvt.MaDVT
        JOIN Loai_Hang lh ON hh.MaLoaiHang = lh.MaLoaiHang
        WHERE 
            hh.MaHang LIKE @Search
            OR hh.TenHang LIKE @Search
            OR lh.TenLoaiHang LIKE @Search
        ORDER BY hh.TenHang";

    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@Search", $"%{searchText}%");
        var dt = new DataTable();
        dt.Load(cmd.ExecuteReader());
        dgvHangHoa.DataSource = dt;
    }
}
```

## 7. Giải Thích Chi Tiết

### 7.1 Đăng Nhập và Phân Quyền
- Sử dụng mã hóa SHA256 cho mật khẩu
- Lưu thông tin người dùng trong static class `CurrentUser`
- Kiểm tra quyền bằng phép toán bit (AND)

### 7.2 Quản Lý Hàng Hóa
- Sử dụng DataGridView để hiển thị danh sách
- Validate dữ liệu đầu vào kỹ lưỡng
- Kiểm tra quyền trước khi thực hiện thao tác
- Xử lý các trường hợp đặc biệt (xóa hàng đã có giao dịch)

### 7.3 Tối Ưu Hiệu Suất
- Sử dụng tham số hóa để tránh SQL Injection
- Cache danh sách đơn vị tính và loại hàng
- Chỉ load lại dữ liệu khi cần thiết

### 7.4 Xử Lý Lỗi
- Bắt tất cả các exception có thể xảy ra
- Hiển thị thông báo lỗi rõ ràng cho người dùng
- Rollback transaction khi cần thiết

## 8. Các Tính Năng Có Thể Phát Triển Thêm

1. **Bảo Mật**
   - Thêm captcha cho form đăng nhập
   - Giới hạn số lần đăng nhập sai
   - Ghi log các thao tác quan trọng

2. **Quản Lý Hàng Hóa**
   - Thêm ảnh sản phẩm
   - Quản lý theo serial/batch number
   - Cảnh báo hàng sắp hết

3. **Tìm Kiếm Nâng Cao**
   - Tìm kiếm theo nhiều tiêu chí
   - Lọc theo khoảng giá
   - Sắp xếp theo nhiều cột

4. **Giao Diện**
   - Thêm giao diện dark mode
   - Tùy chỉnh cột hiển thị
   - Xuất dữ liệu ra Excel

## 9. Quản Lý Đơn Vị Tính

### 9.1 Cấu Trúc Bảng Don_Vi_Tinh
```sql
CREATE TABLE Don_Vi_Tinh (
    MaDVT varchar(10) PRIMARY KEY,
    TenDVT nvarchar(50) NOT NULL,
    GhiChu nvarchar(200)
)
```

### 9.2 Hiển Thị Danh Sách (DonViTinhForm.cs)
```csharp
private void LoadDanhSachDVT()
{
    var query = @"
        SELECT MaDVT, TenDVT, GhiChu
        FROM Don_Vi_Tinh
        ORDER BY TenDVT";

    using (var cmd = new SqlCommand(query, connection))
    {
        var dt = new DataTable();
        dt.Load(cmd.ExecuteReader());
        dgvDonViTinh.DataSource = dt;
        
        // Định dạng lại tên cột
        dgvDonViTinh.Columns["MaDVT"].HeaderText = "Mã ĐVT";
        dgvDonViTinh.Columns["TenDVT"].HeaderText = "Tên Đơn Vị Tính";
        dgvDonViTinh.Columns["GhiChu"].HeaderText = "Ghi Chú";
    }
}
```

### 9.3 Thêm Đơn Vị Tính
```csharp
private void BtnThem_Click(object sender, EventArgs e)
{
    // Kiểm tra quyền
    if (!CurrentUser.HasPermission(Permissions.ThemDVT))
    {
        MessageBox.Show("Bạn không có quyền thêm đơn vị tính!");
        return;
    }

    // Validate dữ liệu
    if (string.IsNullOrEmpty(txtTenDVT.Text))
    {
        MessageBox.Show("Vui lòng nhập tên đơn vị tính!");
        return;
    }

    // Kiểm tra trùng tên
    if (IsDVTExists(txtTenDVT.Text))
    {
        MessageBox.Show("Đơn vị tính này đã tồn tại!");
        return;
    }

    var query = @"
        INSERT INTO Don_Vi_Tinh (MaDVT, TenDVT, GhiChu)
        VALUES (@MaDVT, @TenDVT, @GhiChu)";

    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            // Tạo mã tự động
            string maDVT = GenerateNextMaDVT();
            
            cmd.Parameters.AddWithValue("@MaDVT", maDVT);
            cmd.Parameters.AddWithValue("@TenDVT", txtTenDVT.Text.Trim());
            cmd.Parameters.AddWithValue("@GhiChu", 
                string.IsNullOrEmpty(txtGhiChu.Text) ? DBNull.Value : (object)txtGhiChu.Text);

            cmd.ExecuteNonQuery();
            MessageBox.Show("Thêm đơn vị tính thành công!");
            LoadDanhSachDVT();
            ResetForm();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi: {ex.Message}");
    }
}

// Kiểm tra đơn vị tính đã tồn tại
private bool IsDVTExists(string tenDVT)
{
    var query = "SELECT COUNT(*) FROM Don_Vi_Tinh WHERE TenDVT = @TenDVT";
    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@TenDVT", tenDVT.Trim());
        return (int)cmd.ExecuteScalar() > 0;
    }
}

// Tạo mã đơn vị tính tự động
private string GenerateNextMaDVT()
{
    var query = @"
        SELECT TOP 1 MaDVT 
        FROM Don_Vi_Tinh 
        ORDER BY MaDVT DESC";

    using (var cmd = new SqlCommand(query, connection))
    {
        var lastMaDVT = cmd.ExecuteScalar()?.ToString() ?? "DVT000";
        var number = int.Parse(lastMaDVT.Substring(3)) + 1;
        return $"DVT{number:D3}";
    }
}
```

### 9.4 Cập Nhật Đơn Vị Tính
```csharp
private void BtnCapNhat_Click(object sender, EventArgs e)
{
    if (!CurrentUser.HasPermission(Permissions.SuaDVT))
    {
        MessageBox.Show("Bạn không có quyền sửa đơn vị tính!");
        return;
    }

    if (string.IsNullOrEmpty(txtMaDVT.Text))
    {
        MessageBox.Show("Vui lòng chọn đơn vị tính cần sửa!");
        return;
    }

    // Kiểm tra trùng tên với đơn vị tính khác
    var query = @"
        SELECT COUNT(*) 
        FROM Don_Vi_Tinh 
        WHERE TenDVT = @TenDVT AND MaDVT != @MaDVT";

    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@TenDVT", txtTenDVT.Text.Trim());
        cmd.Parameters.AddWithValue("@MaDVT", txtMaDVT.Text);
        if ((int)cmd.ExecuteScalar() > 0)
        {
            MessageBox.Show("Đơn vị tính này đã tồn tại!");
            return;
        }
    }

    // Cập nhật thông tin
    query = @"
        UPDATE Don_Vi_Tinh 
        SET TenDVT = @TenDVT, GhiChu = @GhiChu
        WHERE MaDVT = @MaDVT";

    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            cmd.Parameters.AddWithValue("@MaDVT", txtMaDVT.Text);
            cmd.Parameters.AddWithValue("@TenDVT", txtTenDVT.Text.Trim());
            cmd.Parameters.AddWithValue("@GhiChu", 
                string.IsNullOrEmpty(txtGhiChu.Text) ? DBNull.Value : (object)txtGhiChu.Text);

            cmd.ExecuteNonQuery();
            MessageBox.Show("Cập nhật đơn vị tính thành công!");
            LoadDanhSachDVT();
            ResetForm();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi: {ex.Message}");
    }
}
```

### 9.5 Xóa Đơn Vị Tính
```csharp
private void BtnXoa_Click(object sender, EventArgs e)
{
    if (!CurrentUser.HasPermission(Permissions.XoaDVT))
    {
        MessageBox.Show("Bạn không có quyền xóa đơn vị tính!");
        return;
    }

    if (string.IsNullOrEmpty(txtMaDVT.Text))
    {
        MessageBox.Show("Vui lòng chọn đơn vị tính cần xóa!");
        return;
    }

    // Kiểm tra đơn vị tính đã được sử dụng
    if (IsDVTInUse(txtMaDVT.Text))
    {
        MessageBox.Show("Không thể xóa đơn vị tính đã được sử dụng!");
        return;
    }

    if (MessageBox.Show(
        "Bạn có chắc chắn muốn xóa đơn vị tính này?",
        "Xác nhận xóa",
        MessageBoxButtons.YesNo) != DialogResult.Yes)
    {
        return;
    }

    var query = "DELETE FROM Don_Vi_Tinh WHERE MaDVT = @MaDVT";
    
    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            cmd.Parameters.AddWithValue("@MaDVT", txtMaDVT.Text);
            cmd.ExecuteNonQuery();
            MessageBox.Show("Xóa đơn vị tính thành công!");
            LoadDanhSachDVT();
            ResetForm();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi: {ex.Message}");
    }
}

// Kiểm tra đơn vị tính đã được sử dụng
private bool IsDVTInUse(string maDVT)
{
    var query = "SELECT COUNT(*) FROM Hang_Hoa WHERE MaDVT = @MaDVT";
    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@MaDVT", maDVT);
        return (int)cmd.ExecuteScalar() > 0;
    }
}
```

### 9.6 Tìm Kiếm Đơn Vị Tính
```csharp
private void TxtTimKiem_TextChanged(object sender, EventArgs e)
{
    var searchText = txtTimKiem.Text.Trim();
    if (string.IsNullOrEmpty(searchText))
    {
        LoadDanhSachDVT();
        return;
    }

    var query = @"
        SELECT MaDVT, TenDVT, GhiChu
        FROM Don_Vi_Tinh
        WHERE 
            MaDVT LIKE @Search
            OR TenDVT LIKE @Search
            OR GhiChu LIKE @Search
        ORDER BY TenDVT";

    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@Search", $"%{searchText}%");
        var dt = new DataTable();
        dt.Load(cmd.ExecuteReader());
        dgvDonViTinh.DataSource = dt;
    }
}
```

### 9.7 Giải Thích Chi Tiết

1. **Cấu Trúc Dữ Liệu**
   - Mã đơn vị tính tự động tăng (DVT001, DVT002,...)
   - Tên đơn vị tính không được trùng
   - Ghi chú có thể để trống

2. **Xử Lý Nghiệp Vụ**
   - Kiểm tra quyền người dùng trước mỗi thao tác
   - Validate dữ liệu đầu vào kỹ lưỡng
   - Không cho phép xóa đơn vị tính đã được sử dụng
   - Tự động tạo mã đơn vị tính mới

3. **Tối Ưu Hiệu Suất**
   - Sử dụng tham số hóa trong câu truy vấn
   - Cache danh sách đơn vị tính
   - Chỉ load lại dữ liệu khi cần thiết

4. **Giao Diện Người Dùng**
   - Hiển thị danh sách dạng lưới
   - Tìm kiếm theo nhiều tiêu chí
   - Thông báo lỗi rõ ràng
   - Form nhập liệu đơn giản, dễ sử dụng

### 9.8 Các Tính Năng Có Thể Phát Triển Thêm

1. **Nhóm Đơn Vị Tính**
   - Phân loại đơn vị tính theo nhóm
   - Quản lý quy đổi giữa các đơn vị

2. **Tùy Chỉnh Hiển Thị**
   - Cho phép ẩn/hiện cột
   - Lưu cấu hình hiển thị

3. **Xuất/Nhập Dữ Liệu**
   - Xuất danh sách ra Excel
   - Nhập từ file Excel

## 10. Quản Lý Khách Hàng

### 10.1 Cấu Trúc Bảng Khach_Hang
```sql
CREATE TABLE Khach_Hang (
    MaKH varchar(10) PRIMARY KEY,
    TenKH nvarchar(100) NOT NULL,
    DiaChi nvarchar(200),
    DienThoai varchar(20),
    Email varchar(100),
    GhiChu nvarchar(200),
    NgayTao datetime DEFAULT GETDATE(),
    NgayCapNhat datetime
)
```

### 10.2 Hiển Thị Danh Sách (KhachHangForm.cs)
```csharp
private void LoadDanhSachKhachHang()
{
    var query = @"
        SELECT 
            MaKH as 'Mã KH',
            TenKH as 'Tên khách hàng',
            DiaChi as 'Địa chỉ',
            DienThoai as 'Điện thoại',
            Email,
            GhiChu as 'Ghi chú',
            NgayTao as 'Ngày tạo',
            NgayCapNhat as 'Ngày cập nhật'
        FROM Khach_Hang
        ORDER BY TenKH";

    using (var cmd = new SqlCommand(query, connection))
    {
        var dt = new DataTable();
        dt.Load(cmd.ExecuteReader());
        dgvKhachHang.DataSource = dt;
        
        // Định dạng ngày tháng
        dgvKhachHang.Columns["Ngày tạo"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        dgvKhachHang.Columns["Ngày cập nhật"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
    }
}
```

### 10.3 Thêm Khách Hàng
```csharp
private void BtnThem_Click(object sender, EventArgs e)
{
    // Kiểm tra quyền
    if (!CurrentUser.HasPermission(Permissions.ThemKhachHang))
    {
        MessageBox.Show("Bạn không có quyền thêm khách hàng!");
        return;
    }

    // Validate dữ liệu
    if (string.IsNullOrEmpty(txtTenKH.Text))
    {
        MessageBox.Show("Vui lòng nhập tên khách hàng!");
        return;
    }

    // Validate số điện thoại
    if (!string.IsNullOrEmpty(txtDienThoai.Text) && !IsValidPhoneNumber(txtDienThoai.Text))
    {
        MessageBox.Show("Số điện thoại không hợp lệ!");
        return;
    }

    // Validate email
    if (!string.IsNullOrEmpty(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
    {
        MessageBox.Show("Email không hợp lệ!");
        return;
    }

    var query = @"
        INSERT INTO Khach_Hang (
            MaKH, TenKH, DiaChi, DienThoai,
            Email, GhiChu, NgayTao
        ) VALUES (
            @MaKH, @TenKH, @DiaChi, @DienThoai,
            @Email, @GhiChu, GETDATE()
        )";

    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            // Tạo mã khách hàng tự động
            string maKH = GenerateNextMaKH();
            
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

// Validate số điện thoại
private bool IsValidPhoneNumber(string phoneNumber)
{
    // Chấp nhận số điện thoại 10-11 số, bắt đầu bằng 0
    return Regex.IsMatch(phoneNumber, @"^0\d{9,10}$");
}

// Validate email
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

// Tạo mã khách hàng tự động
private string GenerateNextMaKH()
{
    var query = @"
        SELECT TOP 1 MaKH 
        FROM Khach_Hang 
        ORDER BY MaKH DESC";

    using (var cmd = new SqlCommand(query, connection))
    {
        var lastMaKH = cmd.ExecuteScalar()?.ToString() ?? "KH0000";
        var number = int.Parse(lastMaKH.Substring(2)) + 1;
        return $"KH{number:D4}";
    }
}
```

### 10.4 Cập Nhật Khách Hàng
```csharp
private void BtnCapNhat_Click(object sender, EventArgs e)
{
    if (!CurrentUser.HasPermission(Permissions.SuaKhachHang))
    {
        MessageBox.Show("Bạn không có quyền sửa thông tin khách hàng!");
        return;
    }

    if (string.IsNullOrEmpty(txtMaKH.Text))
    {
        MessageBox.Show("Vui lòng chọn khách hàng cần sửa!");
        return;
    }

    // Validate như phần thêm mới
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
        UPDATE Khach_Hang 
        SET 
            TenKH = @TenKH,
            DiaChi = @DiaChi,
            DienThoai = @DienThoai,
            Email = @Email,
            GhiChu = @GhiChu,
            NgayCapNhat = GETDATE()
        WHERE MaKH = @MaKH";

    try
    {
        using (var cmd = new SqlCommand(query, connection))
        {
            cmd.Parameters.AddWithValue("@MaKH", txtMaKH.Text);
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
```

### 10.5 Xóa Khách Hàng
```csharp
private void BtnXoa_Click(object sender, EventArgs e)
{
    if (!CurrentUser.HasPermission(Permissions.XoaKhachHang))
    {
        MessageBox.Show("Bạn không có quyền xóa khách hàng!");
        return;
    }

    if (string.IsNullOrEmpty(txtMaKH.Text))
    {
        MessageBox.Show("Vui lòng chọn khách hàng cần xóa!");
        return;
    }

    // Kiểm tra khách hàng đã có giao dịch
    if (HasTransactions(txtMaKH.Text))
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
            cmd.Parameters.AddWithValue("@MaKH", txtMaKH.Text);
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

// Kiểm tra khách hàng đã có giao dịch
private bool HasTransactions(string maKH)
{
    var query = @"
        SELECT COUNT(*) 
        FROM Phieu_Xuat 
        WHERE MaKH = @MaKH";

    using (var cmd = new SqlCommand(query, connection))
    {
        cmd.Parameters.AddWithValue("@MaKH", maKH);
        return (int)cmd.ExecuteScalar() > 0;
    }
}
```

### 10.6 Tìm Kiếm Khách Hàng
```csharp
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
            MaKH as 'Mã KH',
            TenKH as 'Tên khách hàng',
            DiaChi as 'Địa chỉ',
            DienThoai as 'Điện thoại',
            Email,
            GhiChu as 'Ghi chú',
            NgayTao as 'Ngày tạo',
            NgayCapNhat as 'Ngày cập nhật'
        FROM Khach_Hang
        WHERE 
            MaKH LIKE @Search
            OR TenKH LIKE @Search
            OR DienThoai LIKE @Search
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
```

### 10.7 Giải Thích Chi Tiết

1. **Cấu Trúc Dữ Liệu**
   - Mã khách hàng tự động tăng (KH0001, KH0002,...)
   - Lưu thời gian tạo và cập nhật
   - Email và số điện thoại có validate

2. **Xử Lý Nghiệp Vụ**
   - Kiểm tra quyền người dùng
   - Validate dữ liệu đầu vào
   - Không cho phép xóa khách hàng đã có giao dịch
   - Tự động tạo mã khách hàng mới

3. **Tối Ưu Hiệu Suất**
   - Sử dụng tham số hóa trong SQL
   - Index cho các trường tìm kiếm
   - Phân trang khi cần thiết

4. **Giao Diện Người Dùng**
   - Hiển thị dạng lưới với các cột chính
   - Tìm kiếm nhanh theo nhiều tiêu chí
   - Form nhập liệu với validate
   - Thông báo lỗi rõ ràng

### 10.8 Các Tính Năng Có Thể Phát Triển Thêm

1. **Phân Loại Khách Hàng**
   - Thêm nhóm khách hàng
   - Chính sách giá theo nhóm
   - Thống kê theo nhóm

2. **Lịch Sử Giao Dịch**
   - Xem lịch sử mua hàng
   - Thống kê doanh số
   - Công nợ khách hàng

3. **Tiện Ích**
   - Gửi email thông báo
   - Xuất danh sách Excel
   - In phiếu thông tin

4. **Tương Tác**
   - Ghi chú theo dõi
   - Lịch hẹn gặp
   - Nhắc nhở công nợ