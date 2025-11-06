using System;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using Web_KLCN.Models;

namespace Web_KLCN.Controllers
{
    public class HomeController : Controller
    {
        private readonly TUQDataContext db;

        public HomeController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_TracNghiem_TUQConnectionString"].ConnectionString;
            db = new TUQDataContext(connectionString);
        }

        // ================================
        // 1️⃣ Trang chủ công khai
        // ================================
        public ActionResult Index()
        {
            // Nếu học viên đã đăng nhập → chuyển sang dashboard
            if (Session["MaTK"] != null)
                return RedirectToAction("Index", "HocVien");

            ViewData["Title"] = "Trung tâm luyện thi TUQ";
            return View();
        }

        // ================================
        // 2️⃣ Đăng nhập học viên
        // ================================
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["MaTK"] != null)
                return RedirectToAction("Index", "HocVien");

            ViewData["Title"] = "Đăng nhập học viên";
            return View();
        }

        [HttpPost]
        public ActionResult Login(string taiKhoan, string matKhau)
        {
            ViewData["Title"] = "Đăng nhập học viên";

            if (string.IsNullOrEmpty(taiKhoan) || string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            var user = (from t in db.TAIKHOANs
                        join p in db.PHANQUYENs on t.MaPQ equals p.MaPQ
                        where t.TaiKhoan1 == taiKhoan && t.MatKhau == matKhau
                        select new { TaiKhoan = t, PhanQuyen = p })
                       .FirstOrDefault();

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            if (user.PhanQuyen.LoaiPQ.Trim().ToLower() != "học viên")
            {
                ViewBag.Error = "Chỉ dành cho học viên!";
                return View();
            }

            var hv = db.HOCVIENs.SingleOrDefault(h => h.MaTK == user.TaiKhoan.MaTK);
            if (hv == null)
            {
                ViewBag.Error = "Không tìm thấy thông tin học viên!";
                return View();
            }

            if (user.TaiKhoan.TrangThai == null ||
                user.TaiKhoan.TrangThai.Trim().ToLower() != "hoạt động")
            {
                ViewBag.Error = "Tài khoản học viên hiện không hoạt động!";
                return View();
            }

            // ✅ Lưu session
            Session["MaTK"] = user.TaiKhoan.MaTK;
            Session["TenHV"] = hv.TenHV;
            Session["HOCVIEN"] = hv;

            return RedirectToAction("Index", "HocVien");
        }

        // ================================
        // 3️⃣ Đăng xuất
        // ================================
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }
    }
}
