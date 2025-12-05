using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CanHosController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public CanHosController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // GET: api/CanHos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CanHo>>> GetCanHos()
        {
            // Trả về danh sách căn hộ có trong DB
            return await _context.CanHos.ToListAsync();
        }
    }
}