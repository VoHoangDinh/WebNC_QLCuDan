using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_API.Models
{
    [Table("HoGiaDinh")]
    public class HoGiaDinh
    {
        [Key]
        public int MaHo { get; set; }

        public DateTime? NgayNhanNha { get; set; }
        public int SoThanhVien { get; set; }

        // Liên kết với Căn hộ
        public int? MaCanHo { get; set; }

        [ForeignKey("MaCanHo")]
        public virtual CanHo CanHo { get; set; }

        // 1 Hộ có nhiều Cư dân
        public virtual ICollection<CuDan> CuDans { get; set; }
    }
}