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
/* ==========================================================================
   PHẦN 1: CÁC BẢNG NGHIỆP VỤ CỐT LÕI (CHUNG CƯ & CƯ DÂN)
   (Phải tạo trước để bảng User có dữ liệu tham chiếu)
   ========================================================================== */

-- 1. Bảng Tòa Nhà
CREATE TABLE ToaNha (
    MaToaNha INT IDENTITY(1,1) PRIMARY KEY,
    TenToaNha NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(200)
);
GO

-- 2. Bảng Căn Hộ
CREATE TABLE CanHo (
    MaCanHo INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong VARCHAR(20) NOT NULL,
    DienTich FLOAT,
    Tang INT,
    TrangThai NVARCHAR(50), -- VD: Trống, Đã bàn giao
    MaToaNha INT NOT NULL,
    FOREIGN KEY (MaToaNha) REFERENCES ToaNha(MaToaNha)
);
GO

-- 3. Bảng Hộ Gia Đình
CREATE TABLE HoGiaDinh (
    MaHo INT IDENTITY(1,1) PRIMARY KEY,
    NgayNhanNha DATE,
    SoThanhVien INT DEFAULT 0,
    MaCanHo INT,
    FOREIGN KEY (MaCanHo) REFERENCES CanHo(MaCanHo)
);
GO

-- 4. Bảng Cư Dân (Đây là bảng trung tâm để nối với User)
CREATE TABLE CuDan (
    MaCuDan INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    SDT VARCHAR(20),
    Email VARCHAR(100),
    Avatar VARCHAR(255),
    
    -- Các trường đặc thù quản lý nhân khẩu
    TrinhDoHocVan NVARCHAR(100),
    NgayVaoDang DATE,
    NgayVaoDoan DATE,
    HocHamHocVi NVARCHAR(100),
    NhanDang_Cao NVARCHAR(50),
    NhanDang_SongMui NVARCHAR(50),
    DauVetDacBiet NVARCHAR(200),
    QuanHeVoiChuHo NVARCHAR(50), -- Chủ hộ, Vợ, Con...
    
    MaHo INT,
    FOREIGN KEY (MaHo) REFERENCES HoGiaDinh(MaHo)
);
GO

/* ==========================================================================
   PHẦN 2: HỆ THỐNG ASP.NET IDENTITY (USER & PHÂN QUYỀN)
   (Đã tùy biến để nối với bảng Cư Dân)
   ========================================================================== */

-- 5. Bảng Roles (Vai trò: Admin, Staff, Resident...)
CREATE TABLE [dbo].[AspNetRoles] (
    [Id] NVARCHAR(450) NOT NULL,
    [Name] NVARCHAR(256) NULL,
    [NormalizedName] NVARCHAR(256) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

-- 6. Bảng Users (Tài khoản đăng nhập) - ĐÃ MODIFIED
CREATE TABLE [dbo].[AspNetUsers] (
    [Id] NVARCHAR(450) NOT NULL,
    [UserName] NVARCHAR(256) NULL,
    [NormalizedUserName] NVARCHAR(256) NULL,
    [Email] NVARCHAR(256) NULL,
    [NormalizedEmail] NVARCHAR(256) NULL,
    [EmailConfirmed] BIT NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NULL,
    [SecurityStamp] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    [PhoneNumber] NVARCHAR(MAX) NULL,
    [PhoneNumberConfirmed] BIT NOT NULL,
    [TwoFactorEnabled] BIT NOT NULL,
    [LockoutEnd] DATETIMEOFFSET NULL,
    [LockoutEnabled] BIT NOT NULL,
    [AccessFailedCount] INT NOT NULL,

    -- Custom Fields
    [FirstName] NVARCHAR(256) NULL,
    [LastName] NVARCHAR(256) NULL,

    -- [QUAN TRỌNG] Khóa ngoại liên kết với Cư Dân
    -- Cho phép NULL (vì Admin hệ thống có thể không phải là cư dân)
    [MaCuDan] INT NULL, 

    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_CuDan] FOREIGN KEY ([MaCuDan]) REFERENCES CuDan(MaCuDan) ON DELETE SET NULL
);
GO

