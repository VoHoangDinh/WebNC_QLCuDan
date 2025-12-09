using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;

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
            // API không cần ValidateAntiForgeryToken (thường dùng JWT hoặc CORS để bảo mật)
            _context.CuDans.Add(cuDan);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCuDan", new { id = cuDan.MaCuDan }, cuDan);
        }

        // PUT: api/CuDans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuDan(int id, CuDan cuDan)
        {
            if (id != cuDan.MaCuDan)
            {
                return BadRequest();
            }

            _context.Entry(cuDan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CuDanExists(id))
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