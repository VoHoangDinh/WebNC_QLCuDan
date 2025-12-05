-- 1. Tạo Database mới (Xóa cũ tạo mới cho sạch sẽ)
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

-- 2. Tạo bảng VaiTro
CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL
);
GO

-- 3. Tạo bảng NguoiDung
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
    TrangThai NVARCHAR(50),
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

-- 7. Tạo bảng CuDan (ĐÃ SỬA: Thêm cột QuanHeVoiChuHo)
CREATE TABLE CuDan (
    MaCuDan INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    SDT VARCHAR(20),
    Email VARCHAR(100),
    Avatar VARCHAR(255),
    
    -- Các trường đặc thù
    TrinhDoHocVan NVARCHAR(100),
    NgayVaoDang DATE,
    NgayVaoDoan DATE,
    HocHamHocVi NVARCHAR(100),
    NhanDang_Cao NVARCHAR(50),
    NhanDang_SongMui NVARCHAR(50),
    DauVetDacBiet NVARCHAR(200),
    
    -- QUAN TRỌNG: Thêm cột này để khớp với lệnh INSERT bên dưới
    QuanHeVoiChuHo NVARCHAR(50), 
    
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
-- BƯỚC 3: THÊM DỮ LIỆU MẪU
-- =============================================
USE QuanLyChungCuDB;
GO

INSERT INTO VaiTro (TenVaiTro) VALUES (N'Admin'), (N'Cư Dân');
INSERT INTO NguoiDung (TaiKhoan, MatKhau, HoTen, Email, MaVaiTro) VALUES ('admin', '123456', N'Quản Trị Viên', 'admin@gmail.com', 1);

INSERT INTO ToaNha (TenToaNha, DiaChi) VALUES (N'Tòa A', N'123 Đường Láng'), (N'Tòa B', N'456 Nguyễn Trãi');

-- Thêm Căn Hộ
INSERT INTO CanHo (SoPhong, DienTich, Tang, TrangThai, MaToaNha) VALUES 
('P101', 80.5, 1, N'Đã ở', 1),
('P102', 75.0, 1, N'Trống', 1),
('P205', 90.0, 2, N'Đã ở', 1),
('B101', 110.0, 1, N'Đã ở', 2);

-- Thêm Hộ Gia Đình
INSERT INTO HoGiaDinh (NgayNhanNha, SoThanhVien, MaCanHo) VALUES 
('2023-01-01', 4, 1), -- Hộ 1
('2023-05-15', 3, 3), -- Hộ 2
('2023-06-20', 2, 4); -- Hộ 3

-- Thêm Cư Dân (Hộ 1)
INSERT INTO CuDan (HoTen, SDT, Email, Avatar, TrinhDoHocVan, NhanDang_Cao, NhanDang_SongMui, DauVetDacBiet, QuanHeVoiChuHo, MaHo) 
VALUES (N'Admin', '0928817228', 'admin@gmail.com', '/Content/Images/avatar.jpg', N'Văn hóa phổ thông', N'Cao', N'Thẳng', N'Nốt ruồi đuôi mắt trái', N'Chủ hộ', 1);

INSERT INTO CuDan (HoTen, SDT, Email, Avatar, TrinhDoHocVan, NgaySinh, GioiTinh, NhanDang_Cao, NhanDang_SongMui, DauVetDacBiet, QuanHeVoiChuHo, MaHo) 
VALUES 
(N'Trần Thị Mai', '0912345678', 'mai.tran@gmail.com', '/Content/Images/default.jpg', N'Đại học Sư phạm', '1995-02-14', N'Nữ', N'1m60', N'Cao', N'Sẹo nhỏ ở tay trái', N'Vợ', 1),
(N'Nguyễn Văn Tí', '', '', '/Content/Images/default.jpg', N'Mầm non', '2018-09-01', N'Nam', N'1m10', N'Tẹt', N'Bớt xanh ở mông', N'Con', 1);

-- Thêm Cư Dân (Hộ 2)
INSERT INTO CuDan (HoTen, SDT, Email, Avatar, TrinhDoHocVan, NgaySinh, GioiTinh, NhanDang_Cao, NhanDang_SongMui, DauVetDacBiet, QuanHeVoiChuHo, MaHo) 
VALUES 
(N'Lê Văn Hùng', '0988777666', 'hung.le@company.com', '/Content/Images/default.jpg', N'Tiến sĩ Kinh tế', '1980-05-20', N'Nam', N'1m75', N'Thẳng', N'Nốt ruồi trên mép phải', N'Chủ hộ', 2),
(N'Phạm Thu Cúc', '0977888999', 'cuc.pham@shop.com', '/Content/Images/default.jpg', N'Cử nhân Kế toán', '1982-11-10', N'Nữ', N'1m58', N'Thấp', N'Không có', N'Vợ', 2),
(N'Lê Tuấn Kiệt', '0334445555', 'kiet.le@student.com', '/Content/Images/default.jpg', N'Sinh viên ĐH Bách Khoa', '2003-01-01', N'Nam', N'1m80', N'Cao', N'Cận thị nặng', N'Con', 2);

-- Thêm Cư Dân (Hộ 3)
INSERT INTO CuDan (HoTen, SDT, Email, Avatar, TrinhDoHocVan, NgaySinh, GioiTinh, NhanDang_Cao, NhanDang_SongMui, DauVetDacBiet, QuanHeVoiChuHo, MaHo) 
VALUES 
(N'Hoàng Thị Lan', '0909090909', 'lan.hoang@retired.com', '/Content/Images/default.jpg', N'Về hưu', '1960-12-25', N'Nữ', N'1m55', N'Bình thường', N'Tóc bạc trắng', N'Chủ hộ', 3),
(N'Nguyễn Ngọc Thúy', '0911223344', 'thuy.nguyen@bank.com', '/Content/Images/default.jpg', N'Thạc sĩ Tài chính', '1990-08-15', N'Nữ', N'1m65', N'Thẳng', N'Nốt ruồi son ở cổ', N'Con', 3);
GO


USE QuanLyChungCuDB;
GO

-- 1. Tạo bảng Loại Hộ
CREATE TABLE LoaiHo (
    MaLoaiHo INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL -- Ví dụ: Thường trú, Tạm trú
);
GO

-- 2. Thêm cột MaLoaiHo vào bảng HoGiaDinh
ALTER TABLE HoGiaDinh
ADD MaLoaiHo INT,
    TenChuHo NVARCHAR(100), -- Cache tên chủ hộ hiển thị cho nhanh
    TrangThai NVARCHAR(50) DEFAULT N'Đang ở'; -- Đang ở/Đã chuyển đi
GO

-- 3. Tạo khóa ngoại
ALTER TABLE HoGiaDinh
ADD FOREIGN KEY (MaLoaiHo) REFERENCES LoaiHo(MaLoaiHo);
GO

-- 4. Thêm dữ liệu mẫu
INSERT INTO LoaiHo (TenLoai) VALUES (N'Thường trú'), (N'Tạm trú'), (N'Hộ nghèo');
UPDATE HoGiaDinh SET MaLoaiHo = 1; -- Set mặc định
GO
select * from LoaiHo