-- 7. Bảng RoleClaims
CREATE TABLE [dbo].[AspNetRoleClaims] (
    [Id] INT NOT NULL IDENTITY,
    [RoleId] NVARCHAR(450) NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles]([Id]) ON DELETE CASCADE
);
GO

-- 8. Bảng UserClaims
CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id] INT NOT NULL IDENTITY,
    [UserId] NVARCHAR(450) NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
);
GO

-- 9. Bảng UserLogins (Lưu token Google, FB...)
CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] NVARCHAR(450) NOT NULL,
    [ProviderKey] NVARCHAR(450) NOT NULL,
    [ProviderDisplayName] NVARCHAR(MAX) NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
);
GO

-- 10. Bảng UserRoles (Liên kết N-N giữa User và Role)
CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR(450) NOT NULL,
    [RoleId] NVARCHAR(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles]([Id]) ON DELETE CASCADE
);
GO

-- 11. Bảng UserTokens
CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId] NVARCHAR(450) NOT NULL,
    [LoginProvider] NVARCHAR(450) NOT NULL,
    [Name] NVARCHAR(450) NOT NULL,
    [Value] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
);
GO

/* ==========================================================================
   PHẦN 3: TẠO INDEX CHO IDENTITY (BẮT BUỘC ĐỂ TỐI ƯU HIỆU NĂNG)
   ========================================================================== */

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

/* ==========================================================================
   PHẦN 4: CÁC BẢNG NGHIỆP VỤ PHỤ TRỢ (LIÊN QUAN ĐẾN CƯ DÂN)
   ========================================================================== */

-- 12. Bảng Phản Ánh
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

-- 13. Bảng Thẻ Bảo Hiểm
CREATE TABLE TheBaoHiem (
    MaTheBaoHiem INT IDENTITY(1,1) PRIMARY KEY,
    MaSoThe VARCHAR(50) NOT NULL,       
    NgayDangKy DATE NOT NULL,
    NgayHetHan DATE NOT NULL,
    NoiDangKyKCB NVARCHAR(200),         
    GhiChu NVARCHAR(255),               
    MaCuDan INT NOT NULL,
    FOREIGN KEY (MaCuDan) REFERENCES CuDan(MaCuDan) ON DELETE CASCADE
);
GO

/* ==========================================================================
   PHẦN 1: DỮ LIỆU NGHIỆP VỤ (CHẠY TRƯỚC ĐỂ LẤY ID LIÊN KẾT)
   ========================================================================== */

-- 1. Thêm Tòa Nhà
INSERT INTO ToaNha (TenToaNha, DiaChi) VALUES 
(N'Tòa A - Sun Tower', N'101 Võ Văn Ngân, TP. Thủ Đức'),
(N'Tòa B - Moon Tower', N'101 Võ Văn Ngân, TP. Thủ Đức'),
(N'Tòa C - Star Tower', N'105 Lê Văn Việt, TP. Thủ Đức');
GO

-- 2. Thêm Căn Hộ (Liên kết Tòa Nhà ID 1, 2, 3)
INSERT INTO CanHo (SoPhong, DienTich, Tang, TrangThai, MaToaNha) VALUES 
('A101', 75.5, 1, N'Đã bàn giao', 1),
('B205', 80.0, 2, N'Đang trống', 2),
('C309', 65.0, 3, N'Đã bàn giao', 3);
GO

-- 3. Thêm Hộ Gia Đình (Liên kết Căn Hộ ID 1, 2, 3)
INSERT INTO HoGiaDinh (NgayNhanNha, SoThanhVien, MaCanHo) VALUES 
('2024-01-15', 4, 1), -- Hộ A101
('2024-02-20', 2, 2), -- Hộ B205
('2024-03-10', 3, 3); -- Hộ C309
GO

-- 4. Thêm Cư Dân (Liên kết Hộ Gia Đình ID 1, 2, 3)
INSERT INTO CuDan (HoTen, NgaySinh, GioiTinh, SDT, Email, QuanHeVoiChuHo, MaHo, TrinhDoHocVan) VALUES 
(N'Nguyễn Văn A', '1980-05-10', N'Nam', '0909123456', 'nguyenvana@gmail.com', N'Chủ hộ', 1, N'Đại học'),
(N'Trần Thị B', '1995-08-20', N'Nữ', '0909654321', 'tranthib@gmail.com', N'Chủ hộ', 2, N'Thạc sĩ'),
(N'Lê Văn C', '1990-12-05', N'Nam', '0912345678', 'levanc@gmail.com', N'Con', 3, N'Cao đẳng');
GO

