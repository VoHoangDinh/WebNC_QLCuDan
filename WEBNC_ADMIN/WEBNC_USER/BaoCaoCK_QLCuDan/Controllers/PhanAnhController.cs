using BaoCaoCK_QLCuDan.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class PhanAnhController : Controller
    {
        private const string BaseUrl = "https://localhost:7107/";

        // Tạo HttpClient để gọi API (thêm Token vào header nếu có)
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (Session["Token"] != null)
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", Session["Token"].ToString());
            }
            
            return client;
        }

        // GET: PhanAnh/Index - Xem danh sách TẤT CẢ phản ánh của user (bao gồm cả "Mới tiếp nhận")
        public async Task<ActionResult> Index()
        {
            if (Session["Token"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? maCuDan = Session["MaCuDan"] as int?;
            if (!maCuDan.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin cư dân. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            List<PhanAnh> listPhanAnh = new List<PhanAnh>();

            try
            {
                using (var client = CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"api/PhanAnhApi/by-cudan/{maCuDan.Value}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        listPhanAnh = JsonConvert.DeserializeObject<List<PhanAnh>>(responseData);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách phản ánh: " + ex.Message;
            }

            return View(listPhanAnh);
        }

        // GET: PhanAnh/Create - Hiển thị form để user gửi phản ánh mới
        public ActionResult Create()
        {
            if (Session["Token"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: PhanAnh/Create - Xử lý gửi phản ánh mới từ user (lưu vào database với trạng thái "Mới tiếp nhận")
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PhanAnh phanAnh)
        {
            if (Session["Token"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? maCuDan = Session["MaCuDan"] as int?;
            if (!maCuDan.HasValue)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin cư dân. Vui lòng đăng nhập lại.");
                return View(phanAnh);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    phanAnh.MaCuDan = maCuDan.Value;
                    phanAnh.NgayGui = DateTime.Now;
                    phanAnh.TrangThai = "Mới tiếp nhận";

                    using (var client = CreateClient())
                    {
                        var json = JsonConvert.SerializeObject(phanAnh);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync("api/PhanAnhApi", content);

                        if (response.IsSuccessStatusCode)
                        {
                            TempData["SuccessMessage"] = "Gửi phản ánh thành công! Admin sẽ xem xét và phản hồi sớm nhất.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            ModelState.AddModelError("", "Có lỗi xảy ra khi gửi phản ánh: " + errorContent);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError("", "Không thể kết nối đến API. Vui lòng kiểm tra API đã chạy chưa.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(phanAnh);
        }

        // GET: PhanAnh/Details/{id} - Xem chi tiết một phản ánh cụ thể (chỉ xem được phản ánh của chính mình)
        public async Task<ActionResult> Details(int? id)
        {
            if (Session["Token"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int? maCuDan = Session["MaCuDan"] as int?;
            if (!maCuDan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            PhanAnh phanAnh = null;

            try
            {
                using (var client = CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"api/PhanAnhApi/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        phanAnh = JsonConvert.DeserializeObject<PhanAnh>(responseData);

                        // Kiểm tra bảo mật: chỉ cho phép xem phản ánh của chính mình
                        if (phanAnh.MaCuDan != maCuDan.Value)
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền xem phản ánh này.";
                            return RedirectToAction("Index");
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return HttpNotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }

            if (phanAnh == null)
            {
                return HttpNotFound();
            }

            return View(phanAnh);
        }

        // GET: PhanAnh/Notifications - Xem thông báo về phản ánh đã được admin cập nhật (chỉ hiển thị phản ánh có trạng thái khác "Mới tiếp nhận")
        public async Task<ActionResult> Notifications()
        {
            if (Session["Token"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? maCuDan = Session["MaCuDan"] as int?;
            if (!maCuDan.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin cư dân. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            List<PhanAnh> listPhanAnh = new List<PhanAnh>();

            try
            {
                using (var client = CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"api/PhanAnhApi/by-cudan/{maCuDan.Value}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var allPhanAnh = JsonConvert.DeserializeObject<List<PhanAnh>>(responseData);
                        
                        // Lọc chỉ lấy phản ánh đã được admin cập nhật (trạng thái khác "Mới tiếp nhận")
                        listPhanAnh = allPhanAnh.Where(p => p.TrangThai != "Mới tiếp nhận")
                                                 .OrderByDescending(p => p.NgayGui)
                                                 .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông báo: " + ex.Message;
            }

            return View(listPhanAnh);
        }
    }
}

