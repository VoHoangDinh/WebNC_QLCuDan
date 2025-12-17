using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("HoGiaDinh")]
    public class HoGiaDinh
    {
        [Key]
        public int MaHo { get; set; }

        public DateTime? NgayNhanNha { get; set; }
        public int? SoThanhVien { get; set; }
        public int? MaCanHo { get; set; }

        // --- CÁC CỘT MỚI THÊM VÀO ---
        public int? MaLoaiHo { get; set; }
        public string TenChuHo { get; set; } // Cột này quan trọng để hiển thị Dropdown
        public string TrangThai { get; set; }
        // -----------------------------

        // Giữ nguyên các quan hệ nếu có
        // [ForeignKey("MaCanHo")]
        // public virtual CanHo CanHo { get; set; }
    }
}