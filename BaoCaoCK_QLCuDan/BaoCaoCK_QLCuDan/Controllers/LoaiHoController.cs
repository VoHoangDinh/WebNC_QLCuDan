using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks; // Cần thêm dòng này
using System.Web.Mvc;
using BaoCaoCK_QLCuDan.Models;
using Newtonsoft.Json;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class LoaiHoController : Controller
    {
        string apiBaseUrl = "https://localhost:7107/api/LoaiHos";

        // 1. HIỂN THỊ DANH SÁCH (GET)
        public async Task<ActionResult> Index()
        {
            List<LoaiHo> listLoaiHo = new List<LoaiHo>();
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    listLoaiHo = JsonConvert.DeserializeObject<List<LoaiHo>>(data);
                }
            }
            return View(listLoaiHo);
        }

        // 2. TẠO MỚI (CREATE)
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(LoaiHo model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiBaseUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
                ModelState.AddModelError("", "Lỗi khi gọi API thêm mới.");
            }
            return View(model);
        }

        // 3. CHỈNH SỬA (EDIT)
        public async Task<ActionResult> Edit(int id)
        {
            LoaiHo model = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl + "/" + id);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<LoaiHo>(data);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(LoaiHo model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                    // PUT: api/LoaiHos/{id}
                    var response = await client.PutAsync(apiBaseUrl + "/" + model.MaLoaiHo, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(model);
        }

        // 4. XÓA (DELETE)
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(apiBaseUrl + "/" + id);
                return RedirectToAction("Index");
            }
        }
    }
}