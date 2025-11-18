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
        // 1️. Trang chủ
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
        // 2️. Đăng nhập học viên
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
        // 3️. Đăng xuất
        // ================================
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }

        // ================================
        // 4️. Đăng ký học viên
        // ================================
        [HttpGet]
        public ActionResult Register()
        {
            // Nếu đã đăng nhập thì chuyển sang trang học viên
            if (Session["MaTK"] != null)
                return RedirectToAction("Index", "HocVien");

            ViewData["Title"] = "Đăng ký học viên";
            return View();
        }

        [HttpPost]
        public ActionResult Register(string taiKhoan, string matKhau, string xacNhanMatKhau, string hoTen)
        {
            ViewData["Title"] = "Đăng ký học viên";

            // 1️. Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(taiKhoan) || string.IsNullOrWhiteSpace(matKhau) || string.IsNullOrWhiteSpace(hoTen))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            // 2️. Kiểm tra tài khoản đã tồn tại chưa
            var existingUser = db.TAIKHOANs.FirstOrDefault(t => t.TaiKhoan1 == taiKhoan);
            if (existingUser != null)
            {
                ViewBag.Error = "Tài khoản đã tồn tại!";
                return View();
            }

            // 3️. Lấy mã phân quyền học viên
            var pqHocVien = db.PHANQUYENs.FirstOrDefault(p => p.LoaiPQ.Trim().ToLower() == "học viên");
            if (pqHocVien == null)
            {
                ViewBag.Error = "Không tìm thấy phân quyền học viên!";
                return View();
            }

            // 4️. Tạo tài khoản mới
            var taiKhoanMoi = new TAIKHOAN
            {
                TaiKhoan1 = taiKhoan,
                MatKhau = matKhau,
                MaPQ = pqHocVien.MaPQ,
                TrangThai = "Hoạt động"
            };
            db.TAIKHOANs.InsertOnSubmit(taiKhoanMoi);
            db.SubmitChanges();

            // 5️. Tạo thông tin học viên mới
            var hocVienMoi = new HOCVIEN
            {
                TenHV = hoTen,
                MaTK = taiKhoanMoi.MaTK
            };
            db.HOCVIENs.InsertOnSubmit(hocVienMoi);
            db.SubmitChanges();

            TempData["Success"] = "Đăng ký thành công! Hãy đăng nhập để tiếp tục.";
            return RedirectToAction("Login");
        }

    }
}
