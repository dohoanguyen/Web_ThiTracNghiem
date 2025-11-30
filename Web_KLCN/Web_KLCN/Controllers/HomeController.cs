using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http; //xử lý dữ liệu JSON
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
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

        // ================================
        // 5️. Trang Khóa học
        // ================================
        public ActionResult KhoaHoc()
        {
            ViewData["Title"] = "Khóa học - TUQ Education";

            var danhSachKhoaHoc = db.KHOAHOCs
                    .Select(k => new KhoaHocVM
                    {
                        MaKH = k.MaK,
                        TenKhoaHoc = k.TenKhoaHoc,
                        //DanhMuc = k.DanhMuc,
                        //MoTa = k.MoTa,
                        //CapDo = k.CapDo,
                        //AnhKhoaHoc = k.AnhKhoaHoc,
                        NgayBatDau = k.NgayBatDau,
                        NgayKetThuc = k.NgayKetThuc,
                        //SoHocVien = k.DangKies.Count() 
                    }).ToList();

            return View(danhSachKhoaHoc);
        }

        // ================================
        // 6️. Trang Luyện thi THPT
        // ================================
        public ActionResult LuyenThiTHPT()
        {
            ViewData["Title"] = "Luyện thi THPT - TUQ Education";


            return View();
        }

        // ================================
        // 7️. Trang Tin tức
        // ================================
        //    public ActionResult TinTuc()
        //    {
        //        ViewBag.Title = "Tin tức - TUQ Education";

        //        // Dữ liệu mẫu cho ViewBag.Posts
        //        ViewBag.Posts = new List<dynamic>
        //{
        //    new { Title = "Bài viết 1", Url = "#", Image = "/images/post1.jpg", Category = "Giáo dục", Date = DateTime.Now, Summary = "Tóm tắt bài viết 1" },
        //    new { Title = "Bài viết 2", Url = "#", Image = "/images/post2.jpg", Category = "Kỳ thi", Date = DateTime.Now, Summary = "Tóm tắt bài viết 2" }
        //};

        //        // Dữ liệu mẫu cho ViewBag.Topics
        //        ViewBag.Topics = new List<dynamic>
        //{
        //    new { Name = "Chủ đề 1", Url = "#", Count = 5 },
        //    new { Name = "Chủ đề 2", Url = "#", Count = 3 }
        //};

        //        // Dữ liệu mẫu cho ViewBag.FeaturedPosts
        //        ViewBag.FeaturedPosts = new List<dynamic>
        //{
        //    new { Title = "Bài viết nổi bật 1", Url = "#", Image = "/images/featured1.jpg", Date = DateTime.Now },
        //    new { Title = "Bài viết nổi bật 2", Url = "#", Image = "/images/featured2.jpg", Date = DateTime.Now }
        //};

        //        ViewBag.TotalPages = 3;
        //        ViewBag.CurrentPage = 1;

        //        return View();
        //    }

        // ================================
        // 8. Trang Liên hệ
        // ================================
        public ActionResult LienHe()
        {
            ViewData["Title"] = "Liên hệ - TUQ Education";

            return View();
        }

        // ================================
        // 9. Trang Tin Tức (API thực)
        // ================================
        public async Task<ActionResult> TinTuc(int trang = 1)
        {
            string apiKey = "1e3d09a5bffc4f65bd97f2a85a1a5e70";
            string tuKhoa = "THPT quốc gia";
            int soLuongMoiTrang = 5;

            string diaChiApi =
                $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(tuKhoa)}" +
                $"&language=vi&pageSize={soLuongMoiTrang}&page={trang}&apiKey={apiKey}";

            List<BaiVietTinTuc> danhSachBaiViet = new List<BaiVietTinTuc>();

            using (var http = new HttpClient())
            {
                try
                {
                    var phanHoiJson = await http.GetStringAsync(diaChiApi);

                    // Deserialize vào model API
                    var duLieuApi = JsonConvert.DeserializeObject<PhanHoiTinTuc>(phanHoiJson);

                    if (duLieuApi != null && duLieuApi.Status == "ok")
                    {
                        // Map sang model để hiển thị View
                        danhSachBaiViet = duLieuApi.Articles.Select(a => new BaiVietTinTuc
                        {
                            TieuDe = a.Title,
                            DiaChi = a.Url,
                            HinhAnh = string.IsNullOrEmpty(a.UrlToImage) ? "/images/default.png" : a.UrlToImage,
                            ChuDe = a.Source?.Name ?? "Không rõ",
                            TomTat = a.Description,
                            NgayDang = a.PublishedAt.ToLocalTime()
                        }).ToList();

                        ViewBag.TongSoTrang =
                            (int)Math.Ceiling(duLieuApi.TotalResults / (double)soLuongMoiTrang);
                    }
                }
                catch
                {
                    ViewBag.Loi = "Không thể tải dữ liệu tin tức từ API.";
                }
            }

            ViewBag.BaiViet = danhSachBaiViet;
            ViewBag.TrangHienTai = trang;
            ViewBag.BaiVietNoiBat = danhSachBaiViet.Take(3).ToList();

            return View();
        }

        // Model tin tức
        public class PhanHoiTinTuc
        {
            public string Status { get; set; }                 // giữ nguyên theo API
            public int TotalResults { get; set; }              // tổng số bài viết
            public List<BaiVietApi> Articles { get; set; }     // danh sách bài viết API
        }

        public class BaiVietApi
        {
            public NguonApi Source { get; set; }
            public string Author { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string UrlToImage { get; set; }
            public DateTime PublishedAt { get; set; }
            public string Content { get; set; }
        }

        public class NguonApi
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class BaiVietTinTuc
        {
            public string TieuDe { get; set; }
            public string DiaChi { get; set; }
            public string HinhAnh { get; set; }
            public string ChuDe { get; set; }
            public string TomTat { get; set; }
            public DateTime NgayDang { get; set; }
        }
    }
}