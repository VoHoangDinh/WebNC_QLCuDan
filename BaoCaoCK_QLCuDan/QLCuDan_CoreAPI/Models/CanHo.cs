using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    [Table("CanHo")]
    public class CanHo
    {
        [Key]
        public int MaCanHo { get; set; }
        public string SoPhong { get; set; }
        public double? DienTich { get; set; }
        public int? Tang { get; set; }
        public string? TrangThai { get; set; }
        public int MaToaNha { get; set; }
    }
}