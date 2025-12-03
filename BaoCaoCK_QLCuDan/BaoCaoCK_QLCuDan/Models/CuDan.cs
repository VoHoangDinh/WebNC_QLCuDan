using System;
using System.Collections.Generic; // Thêm thư viện này
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("CuDan")]
    public class CuDan
    {
        [Key]
        public int MaCuDan { get; set; }

        [Required]
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string SDT { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }

        // Các trường chi tiết theo ảnh bạn gửi
        public string TrinhDoHocVan { get; set; }
        public DateTime? NgayVaoDang { get; set; }
        public DateTime? NgayVaoDoan { get; set; }
        public string NhanDang_Cao { get; set; }
        public string NhanDang_SongMui { get; set; }
        public string DauVetDacBiet { get; set; }
        public string HocHamHocVi { get; set; }

        // Khóa ngoại liên kết với Hộ Gia Đình
        public int? MaHo { get; set; }

        [ForeignKey("MaHo")]
        public virtual HoGiaDinh HoGiaDinh { get; set; }

        // 1 Cư dân có thể gửi nhiều phản ánh
        public virtual ICollection<PhanAnh> PhanAnhs { get; set; }
    }
}