using System.Data.Entity;

namespace BaoCaoCK_QLCuDan.Models
{
    public class QuanLyCuDanContext : DbContext
    {
        // Tên chuỗi kết nối là "QLCuDanConnection"
        public QuanLyCuDanContext() : base("name=QLCuDanConnection")
        {
        }

        // Khai báo các bảng sẽ tạo trong Database
        public virtual DbSet<VaiTro> VaiTros { get; set; }
        public virtual DbSet<NguoiDung> NguoiDungs { get; set; }
        public virtual DbSet<ToaNha> ToaNhas { get; set; }
        public virtual DbSet<CanHo> CanHos { get; set; }
        public virtual DbSet<HoGiaDinh> HoGiaDinhs { get; set; }
        public virtual DbSet<CuDan> CuDans { get; set; }
        public virtual DbSet<PhanAnh> PhanAnhs { get; set; }
    }
}