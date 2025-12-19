using BaoCaoCK_QLCuDan.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class PhanAnhController : Controller
    {
        // Lấy URL từ Web.config
        private string BaseUrl = ConfigurationManager.AppSettings["ApiUrl"];

        // ================== 1. INDEX (XEM DANH SÁCH) ==================
        // Admin vào xem danh sách các phản ánh
        public async Task<ActionResult> Index()
        {
            List<PhanAnh> listPhanAnh = new List<PhanAnh>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Gọi API: GET api/PhanAnhApi
                HttpResponseMessage Res = await client.GetAsync("api/PhanAnhApi");

                if (Res.IsSuccessStatusCode)
                {
                    var responseData = await Res.Content.ReadAsStringAsync();
                    listPhanAnh = JsonConvert.DeserializeObject<List<PhanAnh>>(responseData);
                }
            }
            return View(listPhanAnh);
        }

        // ================== 2. EDIT (XEM CHI TIẾT & DUYỆT) ==================

        // GET: Hiển thị thông tin chi tiết để Admin đọc và chọn trạng thái mới
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            PhanAnh phanAnh = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                // Gọi API lấy chi tiết: GET api/PhanAnhApi/5
                HttpResponseMessage result = await client.GetAsync($"api/PhanAnhApi/{id}");

                if (result.IsSuccessStatusCode)
                {
                    var readTask = await result.Content.ReadAsStringAsync();
                    phanAnh = JsonConvert.DeserializeObject<PhanAnh>(readTask);
                }
                else
                {
                    return HttpNotFound();
                }
            }

            // Tạo danh sách trạng thái để Admin chọn (Duyệt)
            // Lưu ý: Không cần load danh sách Cư Dân nữa vì Admin không được sửa người gửi
            ViewBag.TrangThaiList = new List<string> { "Mới tiếp nhận", "Đang xử lý", "Đã xong" };

            return View(phanAnh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PhanAnh phanAnh)
        {
            // Admin chỉ đổi trạng thái
            // Lưu ý: Dù ModelState có lỗi ở các trường khác (do view hiển thị thiếu), ta vẫn cho qua
            // vì mục đích chỉ là update trạng thái.

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);

                // [QUAN TRỌNG] Thay vì gửi cả object phanAnh to đùng, 
                // ta tạo một object nhỏ chỉ chứa đúng thông tin cần thiết.
                // Điều này giúp API nhận diện chính xác và không bị lỗi Validate các trường khác.
                var dataGuiDi = new
                {
                    TrangThai = phanAnh.TrangThai
                };

                // Gọi API sửa: PUT api/PhanAnhApi/5
                HttpResponseMessage response = await client.PutAsJsonAsync($"api/PhanAnhApi/{phanAnh.MaPhanAnh}", dataGuiDi);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    // Đọc lỗi chi tiết từ API để debug
                    var noiDungLoi = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Lỗi API ({response.StatusCode}): {noiDungLoi}");
                }
            }

            // Nếu lỗi thì load lại view
            ViewBag.TrangThaiList = new List<string> { "Mới tiếp nhận", "Đang xử lý", "Đã xong" };
            return View(phanAnh);
        }
    }
}