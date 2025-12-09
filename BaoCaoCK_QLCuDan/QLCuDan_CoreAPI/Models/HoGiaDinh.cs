using System;
using System.Collections.Generic; // <--- Nhớ dòng này
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// using System.Text.Json.Serialization; // Mở cái này nếu cần JsonIgnore

namespace QLCuDan_CoreAPI.Models
{
    [Table("HoGiaDinh")]
    public class HoGiaDinh
    {
        [Key]
        public int MaHo { get; set; }
        public DateTime? NgayNhanNha { get; set; }
        public int? SoThanhVien { get; set; }

        public string? TenChuHo { get; set; }
        public string? TrangThai { get; set; }

        public int? MaCanHo { get; set; }
        public int? MaLoaiHo { get; set; }

        [ForeignKey("MaCanHo")]
        public virtual CanHo? MaCanHoNavigation { get; set; }

        [ForeignKey("MaLoaiHo")]
        public virtual LoaiHo? MaLoaiHoNavigation { get; set; }

        // --- BẮT BUỘC THÊM DÒNG NÀY ĐỂ API LẤY ĐƯỢC THÀNH VIÊN ---
        public virtual ICollection<CuDan> CuDans { get; set; }
    }
}