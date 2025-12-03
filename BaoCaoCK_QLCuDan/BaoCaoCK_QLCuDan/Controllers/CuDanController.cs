using BaoCaoCK_QLCuDan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class CuDanController : Controller
    {
        // --- SỬA LẠI: Chỉ để 1 đường dẫn duy nhất ---
        private const string BaseUrl = "http://localhost:5000/";

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        // --- 1. LẤY DANH SÁCH ---
        public async Task<ActionResult> Index()
        {
            List<CuDan> listCuDan = new List<CuDan>();
            try
            {
                using (var client = CreateClient())
                {
                    // SỬA: Thêm chữ 's' vào sau CuDan -> api/CuDans (để khớp với Controller bên API)
                    HttpResponseMessage response = await client.GetAsync("api/CuDans");

                    if (response.IsSuccessStatusCode)
                    {
                        listCuDan = await response.Content.ReadAsAsync<List<CuDan>>();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Lỗi API: " + response.StatusCode + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi kết nối: " + ex.Message);
            }
            return View(listCuDan);
        }

        // --- 2. XEM CHI TIẾT ---
        public async Task<ActionResult> Details(int id)
        {
            CuDan cuDan = null;
            using (var client = CreateClient())
            {
                // SỬA: Thêm chữ 's'
                HttpResponseMessage response = await client.GetAsync("api/CuDans/" + id);
                if (response.IsSuccessStatusCode)
                {
                    cuDan = await response.Content.ReadAsAsync<CuDan>();
                }
            }
            if (cuDan == null) return HttpNotFound();
            return View(cuDan);
        }

        // --- 3. THÊM MỚI ---
        public ActionResult Create()
        {
            LoadDropdownHoGiaDinh();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CuDan cuDan)
        {
            using (var client = CreateClient())
            {
                // SỬA: Thêm chữ 's'
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

        // --- 4. CẬP NHẬT ---
        public async Task<ActionResult> Edit(int id)
        {
            CuDan cuDan = null;
            using (var client = CreateClient())
            {
                // SỬA: Thêm chữ 's'
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
        public async Task<ActionResult> Edit(int id, CuDan cuDan)
        {
            using (var client = CreateClient())
            {
                // SỬA: Thêm chữ 's'
                HttpResponseMessage response = await client.PutAsJsonAsync("api/CuDans/" + id, cuDan);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            LoadDropdownHoGiaDinh();
            return View(cuDan);
        }

        // --- 5. XÓA ---
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (var client = CreateClient())
            {
                // SỬA: Thêm chữ 's'
                await client.DeleteAsync("api/CuDans/" + id);
            }
            return RedirectToAction("Index");
        }

        // --- HÀM PHỤ ---
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