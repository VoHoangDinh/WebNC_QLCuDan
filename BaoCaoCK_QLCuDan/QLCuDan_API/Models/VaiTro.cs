using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_API.Models
{
    [Table("VaiTro")]
    public class VaiTro
    {
        [Key]
        public int MaVaiTro { get; set; }

        [Required]
        [StringLength(50)]
        public string TenVaiTro { get; set; } // Ví dụ: Admin, CuDan

        // Quan hệ: 1 Vai trò có nhiều Người dùng
        public virtual ICollection<NguoiDung> NguoiDungs { get; set; }
    }
}