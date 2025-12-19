using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("LoaiHo")]
    public class LoaiHo
    {
        [Key]
        public int MaLoaiHo { get; set; }
        public string TenLoai { get; set; }

        public virtual ICollection<HoGiaDinh> HoGiaDinhs { get; set; }
    }
}