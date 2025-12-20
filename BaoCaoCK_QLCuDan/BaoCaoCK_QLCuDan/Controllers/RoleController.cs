using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class RoleController : Controller
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

        // GET: Role
        public async Task<ActionResult> Index()
        {
            List<RoleViewModel> roles = new List<RoleViewModel>();

            try
            {
                using (var client = CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync("api/Role/get-all");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        roles = JsonConvert.DeserializeObject<List<RoleViewModel>>(jsonString);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải dữ liệu: " + ex.Message;
            }

            return View(roles);
        }

        // POST: Role/AddPermission
        [HttpPost]
        public async Task<ActionResult> AddPermission(string roleName, string permission)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(permission))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
            }

            try
            {
                using (var client = CreateClient())
                {
                    string url = $"api/Role/add-permission?roleName={Uri.EscapeDataString(roleName)}&permission={Uri.EscapeDataString(permission)}";
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Đã thêm permission thành công" });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = errorMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Role/UpdatePermission
        [HttpPost]
        public async Task<ActionResult> UpdatePermission(string roleName, string oldPermission, string newPermission)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(oldPermission) || string.IsNullOrWhiteSpace(newPermission))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
            }

            try
            {
                using (var client = CreateClient())
                {
                    string url = $"api/Role/update-permission?roleName={Uri.EscapeDataString(roleName)}&oldPermission={Uri.EscapeDataString(oldPermission)}&newPermission={Uri.EscapeDataString(newPermission)}";
                    HttpResponseMessage response = await client.PutAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Đã cập nhật permission thành công" });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = errorMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Role/DeletePermission
        [HttpPost]
        public async Task<ActionResult> DeletePermission(string roleName, string permission)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(permission))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
            }

            try
            {
                using (var client = CreateClient())
                {
                    string url = $"api/Role/delete-permission?roleName={Uri.EscapeDataString(roleName)}&permission={Uri.EscapeDataString(permission)}";
                    HttpResponseMessage response = await client.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Đã xóa permission thành công" });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = errorMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Role/UpdateUser
        public ActionResult UpdateUser()
        {
            return View();
        }

        // POST: Role/UpdateUser
        [HttpPost]
        public async Task<ActionResult> UpdateUser(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin" });
            }

            try
            {
                using (var client = CreateClient())
                {
                    string url = $"api/Role/assign-role-to-user?userId={Uri.EscapeDataString(userId)}&roleName={Uri.EscapeDataString(roleName)}";
                    HttpResponseMessage response = await client.PutAsync(url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var message = await response.Content.ReadAsStringAsync();
                        return Json(new { success = true, message = message });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = errorMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Role/GetUsers
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            List<UserViewModel> users = new List<UserViewModel>();

            try
            {
                using (var client = CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync("api/Role/get-users");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        users = JsonConvert.DeserializeObject<List<UserViewModel>>(jsonString);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = users }, JsonRequestBehavior.AllowGet);
        }
    }

    // ViewModel cho Role
    public class RoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    // ViewModel cho User
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}

