using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Caching;

namespace QLCuDan_API.Models
{
    [Table("ToaNha")]
    public class ToaNha
    {
        [Key]
        public int MaToaNha { get; set; }

        [Required]
        public string TenToaNha { get; set; } // Ví dụ: Tòa A, Tòa B

        public string DiaChi { get; set; }

        // 1 Tòa nhà có nhiều Căn hộ
        public virtual ICollection<CanHo> CanHos { get; set; }
    }
}