-- 5. Thêm Phản Ánh (Liên kết Cư Dân)
INSERT INTO PhanAnh (TieuDe, NoiDung, NgayGui, TrangThai, MaCuDan) VALUES 
(N'Hỏng đèn hành lang', N'Đèn hành lang tầng 3 bị nhấp nháy', GETDATE(), N'Mới tiếp nhận', 1),
(N'Ồn ào', N'Nhà bên cạnh thi công quá ồn vào giờ nghỉ trưa', GETDATE(), N'Đang xử lý', 2),
(N'Vệ sinh', N'Hành lang chưa được quét dọn sạch', GETDATE(), N'Đã xong', 3);
GO

-- 6. Thêm Thẻ Bảo Hiểm (Liên kết Cư Dân)
INSERT INTO TheBaoHiem (MaSoThe, NgayDangKy, NgayHetHan, NoiDangKyKCB, MaCuDan) VALUES 
('BH790123456789', '2024-01-01', '2024-12-31', N'BV Thủ Đức', 1),
('BH790987654321', '2024-01-01', '2024-12-31', N'BV Quân Dân Y', 2),
('BH790112233445', '2024-06-01', '2025-05-31', N'BV Chợ Rẫy', 3);
GO

/* ==========================================================================
   PHẦN 2: DỮ LIỆU IDENTITY (USER & ROLE)
   Lưu ý: ID của Identity là chuỗi (GUID), ở đây mình đặt thủ công cho dễ nhớ.
   ========================================================================== */

-- 7. Thêm Roles (Vai trò)
INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES 
('role-admin', 'Admin', 'ADMIN'),
('role-staff', 'Staff', 'STAFF'),
('role-resident', 'Resident', 'RESIDENT');
GO

-- 8. Thêm Users
-- User 1: Admin hệ thống (Không gắn với cư dân -> MaCuDan NULL)
-- User 2: Nhân viên (Không gắn với cư dân -> MaCuDan NULL)
-- User 3: Cư dân Nguyễn Văn A (Gắn với MaCuDan = 1)
INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName, MaCuDan
) VALUES 
(
    'user-admin', 
    'nguyenvanc@gmail.com',               -- Tên đăng nhập
    'NGUYENVANC@GMAIL.COM',               -- [QUAN TRỌNG] Phải viết hoa toàn bộ Username
    'nguyenvanc@gmail.com', 
    'NGUYENVANC@GMAIL.COM', -- [QUAN TRỌNG] Phải viết hoa toàn bộ Email
    1, 
    -- Hash bên dưới là của mật khẩu: Abc@12345
    'AQAAAAIAAYagAAAAEJkaDGHXMo7KAOW0DV2d/Xq9B1ufIELdUXZiDEJmmr92xQqm7CAU36GxuIp8IFDKUw==', 
    'SECURITY_STAMP_1', 
    'CONCURRENCY_1', 
    '0123456789', 1, 0, 1, 0, 
    N'System', N'User', NULL
),

('user-staff', 'manager', 'MANAGER', 'staff@system.com', 'STAFF@SYSTEM.COM', 1, 
 'HASH_PASS_456', 'SECURITY_STAMP_2', 'CONCURRENCY_2', '0987654321', 1, 0, 1, 0, N'Quản Lý', N'Tòa Nhà', NULL),

('user-res-1', 'nguyenvana', 'NGUYENVANA', 'nguyenvana@gmail.com', 'NGUYENVANA@GMAIL.COM', 1, 
 'HASH_PASS_789', 'SECURITY_STAMP_3', 'CONCURRENCY_3', '0909123456', 1, 0, 1, 0, N'Văn A', N'Nguyễn', 1);
GO

-- 9. Gán Role cho User (AspNetUserRoles)
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES 
('user-admin', 'role-admin'),     -- Admin là Admin
('user-staff', 'role-staff'),     -- Manager là Staff
('user-res-1', 'role-resident');  -- NguyenVanA là Resident
GO

