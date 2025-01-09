USE [QuanLyKho]
GO
/****** Object:  Table [dbo].[Chi_Tiet_Phieu_Nhap]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Chi_Tiet_Phieu_Nhap](
	[MaPhieu] [varchar](10) NOT NULL,
	[MaHang] [varchar](10) NOT NULL,
	[SoLuong] [int] NULL,
	[DonGia] [decimal](18, 2) NULL,
	[ThanhTien] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaPhieu] ASC,
	[MaHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Chi_Tiet_Phieu_Xuat]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Chi_Tiet_Phieu_Xuat](
	[MaPhieu] [varchar](10) NOT NULL,
	[MaHang] [varchar](10) NOT NULL,
	[SoLuong] [int] NULL,
	[DonGia] [decimal](18, 2) NULL,
	[ThanhTien] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaPhieu] ASC,
	[MaHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Don_Vi_Tinh]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Don_Vi_Tinh](
	[MaDVT] [int] IDENTITY(1,1) NOT NULL,
	[TenDVT] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaDVT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Hang_Hoa]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hang_Hoa](
	[MaHang] [varchar](10) NOT NULL,
	[TenHang] [nvarchar](100) NOT NULL,
	[MaDVT] [int] NULL,
	[MaNCC] [varchar](10) NULL,
	[GiaNhap] [decimal](18, 2) NULL,
	[GiaXuat] [decimal](18, 2) NULL,
	[SoLuongTon] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[MaHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Khach_Hang]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Khach_Hang](
	[MaKH] [int] NOT NULL,
	[TenKH] [varchar](100) NULL,
	[DiaChi] [varchar](255) NULL,
	[SDT] [varchar](15) NULL,
	[Email] [varchar](100) NULL,
	[GhiChu] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[MaKH] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Nha_Cung_Cap]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Nha_Cung_Cap](
	[MaNCC] [varchar](10) NOT NULL,
	[TenNCC] [nvarchar](100) NOT NULL,
	[DiaChi] [nvarchar](200) NULL,
	[SDT] [varchar](20) NULL,
	[Email] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaNCC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Phan_Quyen]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Phan_Quyen](
	[MaNV] [int] NOT NULL,
	[MaQuyen] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaNV] ASC,
	[MaQuyen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Phieu_Nhap]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Phieu_Nhap](
	[MaPhieu] [varchar](10) NOT NULL,
	[NgayNhap] [datetime] NULL,
	[MaNCC] [varchar](10) NULL,
	[MaNV] [int] NULL,
	[GhiChu] [nvarchar](200) NULL,
	[TongTien] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaPhieu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Phieu_Xuat]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Phieu_Xuat](
	[MaPhieu] [varchar](10) NOT NULL,
	[NgayXuat] [datetime] NULL,
	[MaNV] [int] NULL,
	[GhiChu] [nvarchar](200) NULL,
	[TongTien] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaPhieu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Quyen_Han]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Quyen_Han](
	[MaQuyen] [int] IDENTITY(1,1) NOT NULL,
	[TenQuyen] [varchar](50) NOT NULL,
	[MoTa] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaQuyen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tai_Khoan]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tai_Khoan](
	[TK] [varchar](50) NOT NULL,
	[MaNV] [int] NOT NULL,
	[MatKhau] [varchar](64) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[MaNV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TT_Nhan_Vien]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TT_Nhan_Vien](
	[MaNV] [int] IDENTITY(1,1) NOT NULL,
	[TenNV] [nvarchar](100) NOT NULL,
	[GT] [char](1) NULL,
	[NgaySinh] [date] NULL,
	[DiaChi] [nvarchar](200) NULL,
	[SDT] [varchar](20) NULL,
	[Gmail] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaNV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VatTu]    Script Date: 2025-01-09 9:11:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VatTu](
	[MaVatTu] [int] IDENTITY(1,1) NOT NULL,
	[TenVT] [nvarchar](100) NULL,
	[MaDVT] [int] NULL,
	[MaNCC] [int] NULL,
	[GhiChu] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[MaVatTu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Nhap] ADD  DEFAULT ((0)) FOR [SoLuong]
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Nhap] ADD  DEFAULT ((0)) FOR [DonGia]
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Nhap] ADD  DEFAULT ((0)) FOR [ThanhTien]
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Xuat] ADD  DEFAULT ((0)) FOR [SoLuong]
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Xuat] ADD  DEFAULT ((0)) FOR [DonGia]
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Xuat] ADD  DEFAULT ((0)) FOR [ThanhTien]
GO
ALTER TABLE [dbo].[Hang_Hoa] ADD  DEFAULT ((0)) FOR [GiaNhap]
GO
ALTER TABLE [dbo].[Hang_Hoa] ADD  DEFAULT ((0)) FOR [GiaXuat]
GO
ALTER TABLE [dbo].[Hang_Hoa] ADD  DEFAULT ((0)) FOR [SoLuongTon]
GO
ALTER TABLE [dbo].[Phieu_Nhap] ADD  DEFAULT (getdate()) FOR [NgayNhap]
GO
ALTER TABLE [dbo].[Phieu_Nhap] ADD  DEFAULT ((0)) FOR [TongTien]
GO
ALTER TABLE [dbo].[Phieu_Xuat] ADD  DEFAULT (getdate()) FOR [NgayXuat]
GO
ALTER TABLE [dbo].[Phieu_Xuat] ADD  DEFAULT ((0)) FOR [TongTien]
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Nhap]  WITH CHECK ADD FOREIGN KEY([MaHang])
REFERENCES [dbo].[Hang_Hoa] ([MaHang])
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Nhap]  WITH CHECK ADD FOREIGN KEY([MaPhieu])
REFERENCES [dbo].[Phieu_Nhap] ([MaPhieu])
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Xuat]  WITH CHECK ADD FOREIGN KEY([MaHang])
REFERENCES [dbo].[Hang_Hoa] ([MaHang])
GO
ALTER TABLE [dbo].[Chi_Tiet_Phieu_Xuat]  WITH CHECK ADD FOREIGN KEY([MaPhieu])
REFERENCES [dbo].[Phieu_Xuat] ([MaPhieu])
GO
ALTER TABLE [dbo].[Hang_Hoa]  WITH CHECK ADD FOREIGN KEY([MaDVT])
REFERENCES [dbo].[Don_Vi_Tinh] ([MaDVT])
GO
ALTER TABLE [dbo].[Hang_Hoa]  WITH CHECK ADD FOREIGN KEY([MaNCC])
REFERENCES [dbo].[Nha_Cung_Cap] ([MaNCC])
GO
ALTER TABLE [dbo].[Phan_Quyen]  WITH CHECK ADD FOREIGN KEY([MaQuyen])
REFERENCES [dbo].[Quyen_Han] ([MaQuyen])
GO
ALTER TABLE [dbo].[Phan_Quyen]  WITH CHECK ADD FOREIGN KEY([MaNV])
REFERENCES [dbo].[TT_Nhan_Vien] ([MaNV])
GO
ALTER TABLE [dbo].[Phieu_Nhap]  WITH CHECK ADD FOREIGN KEY([MaNCC])
REFERENCES [dbo].[Nha_Cung_Cap] ([MaNCC])
GO
ALTER TABLE [dbo].[Phieu_Nhap]  WITH CHECK ADD FOREIGN KEY([MaNV])
REFERENCES [dbo].[TT_Nhan_Vien] ([MaNV])
GO
ALTER TABLE [dbo].[Phieu_Xuat]  WITH CHECK ADD FOREIGN KEY([MaNV])
REFERENCES [dbo].[TT_Nhan_Vien] ([MaNV])
GO
ALTER TABLE [dbo].[Tai_Khoan]  WITH CHECK ADD FOREIGN KEY([MaNV])
REFERENCES [dbo].[TT_Nhan_Vien] ([MaNV])
GO
ALTER TABLE [dbo].[TT_Nhan_Vien]  WITH CHECK ADD CHECK  (([GT]='F' OR [GT]='M'))
GO
