using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    [Table("TheBaoHiem")]
    public class TheBaoHiem
    {
        [Key]
        public int MaTheBaoHiem { get; set; }

        [Required]
        [StringLength(50)]
        public string MaSoThe { get; set; } // Mã số thẻ BHYT

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayDangKy { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayHetHan { get; set; }

        [StringLength(200)]
        public string? NoiDangKyKCB { get; set; } // Cho phép null

        [StringLength(255)]
        public string? GhiChu { get; set; }

        // Khóa ngoại
        public int MaCuDan { get; set; }

        // Navigation property (Optional: để tránh vòng lặp JSON khi API serialize, có thể dùng [JsonIgnore] nếu cần)
        [ForeignKey("MaCuDan")]
        public virtual CuDan? CuDan { get; set; }
    }
}