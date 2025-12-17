using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using QLCuDan_CoreAPI.Models;

namespace QLCuDan_CoreAPI.Controllers
{
    // 1. Định nghĩa đường dẫn là: api/CuDans
    [Route("api/[controller]")]
    [ApiController]
    // 2. Kế thừa ControllerBase
    public class CuDansController : ControllerBase
    {
        private readonly QuanLyChungCuDbContext _context;

        public CuDansController(QuanLyChungCuDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // SỬA ĐOẠN NÀY: GET api/CuDans (Hỗ trợ Tìm kiếm & Phân trang)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> GetCuDans(string? keyword, int page = 1, int pageSize = 10)
        {
            // 1. Khởi tạo truy vấn
            var query = _context.CuDans.AsQueryable();

            // 2. Tìm kiếm (Nếu có từ khóa)
            if (!string.IsNullOrEmpty(keyword))
            {
                // Tìm theo Tên, SĐT hoặc Email
                query = query.Where(c => c.HoTen.Contains(keyword) ||
                                         c.SDT.Contains(keyword) ||
                                         c.Email.Contains(keyword));
            }

            // 3. Đếm tổng số kết quả (quan trọng để tính số trang)
            int totalItems = await query.CountAsync();

            // 4. Phân trang: Bỏ qua (Skip) các trang trước và Lấy (Take) số lượng trang hiện tại
            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            // 5. Trả về cấu trúc JSON mới gồm cả dữ liệu và thông tin phân trang
            return Ok(new
            {
                TotalItems = totalItems, // Tổng số bản ghi tìm thấy
                Page = page,             // Trang hiện tại
                PageSize = pageSize,     // Kích thước trang
                Items = items            // Danh sách cư dân của trang này
            });
        }
        // =========================================================================

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
            // Sử dụng raw SQL để INSERT vì bảng CuDan có trigger
            // SQL Server không cho phép OUTPUT clause với trigger
            var sql = @"
                INSERT INTO CuDan (HoTen, NgaySinh, GioiTinh, SDT, Email, Avatar, 
                                  TrinhDoHocVan, NgayVaoDang, NgayVaoDoan, HocHamHocVi, 
                                  NhanDang_Cao, NhanDang_SongMui, DauVetDacBiet, 
                                  QuanHeVoiChuHo, MaHo)
                VALUES (@HoTen, @NgaySinh, @GioiTinh, @SDT, @Email, @Avatar, 
                        @TrinhDoHocVan, @NgayVaoDang, @NgayVaoDoan, @HocHamHocVi, 
                        @NhanDang_Cao, @NhanDang_SongMui, @DauVetDacBiet, 
                        @QuanHeVoiChuHo, @MaHo);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new[]
            {
                new SqlParameter("@HoTen", cuDan.HoTen ?? (object)DBNull.Value),
                new SqlParameter("@NgaySinh", cuDan.NgaySinh ?? (object)DBNull.Value),
                new SqlParameter("@GioiTinh", cuDan.GioiTinh ?? (object)DBNull.Value),
                new SqlParameter("@SDT", cuDan.SDT ?? (object)DBNull.Value),
                new SqlParameter("@Email", cuDan.Email ?? (object)DBNull.Value),
                new SqlParameter("@Avatar", cuDan.Avatar ?? (object)DBNull.Value),
                new SqlParameter("@TrinhDoHocVan", cuDan.TrinhDoHocVan ?? (object)DBNull.Value),
                new SqlParameter("@NgayVaoDang", cuDan.NgayVaoDang ?? (object)DBNull.Value),
                new SqlParameter("@NgayVaoDoan", cuDan.NgayVaoDoan ?? (object)DBNull.Value),
                new SqlParameter("@HocHamHocVi", cuDan.HocHamHocVi ?? (object)DBNull.Value),
                new SqlParameter("@NhanDang_Cao", cuDan.NhanDang_Cao ?? (object)DBNull.Value),
                new SqlParameter("@NhanDang_SongMui", cuDan.NhanDang_SongMui ?? (object)DBNull.Value),
                new SqlParameter("@DauVetDacBiet", cuDan.DauVetDacBiet ?? (object)DBNull.Value),
                new SqlParameter("@QuanHeVoiChuHo", cuDan.QuanHeVoiChuHo ?? (object)DBNull.Value),
                new SqlParameter("@MaHo", cuDan.MaHo ?? (object)DBNull.Value)
            };

            // Thực thi SQL và lấy ID mới
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;
            
            if (!wasOpen)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters);
                
