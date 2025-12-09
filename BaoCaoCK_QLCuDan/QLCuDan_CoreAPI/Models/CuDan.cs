using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    [Table("CuDan")]
    public class CuDan
    {
        [Key]
        public int MaCuDan { get; set; }

        [Required]
        public string HoTen { get; set; } // Cái này bắt buộc -> Không cần dấu ?

        // CÁC TRƯỜNG DƯỚI ĐÂY CÓ THỂ NULL -> THÊM DẤU ? VÀO
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; } // Thêm ?
        public string? SDT { get; set; }      // Thêm ?
        public string? Email { get; set; }    // Thêm ?
        public string? Avatar { get; set; }   // Thêm ?

        // Các trường đặc thù (THÊM ? HẾT)
        public string? TrinhDoHocVan { get; set; }
        public DateTime? NgayVaoDang { get; set; }
        public DateTime? NgayVaoDoan { get; set; }
        public string? NhanDang_Cao { get; set; }
        public string? NhanDang_SongMui { get; set; }
        public string? DauVetDacBiet { get; set; }
        public string? HocHamHocVi { get; set; }

        // Các trường mới thêm (QUAN TRỌNG: PHẢI CÓ ?)
        public string? BiDanh { get; set; }
        public string? TenKhac { get; set; }
        public string? NoiSinh { get; set; }
        public string? NguyenQuan { get; set; }
        public string? TruQuan { get; set; }
        public string? NoiOHienNay { get; set; }
        public string? DanToc { get; set; }
        public string? TonGiao { get; set; }
        public string? ThanhPhanGiaDinh { get; set; }
        public string? ThanhPhanBanThan { get; set; }
        public string? TieuSu { get; set; }

        public string? HoTenVoChong { get; set; }
        public DateTime? NgaySinhVoChong { get; set; }
        public string? QueQuanVoChong { get; set; }
        public string? NgheNghiepVoChong { get; set; }

        public string? QuanHeVoiChuHo { get; set; }
        public string? AccountId { get; set; }

        public int? MaHo { get; set; }
        [ForeignKey("MaHo")]
        public virtual HoGiaDinh? HoGiaDinh { get; set; }

        public virtual ICollection<PhanAnh>? PhanAnhs { get; set; }
    }
}