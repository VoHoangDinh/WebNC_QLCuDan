using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    [Table("CuDan")]
    public class CuDan
    {
        [Key]
        public int MaCuDan { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }

        // Các trường bổ sung
        public string? TrinhDoHocVan { get; set; }
        public DateTime? NgayVaoDang { get; set; }
        public DateTime? NgayVaoDoan { get; set; }
        public string? HocHamHocVi { get; set; }
        public string? NhanDang_Cao { get; set; }
        public string? NhanDang_SongMui { get; set; }
        public string? DauVetDacBiet { get; set; }
        public string? QuanHeVoiChuHo { get; set; }

        public int? MaHo { get; set; }

        // Có thể bỏ qua khóa ngoại nếu chỉ làm API đơn giản, hoặc thêm vào nếu cần include
        // [ForeignKey("MaHo")]
        // public virtual HoGiaDinh? HoGiaDinh { get; set; }
    }
}