using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("CanHo")]
    public class CanHo
    {
        [Key]
        public int MaCanHo { get; set; }

        [Required]
        public string SoPhong { get; set; } // Ví dụ: P101

        public double DienTich { get; set; }
        public int Tang { get; set; }
        public string TrangThai { get; set; } // Đã ở, Trống

        // Liên kết với Tòa Nhà
        public int MaToaNha { get; set; }

        [ForeignKey("MaToaNha")]
        public virtual ToaNha ToaNha { get; set; }

        // 1 Căn hộ có thể có nhiều Hộ gia đình (lịch sử ở)
        public virtual ICollection<HoGiaDinh> HoGiaDinhs { get; set; }
    }
}