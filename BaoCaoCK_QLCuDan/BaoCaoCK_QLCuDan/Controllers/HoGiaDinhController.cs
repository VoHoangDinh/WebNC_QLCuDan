using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult> Details(int? id, int page = 1)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return RedirectToAction("Index");
            }

            HoGiaDinh model = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + id.Value);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<HoGiaDinh>(data);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            // Lấy danh sách thành viên có phân trang
            if (model != null)
            {
                using (var client = new HttpClient())
                {
                    var membersResponse = await client.GetAsync($"{apiBaseUrl}/HoGiaDinhs/{id.Value}/Members?page={page}&pageSize=5");
                    if (membersResponse.IsSuccessStatusCode)
                    {
                        var membersData = await membersResponse.Content.ReadAsStringAsync();
                        var membersResult = JsonConvert.DeserializeObject<dynamic>(membersData);
                        
                        if (membersResult != null && membersResult.members != null)
                        {
                            var membersList = JsonConvert.DeserializeObject<List<CuDan>>(membersResult.members.ToString());
                            ViewBag.Members = membersList ?? new List<CuDan>();
                        }
                        else
                        {
                            ViewBag.Members = new List<CuDan>();
                        }
                        
                        ViewBag.TotalMembers = membersResult?.totalMembers ?? 0;
                        ViewBag.CurrentPage = membersResult?.currentPage ?? 1;
                        ViewBag.TotalPages = membersResult?.totalPages ?? 1;
                        ViewBag.PageSize = membersResult?.pageSize ?? 5;
                    }
                    else
                    {
                        ViewBag.Members = model.CuDans ?? new List<CuDan>();
                        ViewBag.TotalMembers = model.CuDans?.Count() ?? 0;
                        ViewBag.CurrentPage = 1;
                        ViewBag.TotalPages = 1;
                        ViewBag.PageSize = 5;
                    }
                }
            }

            return View(model);
        }

        // 3. TẠO MỚI (CREATE)
        public async Task<ActionResult> Create()
        {
            await LoadViewBag();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(HoGiaDinh model, int? MaCuDanChuHo)
        {
            if (ModelState.IsValid)
            {
                if (!MaCuDanChuHo.HasValue)
                {
                    ModelState.AddModelError("MaCuDanChuHo", "Vui lòng chọn cư dân làm chủ hộ.");
                    await LoadViewBag();
                    return View(model);
                }

                using (var client = new HttpClient())
                {
                    model.TenChuHo = null;
                    model.SoThanhVien = 1;
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiBaseUrl + "/HoGiaDinhs", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var hoData = await response.Content.ReadAsStringAsync();
                        var hoGiaDinh = JsonConvert.DeserializeObject<HoGiaDinh>(hoData);
                        
                        if (MaCuDanChuHo.HasValue && hoGiaDinh != null)
                        {
                            var cuDanResponse = await client.GetAsync(apiBaseUrl + "/CuDans/" + MaCuDanChuHo.Value);
                            if (cuDanResponse.IsSuccessStatusCode)
                            {
                                var cuDanData = await cuDanResponse.Content.ReadAsStringAsync();
                                var cuDan = JsonConvert.DeserializeObject<CuDan>(cuDanData);
                                
                                if (cuDan != null)
                                {
                                    cuDan.MaHo = hoGiaDinh.MaHo;
                                    cuDan.QuanHeVoiChuHo = "Chủ hộ";
                                    
                                    var updateContent = new StringContent(JsonConvert.SerializeObject(cuDan), Encoding.UTF8, "application/json");
                                    var updateResponse = await client.PutAsync(apiBaseUrl + "/CuDans/" + cuDan.MaCuDan, updateContent);
                                    
                                    // Đợi trigger chạy và query lại hộ gia đình để lấy tên chủ hộ đã được cập nhật
                                    if (updateResponse.IsSuccessStatusCode)
                                    {
                                        // Đợi một chút để trigger chạy
                                        await Task.Delay(500);
                                        
                                        // Query lại hộ gia đình để lấy tên chủ hộ đã được trigger cập nhật
                                        var refreshResponse = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + hoGiaDinh.MaHo);
                                        if (refreshResponse.IsSuccessStatusCode)
                                        {
                                            var refreshData = await refreshResponse.Content.ReadAsStringAsync();
                                            hoGiaDinh = JsonConvert.DeserializeObject<HoGiaDinh>(refreshData);
                                        }
                                    }
                                }
                            }
                        }
                        
                        return RedirectToAction("Details", new { id = hoGiaDinh.MaHo });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", "Lỗi khi tạo hộ gia đình: " + errorContent);
                    }
                }
            }
            await LoadViewBag();
            return View(model);
        }

        // 4. CHỈNH SỬA (EDIT)
        public async Task<ActionResult> Edit(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return RedirectToAction("Index");
            }

            HoGiaDinh model = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + id.Value);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<HoGiaDinh>(data);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            if (model == null)
            {
                return RedirectToAction("Index");
            }

            await LoadViewBag();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(HoGiaDinh model)
        {
            if (model == null || model.MaHo <= 0)
            {
                ModelState.AddModelError("", "Mã hộ không hợp lệ.");
                await LoadViewBag();
                return View(model);
            }

            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var updateModel = new
                    {
                        MaHo = model.MaHo,
                        NgayNhanNha = model.NgayNhanNha,
                        SoThanhVien = model.SoThanhVien,
                        TenChuHo = model.TenChuHo,
                        TrangThai = model.TrangThai,
                        MaCanHo = model.MaCanHo,
                        MaLoaiHo = model.MaLoaiHo
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(updateModel), Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(apiBaseUrl + "/HoGiaDinhs/" + model.MaHo, content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Details", new { id = model.MaHo });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", "Lỗi khi cập nhật: " + errorContent);
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ModelState.AddModelError("", "Dữ liệu không hợp lệ: " + string.Join(", ", errors));
            }
            
            await LoadViewBag();
            return View(model);
        }

        // 5. XÓA (DELETE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Mã hộ không hợp lệ.";
                return RedirectToAction("Index");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    // Set timeout
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    var response = await client.DeleteAsync(apiBaseUrl + "/HoGiaDinhs/" + id);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Xóa hộ gia đình thành công.";
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        var statusCode = (int)response.StatusCode;
                        TempData["ErrorMessage"] = $"Lỗi khi xóa (HTTP {statusCode}): {errorContent}";
                    }
                }
                catch (HttpRequestException ex)
                {
                    TempData["ErrorMessage"] = "Lỗi kết nối API: " + ex.Message;
                }
                catch (TaskCanceledException ex)
                {
                    TempData["ErrorMessage"] = "Lỗi timeout khi gọi API: " + ex.Message;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi xóa: " + ex.Message + " | Inner: " + (ex.InnerException?.Message ?? "");
                }
            }
            
            return RedirectToAction("Index");
        }

        // GET Delete để hiển thị confirm
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return RedirectToAction("Index");
            }

            HoGiaDinh model = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + id.Value);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<HoGiaDinh>(data);
                }
            }

            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // 6. THÊM THÀNH VIÊN VÀO HỘ (ADD MEMBER) - CHỈ CHỌN TỪ CƯ DÂN CÓ MaHo = NULL
        [HttpGet]
        public async Task<ActionResult> AddMember(int id)
        {
            HoGiaDinh hoGiaDinh = null;
            List<CuDan> listCuDanChuaCoHo = new List<CuDan>();

            using (var client = new HttpClient())
            {
                // Lấy thông tin hộ gia đình
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + id);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    hoGiaDinh = JsonConvert.DeserializeObject<HoGiaDinh>(data);
                }
                else
                {
                    return RedirectToAction("Index");
                }

                // Lấy danh sách cư dân chưa có hộ (MaHo = null)
                var cuDanResponse = await client.GetAsync(apiBaseUrl + "/CuDans");
                if (cuDanResponse.IsSuccessStatusCode)
                {
                    var cuDanData = await cuDanResponse.Content.ReadAsStringAsync();
                    var allCuDans = JsonConvert.DeserializeObject<List<CuDan>>(cuDanData) ?? new List<CuDan>();
                    listCuDanChuaCoHo = allCuDans.Where(c => !c.MaHo.HasValue || c.MaHo.Value == 0).ToList();
                }
            }

            if (hoGiaDinh == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.MaHo = id;
            ViewBag.TenHo = hoGiaDinh.TenChuHo ?? $"Hộ #{id}";
            ViewBag.ListCuDan = new SelectList(listCuDanChuaCoHo, "MaCuDan", "HoTen");
            
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddMember(int MaHo, int? MaCuDan, string QuanHeVoiChuHo)
        {
            if (!MaCuDan.HasValue)
            {
                ModelState.AddModelError("MaCuDan", "Vui lòng chọn cư dân để thêm vào hộ.");
                await LoadAddMemberViewBag(MaHo);
                return View();
            }

            if (string.IsNullOrEmpty(QuanHeVoiChuHo))
            {
                ModelState.AddModelError("QuanHeVoiChuHo", "Vui lòng chọn quan hệ với chủ hộ.");
                await LoadAddMemberViewBag(MaHo);
                return View();
            }

            try
            {
                using (var client = new HttpClient())
                {
                    // Lấy thông tin cư dân hiện tại
                    var cuDanResponse = await client.GetAsync(apiBaseUrl + "/CuDans/" + MaCuDan.Value);
                    if (!cuDanResponse.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError("", "Không tìm thấy cư dân được chọn.");
                        await LoadAddMemberViewBag(MaHo);
                        return View();
                    }

                    var cuDanData = await cuDanResponse.Content.ReadAsStringAsync();
                    var cuDan = JsonConvert.DeserializeObject<CuDan>(cuDanData);

                    if (cuDan == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy cư dân được chọn.");
                        await LoadAddMemberViewBag(MaHo);
                        return View();
                    }

                    // Kiểm tra cư dân đã có hộ chưa (phòng trường hợp đã được thêm vào hộ khác)
                    if (cuDan.MaHo.HasValue && cuDan.MaHo.Value > 0 && cuDan.MaHo.Value != MaHo)
                    {
                        ModelState.AddModelError("", "Cư dân này đã thuộc hộ khác, không thể thêm vào hộ này.");
                        await LoadAddMemberViewBag(MaHo);
                        return View();
                    }

                    // Cập nhật cư dân: gán vào hộ và đặt quan hệ
                    cuDan.MaHo = MaHo;
                    cuDan.QuanHeVoiChuHo = QuanHeVoiChuHo;

                    var updateContent = new StringContent(JsonConvert.SerializeObject(cuDan), Encoding.UTF8, "application/json");
                    var updateResponse = await client.PutAsync(apiBaseUrl + "/CuDans/" + cuDan.MaCuDan, updateContent);

                    if (updateResponse.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Details", new { id = MaHo });
                    }
                    else
                    {
                        var errorContent = await updateResponse.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", "Lỗi khi thêm thành viên: " + errorContent);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm thành viên: " + ex.Message);
            }

            await LoadAddMemberViewBag(MaHo);
            return View();
        }

        // Hàm phụ: Load ViewBag cho AddMember
        private async Task LoadAddMemberViewBag(int maHo)
        {
            HoGiaDinh hoGiaDinh = null;
            List<CuDan> listCuDanChuaCoHo = new List<CuDan>();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(apiBaseUrl + "/HoGiaDinhs/" + maHo);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    hoGiaDinh = JsonConvert.DeserializeObject<HoGiaDinh>(data);
                }

                var cuDanResponse = await client.GetAsync(apiBaseUrl + "/CuDans");
                if (cuDanResponse.IsSuccessStatusCode)
                {
                    var cuDanData = await cuDanResponse.Content.ReadAsStringAsync();
                    var allCuDans = JsonConvert.DeserializeObject<List<CuDan>>(cuDanData) ?? new List<CuDan>();
                    listCuDanChuaCoHo = allCuDans.Where(c => !c.MaHo.HasValue || c.MaHo.Value == 0).ToList();
                }
            }

            ViewBag.MaHo = maHo;
            ViewBag.TenHo = hoGiaDinh?.TenChuHo ?? $"Hộ #{maHo}";
            ViewBag.ListCuDan = new SelectList(listCuDanChuaCoHo, "MaCuDan", "HoTen");
        }

        // --- HÀM PHỤ: Lấy dữ liệu Dropdown từ API ---
        private async Task LoadViewBag()
        {
            List<CanHo> listCanHo = new List<CanHo>();
            List<LoaiHo> listLoai = new List<LoaiHo>();
            List<CuDan> listCuDan = new List<CuDan>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var resCanHo = await client.GetAsync(apiBaseUrl + "/CanHos");
                    if (resCanHo.IsSuccessStatusCode)
                    {
                        var data = await resCanHo.Content.ReadAsStringAsync();
                        listCanHo = JsonConvert.DeserializeObject<List<CanHo>>(data) ?? new List<CanHo>();
                    }

                    var resLoai = await client.GetAsync(apiBaseUrl + "/LoaiHos");
                    if (resLoai.IsSuccessStatusCode)
                    {
                        var data = await resLoai.Content.ReadAsStringAsync();
                        listLoai = JsonConvert.DeserializeObject<List<LoaiHo>>(data) ?? new List<LoaiHo>();
                    }

                    var resCuDan = await client.GetAsync(apiBaseUrl + "/CuDans");
                    if (resCuDan.IsSuccessStatusCode)
                    {
                        var data = await resCuDan.Content.ReadAsStringAsync();
                        var allCuDans = JsonConvert.DeserializeObject<List<CuDan>>(data) ?? new List<CuDan>();
                        listCuDan = allCuDans.Where(c => !c.MaHo.HasValue || c.MaHo.Value == 0).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Không kết nối được với API: " + ex.Message);
            }

            ViewBag.MaCanHo = new SelectList(listCanHo, "MaCanHo", "SoPhong");
            ViewBag.MaLoaiHo = new SelectList(listLoai, "MaLoaiHo", "TenLoai");
            
            if (listCuDan != null && listCuDan.Any())
            {
                ViewBag.MaCuDanChuHo = new SelectList(listCuDan, "MaCuDan", "HoTen");
            }
            else
            {
                ViewBag.MaCuDanChuHo = new SelectList(new List<CuDan>(), "MaCuDan", "HoTen");
            }
        }
    }
}
