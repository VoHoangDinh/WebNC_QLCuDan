using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoGiaDinhsController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public HoGiaDinhsController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // GET: api/HoGiaDinhs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HoGiaDinh>>> GetHoGiaDinhs()
        {
            var listHo = await _context.HoGiaDinhs
                .Include(h => h.MaCanHoNavigation)
                .Include(h => h.MaLoaiHoNavigation)
                .Include(h => h.CuDans) // Lấy danh sách thành viên
                .ToListAsync();

            // Logic cập nhật tên chủ hộ (An toàn)
            foreach (var ho in listHo)
            {
                // Chỉ tính toán lại NẾU trong DB chưa có tên (bị null)
                if (string.IsNullOrEmpty(ho.TenChuHo))
                {
                    if (ho.CuDans != null && ho.CuDans.Any())
                    {
                        var chuHo = ho.CuDans.FirstOrDefault(c =>
                            c.QuanHeVoiChuHo != null &&
                            c.QuanHeVoiChuHo.Trim().ToLower() == "chủ hộ");

                        if (chuHo != null) ho.TenChuHo = chuHo.HoTen;
                        else ho.TenChuHo = "Chưa cập nhật";
                    }
                }
                // Nếu DB đã có tên (do bạn chạy SQL update) thì kệ nó, không sửa gì cả.
            }

            return listHo;
        }

        // GET: api/HoGiaDinhs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HoGiaDinh>> GetHoGiaDinh(int id)
        {
            var hoGiaDinh = await _context.HoGiaDinhs
                .Include(h => h.MaCanHoNavigation)
                .Include(h => h.MaLoaiHoNavigation)
                .Include(h => h.CuDans) // Lấy danh sách thành viên
                .FirstOrDefaultAsync(h => h.MaHo == id);

            if (hoGiaDinh == null) return NotFound();

            // Logic tương tự: Chỉ điền nếu đang trống
            if (string.IsNullOrEmpty(hoGiaDinh.TenChuHo))
            {
                var chuHo = hoGiaDinh.CuDans?.FirstOrDefault(c =>
                    c.QuanHeVoiChuHo != null &&
                    c.QuanHeVoiChuHo.Trim().ToLower() == "chủ hộ");

                if (chuHo != null) hoGiaDinh.TenChuHo = chuHo.HoTen;
                else hoGiaDinh.TenChuHo = "Chưa cập nhật";
            }

            return hoGiaDinh;
        }

        // ... (Giữ nguyên các hàm POST, PUT, DELETE cũ) ...
        // POST: api/HoGiaDinhs
        [HttpPost]
        public async Task<ActionResult<HoGiaDinh>> PostHoGiaDinh(HoGiaDinh hoGiaDinh)
        {
            // Kiểm tra trùng căn hộ nếu trạng thái là "Đang ở"
            if (hoGiaDinh.TrangThai == "Đang ở")
            {
                bool isDuplicate = await _context.HoGiaDinhs
                    .AnyAsync(h => h.MaCanHo == hoGiaDinh.MaCanHo && h.TrangThai == "Đang ở");
                if (isDuplicate) return Conflict(new { message = "Căn hộ này đã có người ở." });
            }

            try
            {
                // Dùng raw SQL để tránh mọi vấn đề với trigger hoặc validation
                var sql = @"INSERT INTO HoGiaDinh (NgayNhanNha, MaCanHo, MaLoaiHo, TrangThai, TenChuHo, SoThanhVien)
                           VALUES (@NgayNhanNha, @MaCanHo, @MaLoaiHo, @TrangThai, @TenChuHo, @SoThanhVien);";

                var parameters = new[]
                {
                    new SqlParameter("@NgayNhanNha", hoGiaDinh.NgayNhanNha ?? (object)DBNull.Value),
                    new SqlParameter("@MaCanHo", hoGiaDinh.MaCanHo ?? (object)DBNull.Value),
                    new SqlParameter("@MaLoaiHo", hoGiaDinh.MaLoaiHo ?? (object)DBNull.Value),
                    new SqlParameter("@TrangThai", hoGiaDinh.TrangThai ?? (object)DBNull.Value),
                    new SqlParameter("@TenChuHo", hoGiaDinh.TenChuHo ?? (object)DBNull.Value),
                    new SqlParameter("@SoThanhVien", hoGiaDinh.SoThanhVien ?? (object)DBNull.Value)
                };

                // Lưu các giá trị để query lại
                var maCanHoValue = hoGiaDinh.MaCanHo;
                var trangThaiValue = hoGiaDinh.TrangThai;
                
                // Execute insert
                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                
                // Query lại để lấy record vừa tạo (lấy record mới nhất có cùng MaCanHo và TrangThai)
                HoGiaDinh savedHo = null;
                if (maCanHoValue.HasValue)
                {
                    savedHo = await _context.HoGiaDinhs
                        .Include(h => h.MaCanHoNavigation)
                        .Include(h => h.MaLoaiHoNavigation)
                        .Where(h => h.MaCanHo == maCanHoValue && h.TrangThai == trangThaiValue)
                        .OrderByDescending(h => h.MaHo)
                        .FirstOrDefaultAsync();
                }
                
                // Fallback: lấy record mới nhất
                if (savedHo == null)
                {
                    savedHo = await _context.HoGiaDinhs
                        .Include(h => h.MaCanHoNavigation)
                        .Include(h => h.MaLoaiHoNavigation)
                        .OrderByDescending(h => h.MaHo)
                        .FirstOrDefaultAsync();
                }

                if (savedHo == null)
                {
                    return StatusCode(500, new { message = "Không thể lấy lại dữ liệu sau khi tạo" });
                }

                return CreatedAtAction("GetHoGiaDinh", new { id = savedHo.MaHo }, savedHo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo hộ gia đình: " + ex.Message });
            }
        }

        // PUT: api/HoGiaDinhs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHoGiaDinh(int id, [FromBody] HoGiaDinhUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.MaHo) return BadRequest();
            
            // Kiểm tra hộ có tồn tại không
            var existingHo = await _context.HoGiaDinhs.FindAsync(id);
            if (existingHo == null) return NotFound();
            
            // Kiểm tra trùng căn hộ nếu trạng thái là "Đang ở"
            if (updateDto.TrangThai == "Đang ở")
            {
                bool isDuplicate = await _context.HoGiaDinhs
                    .AnyAsync(h => h.MaCanHo == updateDto.MaCanHo && h.TrangThai == "Đang ở" && h.MaHo != id);
                if (isDuplicate) return Conflict(new { message = "Căn hộ này đã có người ở." });
            }
            
            try
            {
                // Dùng raw SQL để tránh mọi vấn đề với trigger hoặc validation
                var sql = @"UPDATE HoGiaDinh 
                           SET NgayNhanNha = @NgayNhanNha, 
                               MaCanHo = @MaCanHo, 
                               MaLoaiHo = @MaLoaiHo, 
                               TrangThai = @TrangThai
                           WHERE MaHo = @MaHo;
                           -- TenChuHo và SoThanhVien sẽ được trigger tự động cập nhật, không cần set ở đây";

                var parameters = new[]
                {
                    new SqlParameter("@NgayNhanNha", updateDto.NgayNhanNha ?? (object)DBNull.Value),
                    new SqlParameter("@MaCanHo", updateDto.MaCanHo ?? (object)DBNull.Value),
                    new SqlParameter("@MaLoaiHo", updateDto.MaLoaiHo ?? (object)DBNull.Value),
                    new SqlParameter("@TrangThai", updateDto.TrangThai ?? (object)DBNull.Value),
                    new SqlParameter("@MaHo", id)
                };

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật hộ gia đình: " + ex.Message });
            }
        }

        // DTO cho Update (không có CuDans)
        public class HoGiaDinhUpdateDto
        {
            public int MaHo { get; set; }
            public DateTime? NgayNhanNha { get; set; }
            public int? SoThanhVien { get; set; }
            public string? TenChuHo { get; set; }
            public string? TrangThai { get; set; }
            public int? MaCanHo { get; set; }
            public int? MaLoaiHo { get; set; }
        }

        // DELETE: api/HoGiaDinhs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoGiaDinh(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Mã hộ không hợp lệ" });
            
            var hoGiaDinh = await _context.HoGiaDinhs.FindAsync(id);
            if (hoGiaDinh == null) return NotFound(new { message = "Không tìm thấy hộ gia đình với mã: " + id });
            
            try
            {
                // Cập nhật các cư dân thuộc hộ này: set MaHo = null và QuanHeVoiChuHo = N''
                // (Không xóa cư dân, chỉ cập nhật để họ có thể được thêm vào hộ khác sau)
                var updateCuDansSql = @"UPDATE CuDan 
                                       SET MaHo = NULL, 
                                           QuanHeVoiChuHo = N''
                                       WHERE MaHo = @MaHo";
                var paramMaHo = new SqlParameter("@MaHo", id);
                var updatedCuDans = await _context.Database.ExecuteSqlRawAsync(updateCuDansSql, paramMaHo);
                
                // Xóa hộ gia đình
                var deleteHoSql = "DELETE FROM HoGiaDinh WHERE MaHo = @MaHo";
                var deletedHo = await _context.Database.ExecuteSqlRawAsync(deleteHoSql, paramMaHo);
                
                if (deletedHo > 0)
                {
                    return NoContent();
                }
                else
                {
                    return StatusCode(500, new { message = "Không thể xóa hộ gia đình. Có thể đã bị xóa trước đó." });
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                return StatusCode(500, new { message = "Lỗi database khi xóa hộ gia đình: " + sqlEx.Message, errorNumber = sqlEx.Number });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa hộ gia đình: " + ex.Message, innerException = ex.InnerException?.Message });
            }
        }

        // GET: api/HoGiaDinhs/{id}/Members?page=1&pageSize=5
        [HttpGet("{id}/Members")]
        public async Task<ActionResult<object>> GetHoGiaDinhMembers(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var hoGiaDinh = await _context.HoGiaDinhs.FindAsync(id);
            if (hoGiaDinh == null) return NotFound();

            var totalMembers = await _context.CuDans.CountAsync(c => c.MaHo == id);
            var totalPages = (int)Math.Ceiling(totalMembers / (double)pageSize);

            var members = await _context.CuDans
                .Where(c => c.MaHo == id)
                .OrderBy(c => c.QuanHeVoiChuHo == "Chủ hộ" ? 0 : 1)
                .ThenBy(c => c.HoTen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                members = members,
                totalMembers = totalMembers,
                currentPage = page,
                totalPages = totalPages,
                pageSize = pageSize
            });
        }

        private bool HoGiaDinhExists(int id) => _context.HoGiaDinhs.Any(e => e.MaHo == id);
    }
}