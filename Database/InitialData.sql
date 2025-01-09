-- Thêm nhân viên admin mặc định
IF NOT EXISTS (SELECT 1 FROM [dbo].[TT_Nhan_Vien] WHERE MaNV = 1)
BEGIN
    INSERT INTO [dbo].[TT_Nhan_Vien] (MaNV, TenNV, GT, NgaySinh, DiaChi, SDT, Gmail)
    VALUES (1, N'Admin', 'M', '1990-01-01', N'Hà Nội', '0123456789', 'admin@gmail.com');
END

-- Thêm tài khoản admin mặc định (mật khẩu: admin123)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Tai_Khoan] WHERE TK = 'admin')
BEGIN
    INSERT INTO [dbo].[Tai_Khoan] (TK, MaNV, MatKhau)
    VALUES ('admin', 1, 'e5d6845dc6690465368d30e4a7c646e3c2a944fb3434d111db3d1d5d74e4a09e');
END

-- Thêm quyền mặc định nếu chưa có
IF NOT EXISTS (SELECT 1 FROM [dbo].[Quyen_Han] WHERE MaQuyen = 1)
BEGIN
    INSERT INTO [dbo].[Quyen_Han] (MaQuyen, TenQuyen, MoTa)
    VALUES 
        (1, 'ADMIN', N'System Administration - Full rights'),
        (2, 'MANAGER', N'Management - View reports and manage employees'),
        (3, 'STAFF', N'Staff - Import and export warehouse'),
        (4, 'VIEWER', N'Viewer - View report only');
END

-- Cấp quyền ADMIN cho tài khoản admin
IF NOT EXISTS (SELECT 1 FROM [dbo].[Phan_Quyen] WHERE MaNV = 1 AND MaQuyen = 1)
BEGIN
    INSERT INTO [dbo].[Phan_Quyen] (MaNV, MaQuyen)
    VALUES (1, 1);
END
