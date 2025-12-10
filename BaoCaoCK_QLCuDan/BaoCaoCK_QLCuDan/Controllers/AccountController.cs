using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BaoCaoCK_QLCuDan.DTO;
using BaoCaoCK_QLCuDan.Helpers;
using Newtonsoft.Json;
using System.Configuration;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class AccountController : Controller
    {
        private const string BaseUrl = "https://localhost:7107/";

        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (var client = CreateClient())
                {
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("api/Account/SignIn", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                        // Lưu token vào session
                        Session["Token"] = loginResponse.Token;
                        Session["Email"] = model.Email;

                        // Giải mã token và lấy MaCuDan
                        var maCuDan = JwtHelper.GetMaCuDanFromToken(loginResponse.Token);
                        if (maCuDan.HasValue)
                        {
                            Session["MaCuDan"] = maCuDan.Value;
                        }

                        // Redirect đến trang Home
                        return RedirectToAction("Home", "NguoiDung");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", "Đăng nhập thất bại. Vui lòng kiểm tra lại email và mật khẩu.");
                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
    }
}