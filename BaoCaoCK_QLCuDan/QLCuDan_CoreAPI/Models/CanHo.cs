using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Thêm dòng này để dùng List<>
using System.Collections.Generic;
using System.Text.Json.Serialization; // Thêm thư viện này

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

        // --- THÊM 2 DÒNG NÀY ĐỂ HẾT BÁO ĐỎ ---
        [JsonIgnore] // Bỏ qua khi tạo JSON để tránh vòng lặp
        [ForeignKey("MaToaNha")]
        public virtual ToaNha? ToaNha { get; set; }

        [JsonIgnore]
        public virtual ICollection<HoGiaDinh>? HoGiaDinhs { get; set; }
    }
}