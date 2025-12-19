using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToaNhasController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public ToaNhasController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // GET: api/ToaNhas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToaNha>>> GetToaNhas()
        {
            return await _context.ToaNhas.ToListAsync();
        }
    }
}