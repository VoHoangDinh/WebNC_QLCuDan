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
        public int? MaCanHo { get; set; }
    }
}