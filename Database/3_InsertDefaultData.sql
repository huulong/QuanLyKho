-- Thêm nhân viên admin
INSERT INTO [dbo].[TT_Nhan_Vien] ([TenNV], [GT], [NgaySinh], [DiaChi], [SDT], [Gmail])
VALUES (N'Administrator', 'M', '2000-01-01', N'Admin Address', '0123456789', 'admin@example.com');

-- Thêm quyền hạn
INSERT INTO [dbo].[Quyen_Han] ([TenQuyen], [MoTa])
VALUES 
('ADMIN', N'Quản trị hệ thống'),
('USER', N'Người dùng thông thường');

-- Thêm tài khoản admin với mật khẩu là "admin" (đã hash bằng SHA256)
-- Mật khẩu "admin" sau khi hash SHA256: 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
INSERT INTO [dbo].[Tai_Khoan] ([TK], [MaNV], [MatKhau])
VALUES ('admin', 1, '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');

-- Phân quyền admin
INSERT INTO [dbo].[Phan_Quyen] ([MaNV], [MaQuyen])
VALUES (1, 1); -- Admin có quyền ADMIN

-- Thêm một số đơn vị tính mặc định
INSERT INTO [dbo].[Don_Vi_Tinh] ([TenDVT])
VALUES 
(N'Cái'),
(N'Chiếc'),
(N'Hộp'),
(N'Thùng'),
(N'Kg'),
(N'Gam'),
(N'Lít'),
(N'Mét');
