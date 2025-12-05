using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    [Table("HoGiaDinh")]
    public class HoGiaDinh
    {
        [Key]
        public int MaHo { get; set; }
        public DateTime? NgayNhanNha { get; set; }
        public int? SoThanhVien { get; set; }

        // --- CÁC CỘT MỚI BẠN THÊM TRONG SQL ---
        public string? TenChuHo { get; set; }
        public string? TrangThai { get; set; }

        // --- KHÓA NGOẠI ---
        public int? MaCanHo { get; set; }
        public int? MaLoaiHo { get; set; } // Khóa ngoại Loại Hộ

        // --- NAVIGATION PROPERTIES (QUAN TRỌNG ĐỂ .Include CHẠY ĐƯỢC) ---
        [ForeignKey("MaCanHo")]
        public virtual CanHo? MaCanHoNavigation { get; set; }

        [ForeignKey("MaLoaiHo")]
        public virtual LoaiHo? MaLoaiHoNavigation { get; set; }
    }
}