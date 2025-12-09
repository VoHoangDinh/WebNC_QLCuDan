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
    public class HoGiaDinhController : Controller
    {
        private string apiBaseUrl = "https://localhost:7107/api";

        // 1. DANH SÁCH (INDEX)
        public async Task<ActionResult> Index()
        {
            List<HoGiaDinh> listHo = new List<HoGiaDinh>();
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    listHo = JsonConvert.DeserializeObject<List<HoGiaDinh>>(data);
                }
            }
            return View(listHo);
        }

        // 2. XEM CHI TIẾT (DETAILS)
        public async Task<ActionResult> Details(int id)
        {
            HoGiaDinh model = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + id);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<HoGiaDinh>(data);
                }
            }
            return View(model);
        }

        // 3. TẠO MỚI (CREATE)
        public async Task<ActionResult> Create()
        {
            await LoadViewBag(); // Load danh sách Căn hộ, Loại hộ
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(HoGiaDinh model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiBaseUrl + "/HoGiaDinhs", content);
                    if (response.IsSuccessStatusCode) return RedirectToAction("Index");
                }
            }
            await LoadViewBag();
            return View(model);
        }

        // 4. CHỈNH SỬA (EDIT)
        public async Task<ActionResult> Edit(int id)
        {
            HoGiaDinh model = null;
            using (var client = new HttpClient())
            {
                // Lấy thông tin cũ
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + id);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<HoGiaDinh>(data);
                }
            }
            await LoadViewBag();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(HoGiaDinh model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(apiBaseUrl + "/HoGiaDinhs/" + model.MaHo, content);
                    if (response.IsSuccessStatusCode) return RedirectToAction("Index");
                }
            }
            await LoadViewBag();
            return View(model);
        }

        // 5. XÓA (DELETE)
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(apiBaseUrl + "/HoGiaDinhs/" + id);
                // Nếu thành công hoặc lỗi đều quay về Index
                return RedirectToAction("Index");
            }
        }

        // --- HÀM PHỤ: Lấy dữ liệu Dropdown từ API ---
        // Hàm phụ: Lấy dữ liệu Dropdown an toàn
        private async Task LoadViewBag()
        {
            // 1. Khởi tạo list trống mặc định (để tránh lỗi Null Reference)
            List<CanHo> listCanHo = new List<CanHo>();
            List<LoaiHo> listLoai = new List<LoaiHo>();

            try
            {
                using (var client = new HttpClient())
                {
                    // Set timeout để không treo quá lâu nếu API chưa bật
                    client.Timeout = TimeSpan.FromSeconds(5);

                    // --- Lấy Căn hộ ---
                    var resCanHo = await client.GetAsync(apiBaseUrl + "/CanHos");
                    if (resCanHo.IsSuccessStatusCode)
                    {
                        var data = await resCanHo.Content.ReadAsStringAsync();
                        listCanHo = JsonConvert.DeserializeObject<List<CanHo>>(data) ?? new List<CanHo>();
                    }

                    // --- Lấy Loại Hộ ---
                    var resLoai = await client.GetAsync(apiBaseUrl + "/LoaiHos");
                    if (resLoai.IsSuccessStatusCode)
                    {
                        var data = await resLoai.Content.ReadAsStringAsync();
                        listLoai = JsonConvert.DeserializeObject<List<LoaiHo>>(data) ?? new List<LoaiHo>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu lỗi kết nối API, ghi log hoặc bỏ qua
                ModelState.AddModelError("", "Không kết nối được với API: " + ex.Message);
            }

            // Đổ vào ViewBag (Nếu list rỗng thì Dropdown sẽ rỗng chứ không sập web)
            ViewBag.MaCanHo = new SelectList(listCanHo, "MaCanHo", "SoPhong");
            ViewBag.MaLoaiHo = new SelectList(listLoai, "MaLoaiHo", "TenLoai");
        }
    }
}