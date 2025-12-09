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
    public class LoaiHosController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public LoaiHosController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // GET: api/LoaiHos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiHo>>> GetLoaiHos()
        {
            return await _context.LoaiHos.ToListAsync();
        }

        // GET: api/LoaiHos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LoaiHo>> GetLoaiHo(int id)
        {
            var loaiHo = await _context.LoaiHos.FindAsync(id);

            if (loaiHo == null)
            {
                return NotFound();
            }

            return loaiHo;
        }

        // PUT: api/LoaiHos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoaiHo(int id, LoaiHo loaiHo)
        {
            if (id != loaiHo.MaLoaiHo)
            {
                return BadRequest();
            }

            _context.Entry(loaiHo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoaiHoExists(id))
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

        // POST: api/LoaiHos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LoaiHo>> PostLoaiHo(LoaiHo loaiHo)
        {
            _context.LoaiHos.Add(loaiHo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoaiHo", new { id = loaiHo.MaLoaiHo }, loaiHo);
        }

        // DELETE: api/LoaiHos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoaiHo(int id)
        {
            var loaiHo = await _context.LoaiHos.FindAsync(id);
            if (loaiHo == null)
            {
                return NotFound();
            }

            _context.LoaiHos.Remove(loaiHo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoaiHoExists(int id)
        {
            return _context.LoaiHos.Any(e => e.MaLoaiHo == id);
        }
    }
}
