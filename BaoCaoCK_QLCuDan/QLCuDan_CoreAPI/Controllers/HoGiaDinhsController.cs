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
            // Include để lấy luôn thông tin Căn hộ và Loại hộ đi kèm
            return await _context.HoGiaDinhs
                .Include(h => h.MaCanHoNavigation) // Lấy số phòng
                .Include(h => h.MaLoaiHoNavigation) // Lấy tên loại hộ
                .ToListAsync();
        }

        // GET: api/HoGiaDinhs/5
        // GET: api/HoGiaDinhs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HoGiaDinh>> GetHoGiaDinh(int id)
        {
            // CŨ (Sai): var hoGiaDinh = await _context.HoGiaDinhs.FindAsync(id);

            // MỚI (Đúng): Phải Include dữ liệu liên quan
            var hoGiaDinh = await _context.HoGiaDinhs
                .Include(h => h.MaCanHoNavigation)  // Lấy thông tin Căn Hộ
                .Include(h => h.MaLoaiHoNavigation) // Lấy thông tin Loại Hộ
                .FirstOrDefaultAsync(h => h.MaHo == id);

            if (hoGiaDinh == null)
            {
                return NotFound();
            }

            return hoGiaDinh;
        }

        // POST: api/HoGiaDinhs
        [HttpPost]
        public async Task<ActionResult<HoGiaDinh>> PostHoGiaDinh(HoGiaDinh hoGiaDinh)
        {
            _context.HoGiaDinhs.Add(hoGiaDinh);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetHoGiaDinh", new { id = hoGiaDinh.MaHo }, hoGiaDinh);
        }

        // PUT: api/HoGiaDinhs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHoGiaDinh(int id, HoGiaDinh hoGiaDinh)
        {
            if (id != hoGiaDinh.MaHo) return BadRequest();
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
            _context.HoGiaDinhs.Remove(hoGiaDinh);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool HoGiaDinhExists(int id) => _context.HoGiaDinhs.Any(e => e.MaHo == id);
    }
}