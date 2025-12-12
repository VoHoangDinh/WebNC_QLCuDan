using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QLCuDan_CoreAPI.Controllers
{
    // 1. Định nghĩa đường dẫn là: api/CuDans
    [Route("api/[controller]")]
    [ApiController]
    // 2. Kế thừa ControllerBase (Chuẩn cho API), KHÔNG phải Controller (Chuẩn cho MVC)
    public class CuDansController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public CuDansController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // GET: api/CuDans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuDan>>> GetCuDans()
        {
            // Trả về dữ liệu thô (JSON), không phải View
            return await _context.CuDans.ToListAsync();
        }

        // GET: api/CuDans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CuDan>> GetCuDan(int id)
        {
            var cuDan = await _context.CuDans.FindAsync(id);

            if (cuDan == null)
            {
                return NotFound();
            }

            return cuDan;
        }

        // POST: api/CuDans
        [HttpPost]
        public async Task<ActionResult<CuDan>> PostCuDan(CuDan cuDan)
        {
            try
            {
                // Sử dụng raw SQL để tránh lỗi OUTPUT clause với trigger
                var sql = @"INSERT INTO CuDan (HoTen, NgaySinh, GioiTinh, SDT, Email, Avatar, TrinhDoHocVan, 
                            NgayVaoDang, NgayVaoDoan, HocHamHocVi, NhanDang_Cao, NhanDang_SongMui, 
                            DauVetDacBiet, QuanHeVoiChuHo, MaHo)
                           VALUES (@HoTen, @NgaySinh, @GioiTinh, @SDT, @Email, @Avatar, @TrinhDoHocVan,
                            @NgayVaoDang, @NgayVaoDoan, @HocHamHocVi, @NhanDang_Cao, @NhanDang_SongMui,
                            @DauVetDacBiet, @QuanHeVoiChuHo, @MaHo);";

                var parameters = new[]
                {
                    new SqlParameter("@HoTen", cuDan.HoTen ?? (object)DBNull.Value),
                    new SqlParameter("@NgaySinh", cuDan.NgaySinh ?? (object)DBNull.Value),
                    new SqlParameter("@GioiTinh", cuDan.GioiTinh ?? (object)DBNull.Value),
                    new SqlParameter("@SDT", cuDan.SDT ?? (object)DBNull.Value),
                    new SqlParameter("@Email", cuDan.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Avatar", cuDan.Avatar ?? (object)DBNull.Value),
                    new SqlParameter("@TrinhDoHocVan", cuDan.TrinhDoHocVan ?? (object)DBNull.Value),
                    new SqlParameter("@NgayVaoDang", cuDan.NgayVaoDang ?? (object)DBNull.Value),
                    new SqlParameter("@NgayVaoDoan", cuDan.NgayVaoDoan ?? (object)DBNull.Value),
                    new SqlParameter("@HocHamHocVi", cuDan.HocHamHocVi ?? (object)DBNull.Value),
                    new SqlParameter("@NhanDang_Cao", cuDan.NhanDang_Cao ?? (object)DBNull.Value),
                    new SqlParameter("@NhanDang_SongMui", cuDan.NhanDang_SongMui ?? (object)DBNull.Value),
                    new SqlParameter("@DauVetDacBiet", cuDan.DauVetDacBiet ?? (object)DBNull.Value),
                    new SqlParameter("@QuanHeVoiChuHo", cuDan.QuanHeVoiChuHo ?? (object)DBNull.Value),
                    new SqlParameter("@MaHo", cuDan.MaHo ?? (object)DBNull.Value)
                };

                // Lưu MaHo và HoTen để query lại
                var maHoValue = cuDan.MaHo;
                var hoTenValue = cuDan.HoTen;
                
                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                
                // Query lại từ database để lấy dữ liệu đầy đủ
                // Tìm record mới nhất có cùng MaHo và HoTen (nếu có MaHo)
                CuDan savedCuDan = null;
                if (maHoValue.HasValue)
                {
                    savedCuDan = await _context.CuDans
                        .AsNoTracking()
                        .Where(c => c.MaHo == maHoValue && c.HoTen == hoTenValue)
                        .OrderByDescending(c => c.MaCuDan)
                        .FirstOrDefaultAsync();
                }
                
                // Fallback: lấy record mới nhất
                if (savedCuDan == null)
                {
                    savedCuDan = await _context.CuDans
                        .AsNoTracking()
                        .OrderByDescending(c => c.MaCuDan)
                        .FirstOrDefaultAsync();
                }

                if (savedCuDan == null)
                {
                    return StatusCode(500, new { message = "Không thể lấy lại dữ liệu sau khi lưu" });
                }

                return CreatedAtAction("GetCuDan", new { id = savedCuDan.MaCuDan }, savedCuDan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm cư dân: " + ex.Message + " | Inner: " + ex.InnerException?.Message });
            }
        }

        // PUT: api/CuDans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuDan(int id, CuDan cuDan)
        {
            if (id != cuDan.MaCuDan)
            {
                return BadRequest();
            }

            try
            {
                // Kiểm tra cư dân có tồn tại không
                var existingCuDan = await _context.CuDans.FindAsync(id);
                if (existingCuDan == null)
                {
                    return NotFound();
                }

                // Dùng raw SQL để tránh trigger conflict
                var sql = @"UPDATE CuDan SET 
                           HoTen = @HoTen, 
                           NgaySinh = @NgaySinh, 
                           GioiTinh = @GioiTinh, 
                           SDT = @SDT, 
                           Email = @Email, 
                           Avatar = @Avatar, 
                           TrinhDoHocVan = @TrinhDoHocVan,
                           NgayVaoDang = @NgayVaoDang, 
                           NgayVaoDoan = @NgayVaoDoan, 
                           HocHamHocVi = @HocHamHocVi,
                           NhanDang_Cao = @NhanDang_Cao, 
                           NhanDang_SongMui = @NhanDang_SongMui,
                           DauVetDacBiet = @DauVetDacBiet, 
                           QuanHeVoiChuHo = @QuanHeVoiChuHo, 
                           MaHo = @MaHo
                           WHERE MaCuDan = @MaCuDan;";

                var parameters = new[]
                {
                    new SqlParameter("@HoTen", cuDan.HoTen ?? (object)DBNull.Value),
                    new SqlParameter("@NgaySinh", cuDan.NgaySinh ?? (object)DBNull.Value),
                    new SqlParameter("@GioiTinh", cuDan.GioiTinh ?? (object)DBNull.Value),
                    new SqlParameter("@SDT", cuDan.SDT ?? (object)DBNull.Value),
                    new SqlParameter("@Email", cuDan.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Avatar", cuDan.Avatar ?? (object)DBNull.Value),
                    new SqlParameter("@TrinhDoHocVan", cuDan.TrinhDoHocVan ?? (object)DBNull.Value),
                    new SqlParameter("@NgayVaoDang", cuDan.NgayVaoDang ?? (object)DBNull.Value),
                    new SqlParameter("@NgayVaoDoan", cuDan.NgayVaoDoan ?? (object)DBNull.Value),
                    new SqlParameter("@HocHamHocVi", cuDan.HocHamHocVi ?? (object)DBNull.Value),
                    new SqlParameter("@NhanDang_Cao", cuDan.NhanDang_Cao ?? (object)DBNull.Value),
                    new SqlParameter("@NhanDang_SongMui", cuDan.NhanDang_SongMui ?? (object)DBNull.Value),
                    new SqlParameter("@DauVetDacBiet", cuDan.DauVetDacBiet ?? (object)DBNull.Value),
                    new SqlParameter("@QuanHeVoiChuHo", cuDan.QuanHeVoiChuHo ?? (object)DBNull.Value),
                    new SqlParameter("@MaHo", cuDan.MaHo ?? (object)DBNull.Value),
                    new SqlParameter("@MaCuDan", id)
                };

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật cư dân: " + ex.Message + " | Inner: " + ex.InnerException?.Message });
            }
        }

        // DELETE: api/CuDans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuDan(int id)
        {
            var cuDan = await _context.CuDans.FindAsync(id);
            if (cuDan == null)
            {
                return NotFound();
            }

            _context.CuDans.Remove(cuDan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CuDanExists(int id)
        {
            return _context.CuDans.Any(e => e.MaCuDan == id);
        }
    }
}