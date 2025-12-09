using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;

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
            if (hoGiaDinh.TrangThai == "Đang ở")
            {
                bool isDuplicate = await _context.HoGiaDinhs
                    .AnyAsync(h => h.MaCanHo == hoGiaDinh.MaCanHo && h.TrangThai == "Đang ở");
                if (isDuplicate) return Conflict(new { message = "Căn hộ này đã có người ở." });
            }
            _context.HoGiaDinhs.Add(hoGiaDinh);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetHoGiaDinh", new { id = hoGiaDinh.MaHo }, hoGiaDinh);
        }

        // PUT: api/HoGiaDinhs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHoGiaDinh(int id, HoGiaDinh hoGiaDinh)
        {
            if (id != hoGiaDinh.MaHo) return BadRequest();
            if (hoGiaDinh.TrangThai == "Đang ở")
            {
                bool isDuplicate = await _context.HoGiaDinhs
                    .AnyAsync(h => h.MaCanHo == hoGiaDinh.MaCanHo && h.TrangThai == "Đang ở" && h.MaHo != id);
                if (isDuplicate) return Conflict(new { message = "Căn hộ này đã có người ở." });
            }
            _context.Entry(hoGiaDinh).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!HoGiaDinhExists(id)) return NotFound(); else throw; }
            return NoContent();
        }

        // DELETE: api/HoGiaDinhs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoGiaDinh(int id)
        {
            var hoGiaDinh = await _context.HoGiaDinhs.FindAsync(id);
            if (hoGiaDinh == null) return NotFound();
            var cuDans = _context.CuDans.Where(c => c.MaHo == id).ToList();
            if (cuDans.Any()) _context.CuDans.RemoveRange(cuDans);
            _context.HoGiaDinhs.Remove(hoGiaDinh);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool HoGiaDinhExists(int id) => _context.HoGiaDinhs.Any(e => e.MaHo == id);
    }
}