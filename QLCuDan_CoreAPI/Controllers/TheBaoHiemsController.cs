using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TheBaoHiemsController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public TheBaoHiemsController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // 1. GET: api/TheBaoHiems
        // Lấy danh sách tất cả thẻ bảo hiểm
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TheBaoHiem>>> GetTheBaoHiems()
        {
            // Include CuDan để lấy luôn tên chủ thẻ khi hiển thị danh sách
            return await _context.TheBaoHiems.Include(t => t.CuDan).ToListAsync();
        }

        // 2. GET: api/TheBaoHiems/5
        // Lấy chi tiết 1 thẻ theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TheBaoHiem>> GetTheBaoHiem(int id)
        {
            var theBaoHiem = await _context.TheBaoHiems
                                           .Include(t => t.CuDan)
                                           .FirstOrDefaultAsync(t => t.MaTheBaoHiem == id);

            if (theBaoHiem == null)
            {
                return NotFound(new { message = "Không tìm thấy thẻ bảo hiểm này." });
            }

            return theBaoHiem;
        }

        // 3. GET: api/TheBaoHiems/ByCuDan/5
        // API phụ: Lấy danh sách thẻ bảo hiểm của 1 cư dân cụ thể (dùng cho App Mobile cư dân)
        [HttpGet("ByCuDan/{maCuDan}")]
        public async Task<ActionResult<IEnumerable<TheBaoHiem>>> GetTheBaoHiemByCuDan(int maCuDan)
        {
            var listThe = await _context.TheBaoHiems
                                        .Where(t => t.MaCuDan == maCuDan)
                                        .ToListAsync();
            return listThe;
        }

        // 4. PUT: api/TheBaoHiems/5
        // Cập nhật thông tin thẻ
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTheBaoHiem(int id, TheBaoHiem theBaoHiem)
        {
            if (id != theBaoHiem.MaTheBaoHiem)
            {
                return BadRequest();
            }

            _context.Entry(theBaoHiem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TheBaoHiemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // 5. POST: api/TheBaoHiems
        // Thêm mới thẻ
        [HttpPost]
        public async Task<ActionResult<TheBaoHiem>> PostTheBaoHiem(TheBaoHiem theBaoHiem)
        {
            // Kiểm tra xem Cư dân có tồn tại không trước khi thêm
            var cuDanExists = await _context.CuDans.AnyAsync(c => c.MaCuDan == theBaoHiem.MaCuDan);
            if (!cuDanExists)
            {
                return BadRequest(new { message = "Mã cư dân không tồn tại." });
            }

            _context.TheBaoHiems.Add(theBaoHiem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTheBaoHiem", new { id = theBaoHiem.MaTheBaoHiem }, theBaoHiem);
        }

        // 6. DELETE: api/TheBaoHiems/5
        // Xóa thẻ
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTheBaoHiem(int id)
        {
            var theBaoHiem = await _context.TheBaoHiems.FindAsync(id);
            if (theBaoHiem == null)
            {
                return NotFound();
            }

            _context.TheBaoHiems.Remove(theBaoHiem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TheBaoHiemExists(int id)
        {
            return _context.TheBaoHiems.Any(e => e.MaTheBaoHiem == id);
        }
    }
}