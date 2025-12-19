using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("PhanAnh")]
    public class PhanAnh
    {
        [Key]
        public int MaPhanAnh { get; set; }

        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime? NgayGui { get; set; } // Giữ nguyên DateTime? để tránh lỗi nếu dữ liệu null

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; }

        public int? MaCuDan { get; set; } // Để int? (nullable) vì trong SQL cho phép null

        // [QUAN TRỌNG] Phải có dòng này thì View mới lấy được tên: Model.CuDan.HoTen
        [ForeignKey("MaCuDan")]
        public virtual CuDan CuDan { get; set; }
    }
}