using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoCK_QLCuDan.Models
{
    [Table("TheBaoHiem")]
    public class TheBaoHiem
    {
        [Key]
        public int MaTheBaoHiem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã số thẻ")]
        [StringLength(50)]
        [Display(Name = "Mã Số Thẻ")]
        public string MaSoThe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày đăng ký")]
        [Display(Name = "Ngày Đăng Ký")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NgayDangKy { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày hết hạn")]
        [Display(Name = "Ngày Hết Hạn")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NgayHetHan { get; set; }

        [StringLength(200)]
        [Display(Name = "Nơi Đăng Ký KCB")]
        public string NoiDangKyKCB { get; set; }

        [StringLength(255)]
        [Display(Name = "Ghi Chú")]
        public string GhiChu { get; set; }

        // Khóa ngoại
        [Display(Name = "Cư Dân")]
        public int MaCuDan { get; set; }

        // Relationship để lấy tên Cư dân hiển thị ra view
        [ForeignKey("MaCuDan")]
        public virtual CuDan CuDan { get; set; }
    }
}