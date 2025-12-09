using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required]
        [StringLength(50)]
        public string TaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string MatKhau { get; set; }

        public string HoTen { get; set; }
        public string Email { get; set; }

        // Khóa ngoại liên kết với bảng VaiTro
        public int MaVaiTro { get; set; }

        [ForeignKey("MaVaiTro")]
        public virtual VaiTro VaiTro { get; set; }
    }
}