                var newId = await command.ExecuteScalarAsync();
                if (newId != null && newId != DBNull.Value)
                {
                    cuDan.MaCuDan = Convert.ToInt32(newId);
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }

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

            // Kiểm tra xem cư dân có tồn tại không
            if (!CuDanExists(id))
            {
                return NotFound();
            }

            // Sử dụng raw SQL để UPDATE vì bảng CuDan có trigger
            // SQL Server không cho phép OUTPUT clause với trigger
            var sql = @"
                UPDATE CuDan 
                SET HoTen = @HoTen,
                    NgaySinh = @NgaySinh,
                    GioiTinh = @GioiTinh,
                    SDT = @SDT,
                    Email = @Email,
                    Avatar = @Avatar,
                    TrinhDoHocVan = @TrinhDoHocVan,
                    NgayVaoDang = @NgayVaoDang,
                    NgayVaoDoan = @NgayVaoDoan,
                    HocHamHocVi = @HocHamHocVi,
                    NhanDang_Cao = @NhanDang_Cao,
                    NhanDang_SongMui = @NhanDang_SongMui,
                    DauVetDacBiet = @DauVetDacBiet,
                    QuanHeVoiChuHo = @QuanHeVoiChuHo,
                    MaHo = @MaHo
                WHERE MaCuDan = @MaCuDan;";

            var parameters = new[]
            {
                new SqlParameter("@MaCuDan", cuDan.MaCuDan),
                new SqlParameter("@HoTen", cuDan.HoTen ?? (object)DBNull.Value),
                new SqlParameter("@NgaySinh", cuDan.NgaySinh ?? (object)DBNull.Value),
                new SqlParameter("@GioiTinh", cuDan.GioiTinh ?? (object)DBNull.Value),
                new SqlParameter("@SDT", cuDan.SDT ?? (object)DBNull.Value),
                new SqlParameter("@Email", cuDan.Email ?? (object)DBNull.Value),
                new SqlParameter("@Avatar", cuDan.Avatar ?? (object)DBNull.Value),
                new SqlParameter("@TrinhDoHocVan", cuDan.TrinhDoHocVan ?? (object)DBNull.Value),
                new SqlParameter("@NgayVaoDang", cuDan.NgayVaoDang ?? (object)DBNull.Value),
                new SqlParameter("@NgayVaoDoan", cuDan.NgayVaoDoan ?? (object)DBNull.Value),
                new SqlParameter("@HocHamHocVi", cuDan.HocHamHocVi ?? (object)DBNull.Value),
                new SqlParameter("@NhanDang_Cao", cuDan.NhanDang_Cao ?? (object)DBNull.Value),
                new SqlParameter("@NhanDang_SongMui", cuDan.NhanDang_SongMui ?? (object)DBNull.Value),
                new SqlParameter("@DauVetDacBiet", cuDan.DauVetDacBiet ?? (object)DBNull.Value),
                new SqlParameter("@QuanHeVoiChuHo", cuDan.QuanHeVoiChuHo ?? (object)DBNull.Value),
                new SqlParameter("@MaHo", cuDan.MaHo ?? (object)DBNull.Value)
            };

            // Thực thi SQL
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;
            
            if (!wasOpen)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters);
                
                var rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected == 0)
                {
                    return NotFound();
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }

            return NoContent();
        }

        // DELETE: api/CuDans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuDan(int id)
        {
            // Kiểm tra xem cư dân có tồn tại không
            if (!CuDanExists(id))
            {
                return NotFound();
            }

            // Sử dụng raw SQL để DELETE vì bảng CuDan có trigger
            // SQL Server không cho phép OUTPUT clause với trigger
            var sql = "DELETE FROM CuDan WHERE MaCuDan = @MaCuDan;";

            var parameter = new SqlParameter("@MaCuDan", id);

            // Thực thi SQL
            var connection = _context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;
            
            if (!wasOpen)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.Add(parameter);
                
                var rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected == 0)
                {
                    return NotFound();
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }

            return NoContent();
        }

        private bool CuDanExists(int id)
        {
            return _context.CuDans.Any(e => e.MaCuDan == id);
        }
    }
}