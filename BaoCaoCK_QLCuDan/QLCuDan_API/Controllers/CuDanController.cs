using QLCuDan_API.Models; // Namespace trỏ đến thư mục Models
using System;
using System.Linq;
using System.Web; // Để dùng HttpContext nếu cần xử lý file
using System.Web.Http; // QUAN TRỌNG: Thư viện dành cho API

namespace QLCuDan_API.Controllers
{
    // Kế thừa ApiController (thay vì Controller thường)
    public class CuDanController : ApiController
    {
        private QuanLyCuDanContext db = new QuanLyCuDanContext();

        public CuDanController()
        {
            // BẮT BUỘC: Ngắt chuỗi quan hệ để tránh lỗi vòng lặp khi trả về JSON
            db.Configuration.ProxyCreationEnabled = false;
        }

        // 1. GET: api/cudan (Lấy danh sách)
        // Thay thế cho hàm Index() bên MVC
        [HttpGet]
        public IHttpActionResult Get()
        {
            var list = db.CuDans.ToList();
            return Ok(list); // Trả về JSON danh sách
        }

        // 2. GET: api/cudan/5 (Lấy chi tiết)
        // Thay thế cho hàm Details(id) bên MVC
        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            var cuDan = db.CuDans.Find(id);
            if (cuDan == null) return NotFound(); // Trả về mã 404
            return Ok(cuDan);
        }

        // 3. POST: api/cudan (Thêm mới)
        // Thay thế cho hàm Create(POST) bên MVC
        [HttpPost]
        public IHttpActionResult Post(CuDan cuDan)
        {
            // Lưu ý: Upload file qua API phức tạp hơn MVC.
            // Tạm thời code này nhận JSON thông tin cư dân trước.

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Gán ảnh mặc định nếu chưa có
            if (string.IsNullOrEmpty(cuDan.Avatar))
            {
                cuDan.Avatar = "/Content/Images/default.jpg";
            }

            db.CuDans.Add(cuDan);
            db.SaveChanges();

            return Ok(cuDan); // Trả về đối tượng vừa tạo kèm ID mới
        }

        // 4. PUT: api/cudan/5 (Cập nhật)
        // Thay thế cho hàm Edit(POST) bên MVC
        [HttpPut]
        public IHttpActionResult Put(int id, CuDan cuDanMoi)
        {
            var cuDanCu = db.CuDans.Find(id);
            if (cuDanCu == null) return NotFound();

            // Cập nhật từng trường dữ liệu
            cuDanCu.HoTen = cuDanMoi.HoTen;
            cuDanCu.SDT = cuDanMoi.SDT;
            cuDanCu.Email = cuDanMoi.Email;
            cuDanCu.GioiTinh = cuDanMoi.GioiTinh;
            cuDanCu.TrinhDoHocVan = cuDanMoi.TrinhDoHocVan;
            cuDanCu.NgayVaoDang = cuDanMoi.NgayVaoDang;
            cuDanCu.NgayVaoDoan = cuDanMoi.NgayVaoDoan;
            cuDanCu.NhanDang_Cao = cuDanMoi.NhanDang_Cao;
            cuDanCu.NhanDang_SongMui = cuDanMoi.NhanDang_SongMui;
            cuDanCu.DauVetDacBiet = cuDanMoi.DauVetDacBiet;

            // Nếu có gửi link ảnh mới thì cập nhật
            if (!string.IsNullOrEmpty(cuDanMoi.Avatar))
            {
                cuDanCu.Avatar = cuDanMoi.Avatar;
            }

            db.SaveChanges();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 5. DELETE: api/cudan/5 (Xóa)
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var cuDan = db.CuDans.Find(id);
            if (cuDan == null) return NotFound();

            db.CuDans.Remove(cuDan);
            db.SaveChanges();
            return Ok(new { message = "Đã xóa thành công!" });
        }
    }
}