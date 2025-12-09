using BaoCaoCK_QLCuDan.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration; // [QUAN TRỌNG] Thư viện để đọc Web.config
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class TheBaoHiemController : Controller
    {
        // Lấy đường dẫn API từ Web.config thay vì hardcode
        private string BaseUrl = ConfigurationManager.AppSettings["ApiUrl"];

        // 1. GET: Index (Lấy danh sách từ API)
        public async Task<ActionResult> Index()
        {
            List<TheBaoHiem> listThe = new List<TheBaoHiem>();

            using (var client = new HttpClient())
            {
                // BaseUrl lấy từ config: "https://localhost:7107/"
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Gọi API: GET api/TheBaoHiems
                HttpResponseMessage Res = await client.GetAsync("api/TheBaoHiems");

                if (Res.IsSuccessStatusCode)
                {
                    var EmpResponse = Res.Content.ReadAsStringAsync().Result;
                    listThe = JsonConvert.DeserializeObject<List<TheBaoHiem>>(EmpResponse);
                }
            }
            return View(listThe);
        }

        // 2. GET: Create (Load danh sách Cư dân để chọn)
        public async Task<ActionResult> Create()
        {
            List<CuDan> listCuDan = new List<CuDan>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Gọi API lấy danh sách cư dân: GET api/CuDans
                // (Đảm bảo bên API bạn đã có Controller CuDans)
                HttpResponseMessage Res = await client.GetAsync("api/CuDans");

                if (Res.IsSuccessStatusCode)
                {
                    var response = Res.Content.ReadAsStringAsync().Result;
                    listCuDan = JsonConvert.DeserializeObject<List<CuDan>>(response);
                }
            }

            // Đổ dữ liệu vào ViewBag để View hiển thị Dropdown
            ViewBag.MaCuDan = new SelectList(listCuDan, "MaCuDan", "HoTen");
            return View();
        }

        // 3. POST: Create (Gửi dữ liệu lên API)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TheBaoHiem theBaoHiem)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Gửi POST request: POST api/TheBaoHiems
                    // Cần cài NuGet: Microsoft.AspNet.WebApi.Client để dùng PostAsJsonAsync
                    HttpResponseMessage response = await client.PostAsJsonAsync("api/TheBaoHiems", theBaoHiem);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi API: " + response.ReasonPhrase);
                    }
                }
            }

            // Nếu lỗi, load lại danh sách cư dân để không bị crash view
            await Create();
            return View(theBaoHiem);
        }
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TheBaoHiem theBaoHiem = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                // Gọi API lấy chi tiết thẻ: GET api/TheBaoHiems/5
                HttpResponseMessage result = await client.GetAsync($"api/TheBaoHiems/{id}");

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    theBaoHiem = JsonConvert.DeserializeObject<TheBaoHiem>(readTask.Result);
                }
                else
                {
                    return HttpNotFound();
                }
            }

            // Load lại danh sách cư dân để hiển thị Dropdown (cần thiết nếu muốn đổi chủ thẻ)
            await LoadCuDanList(theBaoHiem.MaCuDan);
            return View(theBaoHiem);
        }

        // POST: TheBaoHiem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TheBaoHiem theBaoHiem)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseUrl);

                    // Gọi API sửa: PUT api/TheBaoHiems/5
                    HttpResponseMessage response = await client.PutAsJsonAsync($"api/TheBaoHiems/{theBaoHiem.MaTheBaoHiem}", theBaoHiem);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi API Update: " + response.StatusCode);
                    }
                }
            }

            await LoadCuDanList(theBaoHiem.MaCuDan);
            return View(theBaoHiem);
        }

        // ================== CHỨC NĂNG XÓA (DELETE) ==================

        // GET: TheBaoHiem/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TheBaoHiem theBaoHiem = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                // Gọi API lấy thông tin để hiển thị xác nhận xóa
                HttpResponseMessage result = await client.GetAsync($"api/TheBaoHiems/{id}");

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();
                    theBaoHiem = JsonConvert.DeserializeObject<TheBaoHiem>(readTask.Result);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(theBaoHiem);
        }

        // POST: TheBaoHiem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);

                // Gọi API xóa: DELETE api/TheBaoHiems/5
                HttpResponseMessage response = await client.DeleteAsync($"api/TheBaoHiems/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error"); // Hoặc xử lý lỗi tùy ý
                }
            }
        }

        // Hàm phụ trợ để load danh sách cư dân (tránh lặp code)
        private async Task LoadCuDanList(int? selectedId = null)
        {
            List<CuDan> listCuDan = new List<CuDan>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                HttpResponseMessage Res = await client.GetAsync("api/CuDans");
                if (Res.IsSuccessStatusCode)
                {
                    var response = Res.Content.ReadAsStringAsync().Result;
                    listCuDan = JsonConvert.DeserializeObject<List<CuDan>>(response);
                }
            }
            ViewBag.MaCuDan = new SelectList(listCuDan, "MaCuDan", "HoTen", selectedId);
        }
    }
}