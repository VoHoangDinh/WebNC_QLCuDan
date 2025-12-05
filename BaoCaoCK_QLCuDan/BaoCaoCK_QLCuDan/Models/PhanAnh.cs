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

        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayGui { get; set; }
        public string TrangThai { get; set; } // Mới, Đang xử lý, Đã xong

        // Ai là người gửi?
        public int? MaCuDan { get; set; }

        [ForeignKey("MaCuDan")]
        public virtual CuDan CuDan { get; set; }
    }
}