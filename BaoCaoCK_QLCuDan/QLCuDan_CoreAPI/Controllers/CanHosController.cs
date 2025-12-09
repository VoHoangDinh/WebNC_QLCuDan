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
        // POST: api/CanHos
        // QLCuDan_CoreAPI/Controllers/CanHosController.cs

        [HttpPost]
        public async Task<ActionResult<CanHo>> PostCanHo(CanHo canHo)
        {
            // 1. Kiểm tra khóa ngoại thủ công (để tránh lỗi 500)
            if (canHo.MaToaNha <= 0)
            {
                return BadRequest("Vui lòng chọn Tòa nhà hợp lệ.");
            }

            try
            {
                // 2. Xóa các Navigation Property để tránh EF Core hiểu nhầm là muốn thêm mới cả Tòa nhà
                canHo.ToaNha = null;
                canHo.HoGiaDinhs = null;

                _context.CanHos.Add(canHo);
                await _context.SaveChangesAsync();

                // 3. Trả về đối tượng đơn giản
                return Ok(canHo);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra để bạn đọc được
                return StatusCode(500, "Lỗi Server: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
            }
        }
        // ... (Các hàm GET và POST ở trên giữ nguyên) ...

        // =========================================================
        // 1. PUT: api/CanHos/5 (Cập nhật thông tin căn hộ)
        // =========================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCanHo(int id, CanHo canHo)
        {
            // Kiểm tra ID trên URL có khớp với ID trong body không
            if (id != canHo.MaCanHo)
            {
                return BadRequest("ID căn hộ không trùng khớp.");
            }

            // Đánh dấu trạng thái là đã sửa đổi
            _context.Entry(canHo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Xử lý lỗi nếu 2 người cùng sửa 1 lúc hoặc dữ liệu bị xóa khi đang sửa
                if (!CanHoExists(id))
                {
                    return NotFound("Căn hộ không tồn tại.");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi Server: " + ex.Message);
            }

            return NoContent(); // Trả về 204 (Thành công nhưng không có nội dung trả về)
        }

        // =========================================================
        // 2. DELETE: api/CanHos/5 (Xóa căn hộ)
        // =========================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCanHo(int id)
        {
            // Tìm căn hộ theo ID
            var canHo = await _context.CanHos.FindAsync(id);
            if (canHo == null)
            {
                return NotFound("Không tìm thấy căn hộ cần xóa.");
            }

            try
            {
                _context.CanHos.Remove(canHo);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Lỗi thường gặp: Xóa căn hộ đang có người ở (dính khóa ngoại bảng HoGiaDinh)
                return StatusCode(500, "Không thể xóa (có thể do ràng buộc dữ liệu): " + ex.Message);
            }

            return NoContent();
        }
        // GET: api/CanHos/5 (Lấy chi tiết 1 căn hộ)
        [HttpGet("{id}")]
        public async Task<ActionResult<CanHo>> GetCanHo(int id)
        {
            var canHo = await _context.CanHos.FindAsync(id);

            if (canHo == null)
            {
                return NotFound();
            }

            return canHo;
        }

        // Hàm phụ: Kiểm tra ID có tồn tại trong DB không
        private bool CanHoExists(int id)
        {
            return _context.CanHos.Any(e => e.MaCanHo == id);
        }
    }
}