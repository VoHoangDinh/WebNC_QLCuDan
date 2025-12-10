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

        // GET: Account/Signup
        public ActionResult Signup()
        {
            return View();
        }

        // POST: Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Signup(SignUpRequest model)
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

                    HttpResponseMessage response = await client.PostAsync("api/Account/signup", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        // Thành công - chuyển đến trang đăng nhập
                        TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        // Xử lý lỗi từ API
                        var errorContent = await response.Content.ReadAsStringAsync();
                        
                        try
                        {
                            // Thử parse lỗi dạng array
                            var errors = JsonConvert.DeserializeObject<List<ApiError>>(errorContent);
                            if (errors != null && errors.Count > 0)
                            {
                                foreach (var error in errors)
                                {
                                    if (!string.IsNullOrEmpty(error.Description))
                                    {
                                        ModelState.AddModelError("", error.Description);
                                    }
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Đăng ký thất bại. Vui lòng thử lại.");
                            }
                        }
                        catch
                        {
                            // Nếu không parse được, hiển thị lỗi chung
                            ModelState.AddModelError("", "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.");
                        }
                        
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

    // Class để deserialize lỗi từ API
    public class ApiError
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
}