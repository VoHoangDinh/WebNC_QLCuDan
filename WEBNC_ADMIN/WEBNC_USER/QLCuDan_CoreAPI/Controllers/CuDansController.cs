using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;

namespace QLCuDan_CoreAPI.Controllers
{
    // 1. Định nghĩa đường dẫn là: api/CuDans
    [Route("api/[controller]")]
    [ApiController]
    // 2. Kế thừa ControllerBase
    public class CuDansController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public CuDansController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // SỬA ĐOẠN NÀY: GET api/CuDans (Hỗ trợ Tìm kiếm & Phân trang)
        // =========================================================================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCuDans(string? keyword, int page = 1, int pageSize = 10)
        {
            // 1. Khởi tạo truy vấn
            var query = _context.CuDans.AsQueryable();

            // 2. Tìm kiếm (Nếu có từ khóa)
            if (!string.IsNullOrEmpty(keyword))
            {
                // Tìm theo Tên, SĐT hoặc Email
                query = query.Where(c => c.HoTen.Contains(keyword) ||
                                         c.SDT.Contains(keyword) ||
                                         c.Email.Contains(keyword));
            }

            // 3. Đếm tổng số kết quả (quan trọng để tính số trang)
            int totalItems = await query.CountAsync();

            // 4. Phân trang: Bỏ qua (Skip) các trang trước và Lấy (Take) số lượng trang hiện tại
            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            // 5. Trả về cấu trúc JSON mới gồm cả dữ liệu và thông tin phân trang
            return Ok(new
            {
                TotalItems = totalItems, // Tổng số bản ghi tìm thấy
                Page = page,             // Trang hiện tại
                PageSize = pageSize,     // Kích thước trang
                Items = items            // Danh sách cư dân của trang này
            });
        }
        // =========================================================================

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
            _context.CuDans.Add(cuDan);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCuDan", new { id = cuDan.MaCuDan }, cuDan);
        }
        // PUT: api/CuDans/5
        [Authorize(Policy = "UpdateUser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuDan(int id, CuDan cuDan)
        {
            if (id != cuDan.MaCuDan)
            {
                return BadRequest();
            }

            // Lấy thông tin cư dân hiện tại từ database
            var existingCuDan = await _context.CuDans.AsNoTracking().FirstOrDefaultAsync(c => c.MaCuDan == id);
            if (existingCuDan == null)
            {
                return NotFound();
            }

            // Bảo vệ MaCuDan, HoTen (không cho phép cập nhật)
            cuDan.MaCuDan = existingCuDan.MaCuDan;
            cuDan.HoTen = existingCuDan.HoTen;

            // Thực hiện UPDATE thuần SQL để tránh OUTPUT clause (bảng có trigger)
            var sql = @"
UPDATE CuDan
SET NgaySinh = @NgaySinh,
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
    QuanHeVoiChuHo = @QuanHeVoiChuHo
WHERE MaCuDan = @MaCuDan";

            var parameters = new[]
            {
                new SqlParameter("@NgaySinh", (object?)cuDan.NgaySinh ?? DBNull.Value),
                new SqlParameter("@GioiTinh", (object?)cuDan.GioiTinh ?? DBNull.Value),
                new SqlParameter("@SDT", (object?)cuDan.SDT ?? DBNull.Value),
                new SqlParameter("@Email", (object?)cuDan.Email ?? DBNull.Value),
                new SqlParameter("@Avatar", (object?)cuDan.Avatar ?? DBNull.Value),
                new SqlParameter("@TrinhDoHocVan", (object?)cuDan.TrinhDoHocVan ?? DBNull.Value),
                new SqlParameter("@NgayVaoDang", (object?)cuDan.NgayVaoDang ?? DBNull.Value),
                new SqlParameter("@NgayVaoDoan", (object?)cuDan.NgayVaoDoan ?? DBNull.Value),
                new SqlParameter("@HocHamHocVi", (object?)cuDan.HocHamHocVi ?? DBNull.Value),
                new SqlParameter("@NhanDang_Cao", (object?)cuDan.NhanDang_Cao ?? DBNull.Value),
                new SqlParameter("@NhanDang_SongMui", (object?)cuDan.NhanDang_SongMui ?? DBNull.Value),
                new SqlParameter("@DauVetDacBiet", (object?)cuDan.DauVetDacBiet ?? DBNull.Value),
                new SqlParameter("@QuanHeVoiChuHo", (object?)cuDan.QuanHeVoiChuHo ?? DBNull.Value),
                new SqlParameter("@MaCuDan", cuDan.MaCuDan)
            };

            var affected = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            if (affected == 0)
            {
                return NotFound();
            }

            return NoContent();
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