using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // <--- Thêm thư viện này

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

        public string? TrinhDoHocVan { get; set; }
        public DateTime? NgayVaoDang { get; set; }
        public DateTime? NgayVaoDoan { get; set; }
        public string? HocHamHocVi { get; set; }
        public string? NhanDang_Cao { get; set; }
        public string? NhanDang_SongMui { get; set; }
        public string? DauVetDacBiet { get; set; }
        public string? QuanHeVoiChuHo { get; set; }

        // --- KHÓA NGOẠI ---
        public int? MaHo { get; set; }

        // --- BẮT BUỘC MỞ LẠI ĐOẠN NÀY ĐỂ EF HIỂU LIÊN KẾT ---
        [ForeignKey("MaHo")] // Chỉ định rõ MaHo là khóa ngoại
        [JsonIgnore]         // Thêm cái này để cắt vòng lặp vô tận (Ho -> Dan -> Ho...)
        public virtual HoGiaDinh? HoGiaDinh { get; set; }
    }
}