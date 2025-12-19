using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BaoCaoCK_QLCuDan.Models;
using Newtonsoft.Json;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class CanHoController : Controller
    {
        // Địa chỉ API (chú ý port cho đúng với project API của bạn)
        private string apiBaseUrl = "https://localhost:7107/api";

        // 1. DANH SÁCH CĂN HỘ
        public async Task<ActionResult> Index()
        {
            List<CanHo> listCanHo = new List<CanHo>();
            using (var client = new HttpClient())
            {
                // Gọi API lấy danh sách
                var response = await client.GetAsync(apiBaseUrl + "/CanHos");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    listCanHo = JsonConvert.DeserializeObject<List<CanHo>>(data);
                }
            }
            return View(listCanHo);
        }

        // 2. TẠO MỚI (Giao diện)
        public async Task<ActionResult> Create()
        {
            await LoadToaNhaDropdown(); // Load danh sách tòa nhà để chọn
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CanHo model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    // Serialize dữ liệu
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                    // CHỈ GỬI ĐẾN "/CanHos" (Số nhiều, không có ID phía sau)
                    var response = await client.PostAsync(apiBaseUrl + "/CanHos", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Ghi log lỗi ra để dễ sửa
                        ModelState.AddModelError("", "Lỗi API: " + response.StatusCode + " - " + response.ReasonPhrase);
                    }
                }
            }
            await LoadToaNhaDropdown();
            return View(model);
        }
        // ... (Các hàm Index, Create cũ giữ nguyên) ...

        // ==========================================
        // 3. EDIT: Hiển thị form sửa (GET)
        // ==========================================
        public async Task<ActionResult> Edit(int id)
        {
            CanHo model = null;
            using (var client = new HttpClient())
            {
                // Gọi API lấy thông tin chi tiết căn hộ
                var response = await client.GetAsync(apiBaseUrl + "/CanHos/" + id);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<CanHo>(data);
                }
            }

            // Nếu không tìm thấy căn hộ -> Báo lỗi 404
            if (model == null) return HttpNotFound();

            // Load lại dropdown Tòa nhà để người dùng chọn lại nếu muốn
            await LoadToaNhaDropdown();

            return View(model);
        }

        // ==========================================
        // 4. EDIT: Lưu thông tin sửa (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CanHo model)
        {
            // Kiểm tra ID trên URL và ID trong form có khớp không
            if (id != model.MaCanHo)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                    // Gọi API PUT: api/CanHos/{id}
                    var response = await client.PutAsync(apiBaseUrl + "/CanHos/" + id, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Lỗi cập nhật: " + response.ReasonPhrase);
                    }
                }
            }

            // Nếu lỗi validation, load lại dropdown và hiện lại form
            await LoadToaNhaDropdown();
            return View(model);
        }

        // 4. XÓA CĂN HỘ
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(apiBaseUrl + "/CanHos/" + id);
                return RedirectToAction("Index");
            }
        }

        // Hàm phụ: Load danh sách tòa nhà từ API
        private async Task LoadToaNhaDropdown()
        {
            List<ToaNha> listToaNha = new List<ToaNha>();
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(apiBaseUrl + "/ToaNhas");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        listToaNha = JsonConvert.DeserializeObject<List<ToaNha>>(data);
                    }
                }
            }
            catch { /* Bỏ qua lỗi nếu API chưa chạy */ }

            ViewBag.MaToaNha = new SelectList(listToaNha, "MaToaNha", "TenToaNha");
        }
    }
}