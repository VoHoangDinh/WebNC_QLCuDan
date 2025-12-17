using System.Linq;
using System.Web.Mvc;
using BaoCaoCK_QLCuDan.Models; // Đảm bảo đúng namespace

namespace BaoCaoCK_QLCuDan.Controllers
{
    public class HomeController : Controller
    {
        private QuanLyCuDanContext db = new QuanLyCuDanContext();

        public ActionResult Index()
        {
            // 1. Thống kê số liệu để hiển thị lên Dashboard
            ViewBag.TongCuDan = db.CuDans.Count();
            ViewBag.TongHoGiaDinh = db.HoGiaDinhs.Count();
            ViewBag.TongCanHo = db.CanHos.Count();

            // Đếm số căn hộ còn trống
            ViewBag.CanHoTrong = db.CanHos.Count(x => x.TrangThai == "Trống" || x.TrangThai == "Chưa có người");

            return View();
        }
    }
}