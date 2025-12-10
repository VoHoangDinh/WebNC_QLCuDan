using System.Collections.Generic;

namespace BaoCaoCK_QLCuDan.Models
{
    // Class này dùng để hứng cục JSON trả về từ API
    public class PhanTrangCuDan
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<CuDan> Items { get; set; }
    }
}