-- 10. Thêm UserClaims (Ví dụ quyền cụ thể)
INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES 
('user-admin', 'Permission', 'FullAccess'),
('user-staff', 'Permission', 'ManageBuilding'),
('user-res-1', 'Permission', 'ViewBill');
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
select * from HoGiaDinh
select * from CuDan
USE QuanLyChungCuDB;
GO

-- Cập nhật cột TenChuHo trong bảng HoGiaDinh
-- Bằng cách lấy HoTen từ bảng CuDan tương ứng
UPDATE h
SET h.TenChuHo = c.HoTen
FROM HoGiaDinh h
JOIN CuDan c ON h.MaHo = c.MaHo
WHERE c.QuanHeVoiChuHo = N'Chủ hộ';
GO

-- Kiểm tra lại kết quả
SELECT * FROM HoGiaDinh;


USE QuanLyChungCuDB;
GO

SELECT 
    h.MaHo,
    ch.SoPhong,               -- Số phòng căn hộ
    h.TenChuHo,               -- Tên chủ hộ (lấy từ bảng Hộ)
    c.HoTen AS TenThanhVien,  -- Tên thành viên
    c.QuanHeVoiChuHo,         -- Quan hệ
    c.GioiTinh,
    c.NgaySinh,
    c.SDT
FROM HoGiaDinh h
-- Join bảng Căn Hộ để biết hộ này ở phòng nào
JOIN CanHo ch ON h.MaCanHo = ch.MaCanHo
-- Join bảng Cư Dân để lấy danh sách người trong hộ
JOIN CuDan c ON h.MaHo = c.MaHo
ORDER BY 
    ch.SoPhong, -- Sắp xếp theo phòng
    -- Logic sắp xếp: Chủ hộ lên đầu, các thành viên khác ở dưới
    CASE WHEN c.QuanHeVoiChuHo = N'Chủ hộ' THEN 0 ELSE 1 END ASC;


    --- update số thành viên

    USE QuanLyChungCuDB;
GO
-- Cập nhật lại cột SoThanhVien trong bảng HoGiaDinh
-- bằng cách đếm thực tế từ bảng CuDan
UPDATE h
SET h.SoThanhVien = (
    SELECT COUNT(*) 
    FROM CuDan c 
    WHERE c.MaHo = h.MaHo
)
FROM HoGiaDinh h;
GO

-- Kiểm tra lại kết quả sau khi update
SELECT MaHo, TenChuHo, SoThanhVien FROM HoGiaDinh;


USE QuanLyChungCuDB;
GO

-- Xóa trigger cũ nếu có (để tránh lỗi)
DROP TRIGGER IF EXISTS trg_TuDongCapNhat_HoGiaDinh;
GO

-- Tạo Trigger mới
CREATE TRIGGER trg_TuDongCapNhat_HoGiaDinh
ON CuDan
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Tìm danh sách các Mã Hộ bị ảnh hưởng (Cần cập nhật)
    -- Lấy MaHo từ dữ liệu mới thêm/sửa (inserted) và dữ liệu cũ vừa xóa (deleted)
    DECLARE @DanhSachHoCanUpdate TABLE (MaHo INT);

    INSERT INTO @DanhSachHoCanUpdate
    SELECT MaHo FROM inserted WHERE MaHo IS NOT NULL
    UNION
    SELECT MaHo FROM deleted WHERE MaHo IS NOT NULL;

    -- 2. Thực hiện cập nhật lại thông tin cho các hộ đó
    UPDATE h
    SET 
        -- Tự động đếm lại số thành viên
        h.SoThanhVien = (
            SELECT COUNT(*) 
            FROM CuDan c 
            WHERE c.MaHo = h.MaHo
        ),
        -- Tự động tìm tên chủ hộ mới nhất
        h.TenChuHo = (
            SELECT TOP 1 c.HoTen 
            FROM CuDan c 
            WHERE c.MaHo = h.MaHo AND c.QuanHeVoiChuHo = N'Chủ hộ'
        )
    FROM HoGiaDinh h
    INNER JOIN @DanhSachHoCanUpdate list ON h.MaHo = list.MaHo;
END;
GO

select * from AspNetUsers


