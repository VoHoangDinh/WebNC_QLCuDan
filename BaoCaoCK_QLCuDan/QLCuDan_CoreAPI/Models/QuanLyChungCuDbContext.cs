using Microsoft.EntityFrameworkCore;

namespace QLCuDan_CoreAPI.Models
{
    public class QuanLyChungCuDbContext : DbContext
    {
        public QuanLyChungCuDbContext(DbContextOptions<QuanLyChungCuDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CanHo> CanHos { get; set; }
        public virtual DbSet<CuDan> CuDans { get; set; }
        public virtual DbSet<HoGiaDinh> HoGiaDinhs { get; set; }
        public virtual DbSet<NguoiDung> NguoiDungs { get; set; }
        public virtual DbSet<PhanAnh> PhanAnhs { get; set; }
        public virtual DbSet<ToaNha> ToaNhas { get; set; }
        public virtual DbSet<VaiTro> VaiTros { get; set; }
    }
}