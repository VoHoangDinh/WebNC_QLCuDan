using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaoCaoCK_QLCuDan.Helpers
{
    public static class JwtHelper
    {
        /// <summary>
        /// Giải mã JWT token và lấy MaCuDan từ claims
        /// </summary>
        public static int? GetMaCuDanFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                // JWT token có format: header.payload.signature
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return null;

                // Lấy payload (phần thứ 2)
                var payload = parts[1];

                // Decode base64 (JWT sử dụng base64url encoding)
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);

                // Parse JSON
                var claims = JObject.Parse(payloadJson);

                // Tìm MaCuDan trong claims
                var maCuDanValue = claims["MaCuDan"]?.ToString();
                
                if (string.IsNullOrEmpty(maCuDanValue))
                    return null;

                // Chuyển đổi sang int
                if (int.TryParse(maCuDanValue, out int maCuDan))
                {
                    return maCuDan;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

