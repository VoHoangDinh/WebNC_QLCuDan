using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BaoCaoCK_QLCuDan.Models;
using BaoCaoCK_QLCuDan.Helpers;
using Newtonsoft.Json;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class NguoiDungController : Controller
    {
        private const string BaseUrl = "https://localhost:7107/";

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Thêm token vào header nếu có
            if (Session["Token"] != null)
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", Session["Token"].ToString());
            }
            
            return client;
        }

        // GET: NguoiDung/Home
        public async Task<ActionResult> Home()
        {
            // Kiểm tra đăng nhập
            if (Session["Token"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy MaCuDan từ Session
            int? maCuDan = null;
            if (Session["MaCuDan"] != null)
            {
                maCuDan = (int)Session["MaCuDan"];
            }
            else
            {
                // Nếu chưa có trong Session, giải mã từ token
                var token = Session["Token"]?.ToString();
                maCuDan = JwtHelper.GetMaCuDanFromToken(token);
                if (maCuDan.HasValue)
                {
                    Session["MaCuDan"] = maCuDan.Value;
                }
            }

            if (!maCuDan.HasValue)
            {
                ViewBag.ErrorMessage = "Không tìm thấy mã cư dân. Vui lòng đăng nhập lại.";
                return View();
            }

            try
            {
                // Gọi API để lấy thông tin cư dân
                using (var client = CreateClient())
                {
                    var response = await client.GetAsync($"api/CuDans/{maCuDan.Value}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var cuDan = JsonConvert.DeserializeObject<CuDan>(content);
                        return View(cuDan);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        ViewBag.ErrorMessage = "Không tìm thấy thông tin cư dân.";
                        return View();
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải thông tin cư dân.";
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra: " + ex.Message;
                return View();
            }
        }
    }
}

