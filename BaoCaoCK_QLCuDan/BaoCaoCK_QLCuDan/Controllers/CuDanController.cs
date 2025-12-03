using BaoCaoCK_QLCuDan.Models; // Đảm bảo đúng namespace project của bạn
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class CuDanController : Controller
    {
        // 1. Gọi kết nối Database
        private QuanLyCuDanContext db = new QuanLyCuDanContext();

        // 2. Action hiển thị chi tiết (Giống trong ảnh)
        // Đường dẫn chạy sẽ là: /CuDan/Details/1
        public ActionResult Details(int id)
        {
            // Tìm cư dân có mã số id
            var cuDan = db.CuDans.Find(id);

            // Nếu không tìm thấy thì báo lỗi 404
            if (cuDan == null)
            {
                return HttpNotFound();
            }

            // Trả về View kèm dữ liệu cư dân tìm được
            return View(cuDan);
        }
        // ... (Đoạn trên là hàm Details, đừng xóa nhé)

        // 3. GET: Hiển thị form cập nhật
        public ActionResult Edit(int id)
        {
            var cuDan = db.CuDans.Find(id);
            if (cuDan == null)
            {
                return HttpNotFound();
            }
            // Truyền danh sách Hộ gia đình sang View để chọn (nếu cần đổi hộ)
            ViewBag.MaHo = new SelectList(db.HoGiaDinhs, "MaHo", "MaHo", cuDan.MaHo);
            return View(cuDan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số ImageFile vào đây để nhận file từ View
        public ActionResult Edit(CuDan cuDan, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                var dataCu = db.CuDans.Find(cuDan.MaCuDan);

                if (dataCu != null)
                {
                    // --- BẮT ĐẦU XỬ LÝ ẢNH ---
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        // 1. Lấy tên file
                        string fileName = System.IO.Path.GetFileName(ImageFile.FileName);

                        // 2. Tạo đường dẫn lưu file trên Server
                        string uploadPath = Server.MapPath("~/Content/Images/");
                        string filePath = System.IO.Path.Combine(uploadPath, fileName);

                        // 3. Lưu file vào thư mục Content/Images
                        ImageFile.SaveAs(filePath);

                        // 4. Cập nhật đường dẫn vào Database
                        dataCu.Avatar = "/Content/Images/" + fileName;
                    }
                    // Nếu không chọn ảnh mới thì giữ nguyên ảnh cũ (không làm gì cả)
                    // --- KẾT THÚC XỬ LÝ ẢNH ---

                    // Cập nhật các thông tin khác
                    dataCu.HoTen = cuDan.HoTen;
                    dataCu.SDT = cuDan.SDT;
                    dataCu.Email = cuDan.Email;
                    dataCu.TrinhDoHocVan = cuDan.TrinhDoHocVan;
                    dataCu.NgayVaoDang = cuDan.NgayVaoDang;
                    dataCu.NgayVaoDoan = cuDan.NgayVaoDoan;
                    dataCu.NhanDang_Cao = cuDan.NhanDang_Cao;
                    dataCu.NhanDang_SongMui = cuDan.NhanDang_SongMui;
                    dataCu.DauVetDacBiet = cuDan.DauVetDacBiet;

                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = cuDan.MaCuDan });
                }
            }
            return View(cuDan);
        }
    }
}