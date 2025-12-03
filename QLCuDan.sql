-- 1. Tạo Database mới (Nếu có rồi thì xóa đi tạo lại cho sạch)
USE master;
GO
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLyChungCuDB')
BEGIN
    ALTER DATABASE QuanLyChungCuDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyChungCuDB;
END
GO
CREATE DATABASE QuanLyChungCuDB;
GO
USE QuanLyChungCuDB;
GO

-- 2. Tạo bảng VaiTro (Admin/User)
CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL
);
GO

-- 3. Tạo bảng NguoiDung (Để đăng nhập)
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoan VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(100) NOT NULL,
    HoTen NVARCHAR(100),
    Email VARCHAR(100),
    MaVaiTro INT NOT NULL,
    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);
GO

-- 4. Tạo bảng ToaNha
CREATE TABLE ToaNha (
    MaToaNha INT IDENTITY(1,1) PRIMARY KEY,
    TenToaNha NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(200)
);
GO

-- 5. Tạo bảng CanHo
CREATE TABLE CanHo (
    MaCanHo INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong VARCHAR(20) NOT NULL,
    DienTich FLOAT,
    Tang INT,
    TrangThai NVARCHAR(50), -- 'Da O', 'Trong'
    MaToaNha INT NOT NULL,
    FOREIGN KEY (MaToaNha) REFERENCES ToaNha(MaToaNha)
);
GO

-- 6. Tạo bảng HoGiaDinh
CREATE TABLE HoGiaDinh (
    MaHo INT IDENTITY(1,1) PRIMARY KEY,
    NgayNhanNha DATE,
    SoThanhVien INT DEFAULT 0,
    MaCanHo INT,
    FOREIGN KEY (MaCanHo) REFERENCES CanHo(MaCanHo)
);
GO

-- 7. Tạo bảng CuDan (Quan trọng: Đầy đủ trường theo ảnh bạn gửi)
CREATE TABLE CuDan (
    MaCuDan INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    SDT VARCHAR(20),
    Email VARCHAR(100),
    Avatar VARCHAR(255),
    
    -- Các trường đặc thù theo ảnh
    TrinhDoHocVan NVARCHAR(100),
    NgayVaoDang DATE,
    NgayVaoDoan DATE,
    HocHamHocVi NVARCHAR(100),
    NhanDang_Cao NVARCHAR(50),
    NhanDang_SongMui NVARCHAR(50),
    DauVetDacBiet NVARCHAR(200),
    
    -- Khóa ngoại
    MaHo INT,
    FOREIGN KEY (MaHo) REFERENCES HoGiaDinh(MaHo)
);
GO

-- 8. Tạo bảng PhanAnh
CREATE TABLE PhanAnh (
    MaPhanAnh INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe NVARCHAR(200),
    NoiDung NVARCHAR(MAX),
    NgayGui DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50),
    MaCuDan INT,
    FOREIGN KEY (MaCuDan) REFERENCES CuDan(MaCuDan)
);
GO

-- =============================================
-- BƯỚC 3: THÊM DỮ LIỆU MẪU (ĐỂ TEST ĐƯỢC NGAY)
-- =============================================

-- Thêm Vai Trò
INSERT INTO VaiTro (TenVaiTro) VALUES (N'Admin'), (N'Cư Dân');

-- Thêm Tài khoản Admin (User: admin / Pass: 123456)
INSERT INTO NguoiDung (TaiKhoan, MatKhau, HoTen, Email, MaVaiTro) 
VALUES ('admin', '123456', N'Quản Trị Viên', 'admin@gmail.com', 1);

-- Thêm Tòa Nhà
INSERT INTO ToaNha (TenToaNha, DiaChi) VALUES (N'Tòa A', N'123 Đường Láng');

-- Thêm Căn Hộ
INSERT INTO CanHo (SoPhong, DienTich, Tang, TrangThai, MaToaNha) 
VALUES ('P101', 80.5, 1, N'Đã ở', 1);

-- Thêm Hộ Gia Đình (Ở căn P101)
INSERT INTO HoGiaDinh (NgayNhanNha, SoThanhVien, MaCanHo) 
VALUES ('2023-01-01', 4, 1);

-- Thêm Cư Dân Mẫu (Giống ảnh bạn gửi)
INSERT INTO CuDan (HoTen, SDT, Email, Avatar, TrinhDoHocVan, NhanDang_Cao, NhanDang_SongMui, DauVetDacBiet, MaHo) 
VALUES 
(N'Admin', '0928817228', 'admin@gmail.com', '/Content/Images/avatar.jpg', 
 N'Văn hóa phổ thông', N'Cao', N'Thẳng', N'Nốt ruồi đuôi mắt trái', 1);
GO