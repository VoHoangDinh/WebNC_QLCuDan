using BaoCaoCK_QLCuDan.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BaoCaoCK_QLCuDan.Helpers
{
    public static class NotificationHelper
    {
        private const string BaseUrl = "https://localhost:7107/";

        // Đếm số lượng thông báo phản ánh đã được admin cập nhật (trạng thái khác "Mới tiếp nhận")
        public static int GetNotificationCount(int? maCuDan, string token = null)
        {
            if (!maCuDan.HasValue)
                return 0;

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    var phanAnhResponse = client.GetAsync($"api/PhanAnhApi/by-cudan/{maCuDan.Value}").Result;
                    
                    if (phanAnhResponse.IsSuccessStatusCode)
                    {
                        var responseData = phanAnhResponse.Content.ReadAsStringAsync().Result;
                        var allPhanAnh = JsonConvert.DeserializeObject<List<PhanAnh>>(responseData);
                        
                        return allPhanAnh?.Count(p => p.TrangThai != "Mới tiếp nhận") ?? 0;
                    }
                }
            }
            catch
            {
                return 0;
            }

            return 0;
        }

        // ============================================================
        // CÁC METHOD DƯỚI ĐÂY ĐÃ BỊ VÔ HIỆU HÓA - CHỈ GIỮ LẠI ĐỂ THAM KHẢO
        // ============================================================
        // Lý do: User phải đăng nhập lại để thấy quyền mới được cấp
        // Không cần thông báo về thay đổi quyền trong trang thông báo
        // ============================================================
        
        /*
        private static bool CheckPermissionChange(int maCuDan, string currentToken, HttpClient client)
        {
            // Method này đã bị vô hiệu hóa
            // Logic: So sánh quyền trong JWT token với quyền từ database
            // Nếu khác nhau → có thay đổi quyền → hiển thị thông báo
            return false;
        }

        private static List<string> GetPermissionsFromToken(string token)
        {
            // Method này đã bị vô hiệu hóa
            // Logic: Giải mã JWT token và lấy danh sách Permission từ claims
            return new List<string>();
        }
        */
    }
}

