using BaoCaoCK_QLCuDan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web; // Thư viện để dùng HttpPostedFileBase
using System.Web.Mvc;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class CuDanController : Controller
    {
        // 1. Link API
        private const string BaseUrl = "https://localhost:7107/";

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        // ==========================================
        // 1. LẤY DANH SÁCH
        // ==========================================
        public async Task<ActionResult> Index()
        {
            List<CuDan> listCuDan = new List<CuDan>();
            try
            {
                using (var client = CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync("api/CuDans");
                    if (response.IsSuccessStatusCode)
                    {
                        listCuDan = await response.Content.ReadAsAsync<List<CuDan>>();
                    }
                }
            }
            catch (Exception) { /* Bỏ qua lỗi để hiện trang trống nếu API chết */ }
            return View(listCuDan);
        }

        // ==========================================
        // 2. XEM CHI TIẾT
        // ==========================================
        public async Task<ActionResult> Details(int id)
        {
            CuDan cuDan = null;
            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync("api/CuDans/" + id);
                if (response.IsSuccessStatusCode)
                {
                    cuDan = await response.Content.ReadAsAsync<CuDan>();
                }
            }
            if (cuDan == null) return HttpNotFound();
            return View(cuDan);
        }

        // ==========================================
        // 3. THÊM MỚI (Đã thêm lại phần xử lý ảnh)
        // ==========================================
        public ActionResult Create()
        {
            LoadDropdownHoGiaDinh();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số ImageFile để nhận file từ View
        public async Task<ActionResult> Create(CuDan cuDan, HttpPostedFileBase ImageFile)
        {
            // --- XỬ LÝ ẢNH (Lưu vào thư mục MVC) ---
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                // 1. Lấy tên file
                string fileName = System.IO.Path.GetFileName(ImageFile.FileName);

                // 2. Đường dẫn lưu (Content/Images)
                string uploadPath = Server.MapPath("~/Content/Images/");

                // Tạo thư mục nếu chưa có
                if (!System.IO.Directory.Exists(uploadPath))
                {
                    System.IO.Directory.CreateDirectory(uploadPath);
                }

                // 3. Lưu file
                string filePath = System.IO.Path.Combine(uploadPath, fileName);
                ImageFile.SaveAs(filePath);

                // 4. Gán đường dẫn vào Model để gửi sang API
                cuDan.Avatar = "/Content/Images/" + fileName;
            }
            else
            {
                // Nếu không chọn ảnh -> Lấy ảnh mặc định
                cuDan.Avatar = "/Content/Images/default.jpg";
            }
            // ---------------------------------------

            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync("api/CuDans", cuDan);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Lỗi API: " + response.ReasonPhrase);
            }

            LoadDropdownHoGiaDinh();
            return View(cuDan);
        }

        // ==========================================
        // 4. CẬP NHẬT (Đã thêm lại phần xử lý ảnh)
        // ==========================================
        public async Task<ActionResult> Edit(int id)
        {
            CuDan cuDan = null;
            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync("api/CuDans/" + id);
                if (response.IsSuccessStatusCode)
                {
                    cuDan = await response.Content.ReadAsAsync<CuDan>();
                }
            }
            if (cuDan == null) return HttpNotFound();
            LoadDropdownHoGiaDinh();
            return View(cuDan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số ImageFile
        public async Task<ActionResult> Edit(int id, CuDan cuDan, HttpPostedFileBase ImageFile)
        {
            // --- XỬ LÝ ẢNH ---
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                // Nếu người dùng chọn ảnh mới -> Lưu và cập nhật đường dẫn mới
                string fileName = System.IO.Path.GetFileName(ImageFile.FileName);
                string uploadPath = Server.MapPath("~/Content/Images/");
                string filePath = System.IO.Path.Combine(uploadPath, fileName);
                ImageFile.SaveAs(filePath);
                cuDan.Avatar = "/Content/Images/" + fileName;
            }
            // Nếu ImageFile == null, thì cuDan.Avatar vẫn giữ giá trị cũ (do View gửi lên qua HiddenField)
            // -----------------

            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.PutAsJsonAsync("api/CuDans/" + id, cuDan);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            LoadDropdownHoGiaDinh();
            return View(cuDan);
        }

        // ==========================================
        // 5. XÓA
        // ==========================================
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (var client = CreateClient())
            {
                await client.DeleteAsync("api/CuDans/" + id);
            }
            return RedirectToAction("Index");
        }

        // Hàm phụ
        private void LoadDropdownHoGiaDinh()
        {
            try
            {
                using (var db = new QuanLyCuDanContext())
                {
                    var list = db.HoGiaDinhs.ToList();
                    if (list.Count == 0)
                        ViewBag.MaHo = new SelectList(new List<object> { new { MaHo = 0, MaHoHienThi = "Trống" } }, "MaHo", "MaHoHienThi");
                    else
                        ViewBag.MaHo = new SelectList(list, "MaHo", "MaHo");
                }
            }
            catch
            {
                ViewBag.MaHo = new SelectList(new List<string>());
            }
        }
    }
}