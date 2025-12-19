using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models; // Namespace Model của bạn

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhanAnhApiController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public PhanAnhApiController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // GET: api/PhanAnhApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhanAnh>>> GetPhanAnhs()
        {
            return await _context.PhanAnhs
                .Include(p => p.CuDan)
                .OrderByDescending(p => p.NgayGui)
                .ToListAsync();
        }

        // GET: api/PhanAnhApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PhanAnh>> GetPhanAnh(int id)
        {
            var phanAnh = await _context.PhanAnhs
                .Include(p => p.CuDan) // Sửa lỗi tương tự ở đây
                .FirstOrDefaultAsync(p => p.MaPhanAnh == id);

            if (phanAnh == null) return NotFound();

            return phanAnh;
        }

        // PUT: api/PhanAnhApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhanAnh(int id, [FromBody] PhanAnh phanAnh)
        {
            // 1. [QUAN TRỌNG] Gán cứng ID từ URL vào Object để đảm bảo luôn khớp
            // Việc này giúp vượt qua lỗi "BadRequest" do id không trùng khớp
            phanAnh.MaPhanAnh = id;

            // 2. Tìm bản ghi cũ trong Database
            var existingPhanAnh = await _context.PhanAnhs.FindAsync(id);
            if (existingPhanAnh == null) return NotFound();

            // 3. Chỉ cập nhật cột Trạng Thái (Bảo toàn các dữ liệu khác)
            // Admin chỉ được duyệt, không được sửa nội dung của dân
            existingPhanAnh.TrangThai = phanAnh.TrangThai;

            // Nếu muốn chắc chắn, có thể cập nhật thêm ngày xử lý nếu có cột đó
            // existingPhanAnh.NgayXuLy = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhanAnhExists(id)) return NotFound();
                else throw;
            }

            return NoContent(); // Trả về 204 (Thành công)
        }

        private bool PhanAnhExists(int id)
        {
            // Kiểm tra tên DbSet (PhanAnh hay PhanAnhs)
            return _context.PhanAnhs.Any(e => e.MaPhanAnh == id);
        }
    }
}