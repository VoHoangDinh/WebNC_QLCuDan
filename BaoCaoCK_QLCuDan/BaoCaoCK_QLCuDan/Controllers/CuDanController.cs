using BaoCaoCK_QLCuDan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web; // Thư viện để dùng HttpPostedFileBase
using System.Web.Mvc;
using PagedList;
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
        // SỬA HÀM INDEX NHƯ SAU:
        public async Task<ActionResult> Index(string keyword, int? page)
        {
            int pageSize = 5; // Số dòng mỗi trang
            int pageNumber = (page ?? 1); // Trang mặc định là 1

            // Tạo danh sách trống mặc định
            IPagedList<CuDan> pagedList = new StaticPagedList<CuDan>(new List<CuDan>(), pageNumber, pageSize, 0);

            try
            {
                using (var client = CreateClient())
                {
                    // Gọi API kèm tham số tìm kiếm và phân trang
                    // Link sẽ thành: api/CuDans?keyword=abc&page=1&pageSize=5
                    string url = $"api/CuDans?keyword={keyword}&page={pageNumber}&pageSize={pageSize}";

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Đọc dữ liệu JSON vào class PhanTrangCuDan
                        var result = await response.Content.ReadAsAsync<PhanTrangCuDan>();

                        // Tạo đối tượng PagedList để đưa ra View
                        pagedList = new StaticPagedList<CuDan>(result.Items, pageNumber, pageSize, result.TotalItems);
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi
            }

            // Giữ lại từ khóa tìm kiếm để hiện lại trên ô input
            ViewBag.CurrentFilter = keyword;

            return View(pagedList);
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
        public async Task<ActionResult> Create(CuDan cuDan, HttpPostedFileBase ImageFile)
        {
            // --- 1. XỬ LÝ ẢNH (Giữ nguyên) ---
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(ImageFile.FileName);
                string uploadPath = Server.MapPath("~/Content/Images/");
                if (!System.IO.Directory.Exists(uploadPath))
                    System.IO.Directory.CreateDirectory(uploadPath);

                string filePath = System.IO.Path.Combine(uploadPath, fileName);
                ImageFile.SaveAs(filePath);
                cuDan.Avatar = "/Content/Images/" + fileName;
            }
            else
            {
                cuDan.Avatar = "/Content/Images/default.jpg";
            }

            // --- 2. QUAN TRỌNG: ÉP MÃ HỘ BẰNG NULL ---
            // Để API không bị lỗi khóa ngoại (vì chưa có hộ nào mã 0)
            cuDan.MaHo = null;

            // Xóa HoGiaDinh ảo để tránh lỗi vòng lặp khi gửi JSON
            cuDan.HoGiaDinh = null;

            // --- 3. GỬI SANG API ---
            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.PostAsJsonAsync("api/CuDans", cuDan);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    // Đọc lỗi chi tiết từ API để biết tại sao sai
                    string errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Lỗi API (" + response.StatusCode + "): " + errorContent);
                }
            }

            // Nếu lỗi thì quay lại form
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
        // --- HÀM PHỤ ĐỂ LOAD DROPDOWN (SỬA LẠI) ---
        private void LoadDropdownHoGiaDinh()
        {
            try
            {
                using (var db = new QuanLyCuDanContext())
                {
                    // Lấy danh sách hộ gia đình
                    // Kết hợp Mã Hộ + Tên Chủ Hộ để hiển thị cho dễ nhìn (VD: "1 - Nguyễn Văn A")
                    var list = db.HoGiaDinhs
                        .Select(h => new
                        {
                            MaHo = h.MaHo,
                            // Nếu chưa có tên chủ hộ (hộ mới) thì hiện "Chưa có chủ hộ"
                            HienThi = h.MaHo.ToString() + " - " + (string.IsNullOrEmpty(h.TenChuHo) ? "Chưa có chủ hộ" : h.TenChuHo)
                        })
                        .ToList();

                    if (list.Count == 0)
                    {
                        ModelState.AddModelError("", "Cảnh báo: Bảng Hộ Gia Đình đang trống!");
                        ViewBag.MaHo = new SelectList(new List<object> { new { MaHo = 0, HienThi = "Trống" } }, "MaHo", "HienThi");
                    }
                    else
                    {
                        // Chọn trường "HienThi" làm text, "MaHo" làm value
                        ViewBag.MaHo = new SelectList(list, "MaHo", "HienThi");
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra console để debug nếu cần
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ViewBag.MaHo = new SelectList(new List<string>());
            }
        }
    }
}