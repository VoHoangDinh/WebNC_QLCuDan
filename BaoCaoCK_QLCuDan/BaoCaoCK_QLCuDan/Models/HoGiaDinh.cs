using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json; // Thư viện quan trọng để map dữ liệu từ API

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("HoGiaDinh")]
    public class HoGiaDinh
    {
        [Key]
        public int MaHo { get; set; }

        // --- Các trường thông tin cơ bản ---

        [Display(Name = "Chủ Hộ")]
        public string TenChuHo { get; set; } // API trả về tên này, MVC hứng được luôn

        [Display(Name = "Trạng Thái")]
        public string TrangThai { get; set; }

        [Display(Name = "Ngày Nhận Nhà")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? NgayNhanNha { get; set; }

        [Display(Name = "Số Thành Viên")]
        public int SoThanhVien { get; set; }

        // --- Khóa ngoại (Foreign Keys) ---
        public int? MaCanHo { get; set; }
        public int? MaLoaiHo { get; set; }

        // --- Thuộc tính điều hướng (Navigation Properties) ---
        // Dùng [JsonProperty] để ép MVC hiểu dữ liệu từ API bất kể tên gì

        [ForeignKey("MaCanHo")]
        [JsonProperty("maCanHoNavigation")] // API trả về "maCanHoNavigation" -> Map vào "CanHo"
        public virtual CanHo CanHo { get; set; }

        [ForeignKey("MaLoaiHo")]
        [JsonProperty("maLoaiHoNavigation")] // API trả về "maLoaiHoNavigation" -> Map vào "LoaiHo"
        public virtual LoaiHo LoaiHo { get; set; }

        // Danh sách cư dân trong hộ
        public virtual ICollection<CuDan> CuDans { get; set; }
    }
}