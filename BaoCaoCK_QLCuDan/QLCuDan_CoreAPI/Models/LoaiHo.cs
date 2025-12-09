using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    [Table("LoaiHo")]
    public class LoaiHo
    {
        [Key]
        public int MaLoaiHo { get; set; }
        public string TenLoai { get; set; }
    }
}