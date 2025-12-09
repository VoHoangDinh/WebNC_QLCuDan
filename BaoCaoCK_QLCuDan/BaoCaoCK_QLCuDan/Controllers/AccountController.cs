using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using BaoCaoCK_QLCuDan.Models;
using System.Threading.Tasks;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _apiBaseUrl = "https://localhost:7107/api/"; // Base URL for API

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            // Bypass SSL cho localhost
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            if (!ModelState.IsValid) return View(model);

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10); // Timeout 10s

                try
                {
                    // 1. GỌI API ĐĂNG NHẬP
                    string loginUrl = _apiBaseUrl + "Account/SignIn"; // Link: .../api/Account/SignIn
                    var jsonContent = JsonConvert.SerializeObject(model);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(loginUrl, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // 2. LẤY TOKEN
                        dynamic data = JsonConvert.DeserializeObject(responseString);
                        string token = data.Token ?? data.token;

                        if (string.IsNullOrEmpty(token))
                        {
                            ModelState.AddModelError("", "Lỗi: API trả về thành công nhưng không có Token!");
                            return View(model);
                        }

                        // Lưu Token
                        Session["AccessToken"] = token;
                        Session["UserEmail"] = model.Email;

                        // 3. GỌI NGAY API LẤY THÔNG TIN (Để check xem Token có dùng được không)
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        // Lưu ý: Sửa lại đường dẫn này nếu port của bạn khác 7107
                        var profileResponse = await client.GetAsync("https://localhost:7107/api/CuDans/me");

                        if (profileResponse.IsSuccessStatusCode)
                        {
                            // Token ngon -> Lấy thông tin lưu Session cho đẹp đội hình
                            var cuDanJson = await profileResponse.Content.ReadAsStringAsync();
                            var cuDan = JsonConvert.DeserializeObject<CuDan>(cuDanJson);

                            Session["UserName"] = cuDan.HoTen;
                            Session["UserAvatar"] = cuDan.Avatar;
                            Session["MaCuDan"] = cuDan.MaCuDan;

                            // Logic phân quyền giả định (Bạn có thể sửa lại theo logic thật)
                            if (model.Email.ToLower().Contains("admin"))
                                Session["UserRole"] = "Admin";
                            else
                                Session["UserRole"] = "CuDan";

                            // CHUYỂN HƯỚNG THÀNH CÔNG
                            return RedirectToAction("MyProfile", "CuDan");
                        }
                        else
                        {
                            // LỖI: Có Token nhưng không lấy được thông tin -> In lỗi ra xem
                            string errDetail = await profileResponse.Content.ReadAsStringAsync();
                            ModelState.AddModelError("", $"Đăng nhập được nhưng không lấy được hồ sơ! Mã lỗi API: {profileResponse.StatusCode}. Chi tiết: {errDetail}");
                            return View(model);
                        }
                    }
                    else
                    {
                        // LỖI: Sai mật khẩu hoặc email
                        ModelState.AddModelError("", $"Đăng nhập thất bại: {responseString}");
                    }
                }
                catch (Exception ex)
                {
                    // LỖI: Không kết nối được API (Sai port, API chưa bật...)
                    ModelState.AddModelError("", "Lỗi kết nối hệ thống: " + ex.Message);
                }
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}