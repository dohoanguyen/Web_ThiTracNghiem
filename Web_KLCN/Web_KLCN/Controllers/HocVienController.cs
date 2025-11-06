using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Web_KLCN.Models;

namespace Web_KLCN.Controllers
{
    public class HocVienController : Controller
    {
        private readonly TUQDataContext db;

        public HocVienController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["QL_TracNghiem_TUQConnectionString"].ConnectionString;
            db = new TUQDataContext(connectionString);
        }

        // ============================
        // TRANG CHÍNH HỌC VIÊN
        // ============================
        public ActionResult Index()
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;
            if (hv == null)
            {
                int maTK = Convert.ToInt32(Session["MaTK"]);
                hv = db.HOCVIENs.SingleOrDefault(x => x.MaTK == maTK);
                Session["HOCVIEN"] = hv;
            }

            return View(hv);
        }

        // ============================
        // THÔNG TIN CÁ NHÂN (GET)
        // ============================
        [HttpGet]
        public ActionResult ThongTinCaNhan()
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;
            if (hv == null)
            {
                int maTK = Convert.ToInt32(Session["MaTK"]);
                hv = db.HOCVIENs.SingleOrDefault(x => x.MaTK == maTK);
                Session["HOCVIEN"] = hv;
            }

            return View(hv);
        }

        // ============================
        // THÔNG TIN CÁ NHÂN (POST - CẬP NHẬT)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThongTinCaNhan(HOCVIEN model)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            try
            {
                var hv = db.HOCVIENs.SingleOrDefault(x => x.MaHV == model.MaHV);
                if (hv == null)
                    return HttpNotFound();

                hv.TenHV = model.TenHV?.Trim();
                hv.NgaySinh = model.NgaySinh;
                hv.GioiTinh = model.GioiTinh;
                hv.Email = model.Email?.Trim();
                hv.SDT = model.SDT?.Trim();
                hv.DiaChi = model.DiaChi?.Trim();

                // Xử lý upload ảnh đại diện
                if (Request.Files["Anh"] != null && Request.Files["Anh"].ContentLength > 0)
                {
                    var file = Request.Files["Anh"];
                    string fileName = System.IO.Path.GetFileName(file.FileName);
                    string path = Server.MapPath("~/hinhanh/" + fileName);
                    file.SaveAs(path);
                    hv.AnhDaiDien = fileName;
                }

                db.SubmitChanges();
                Session["HOCVIEN"] = hv;

                TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
            }

            return RedirectToAction("ThongTinCaNhan");
        }

        // ============================
        // LỊCH THI
        // ============================
        public ActionResult LichThi()
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;

            var raw = (from lt in db.LICHTHIs
                       join lh in db.HOCVIEN_LOPHOCs on lt.MaL equals lh.MaL
                       join d in db.DETHIs on lt.MaD equals d.MaD
                       join mh in db.MONs on d.MaM equals mh.MaM
                       where lh.MaHV == hv.MaHV
                       select new
                       {
                           lt.MaLT,
                           d.TenDe,
                           Mon = mh.TenMon,
                           lt.NgayThi,
                           lt.GioBatDau,
                           lt.GioKetThuc,
                           lt.TrangThai
                       }).ToList();

            var lichThi = raw.Select(x => new HocVienLichThiVM
            {
                MaLT = x.MaLT,
                TenDe = x.TenDe,
                Mon = x.Mon,
                NgayThi = x.NgayThi,
                GioBatDau = x.GioBatDau.ToString(@"hh\:mm"),
                GioKetThuc = x.GioKetThuc.ToString(@"hh\:mm"),
                TrangThai = x.TrangThai
            }).ToList();

            return View(lichThi);
        }

        // ============================
        // LÀM BÀI THI
        // ============================
        [HttpGet]
        public ActionResult LamBai(int id)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;
            if (hv == null)
            {
                int maTK = Convert.ToInt32(Session["MaTK"]);
                hv = db.HOCVIENs.SingleOrDefault(x => x.MaTK == maTK);
                Session["HOCVIEN"] = hv;
            }

            // Tìm lịch thi
            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == id);
            if (lt == null) return HttpNotFound();

            // Lấy đề thi và câu hỏi
            var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
            var mon = db.MONs.SingleOrDefault(x => x.MaM == de.MaM);

            var cauHoiList = (from ct in db.DETHI_CAUHOIs
                              join ch in db.NGANHANGCAUHOIs on ct.MaCH equals ch.MaCH
                              where ct.MaD == de.MaD
                              orderby ct.STT
                              select new BaiThiCauHoiVM
                              {
                                  MaCH = ch.MaCH,
                                  NoiDung = ch.NoiDung,
                                  DapAnA = ch.DapAnA,
                                  DapAnB = ch.DapAnB,
                                  DapAnC = ch.DapAnC,
                                  DapAnD = ch.DapAnD
                              }).ToList();

            var vm = new BaiThiViewModel
            {
                MaLT = lt.MaLT,
                TenBaiThi = de.TenDe,
                MonHoc = mon.TenMon,
                ThoiGian = de.ThoiGian ?? 30, // fallback nếu null
                CauHoiList = cauHoiList
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NopBai(int MaLT, FormCollection form)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;
            if (hv == null)
                return RedirectToAction("Login", "Home");

            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == MaLT);
            if (lt == null)
                return HttpNotFound();

            var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
            if (de == null)
                return HttpNotFound();

            var dsCauHoi = db.DETHI_CAUHOIs.Where(x => x.MaD == de.MaD).ToList();
            if (!dsCauHoi.Any())
            {
                TempData["Error"] = "Đề thi không có câu hỏi.";
                return RedirectToAction("LichThi");
            }

            // ========================
            // 1️ Tạo kết quả thi trước
            // ========================
            // Khi tạo kết quả thi
            var kq = new KETQUATHI
            {
                MaHV = hv.MaHV,
                MaLT = MaLT,
                Diem = 0,
                SoCauDung = 0,
                TrangThai = "Đang làm" 
            };
            db.KETQUATHIs.InsertOnSubmit(kq);
            db.SubmitChanges();


            // ========================
            // 2️ Chấm bài + lưu chi tiết
            // ========================
            int soDung = 0;
            var listBaiLam = new List<BAILAM>();

            foreach (var ch in dsCauHoi)
            {
                string key = "CauHoi_" + ch.MaCH;
                string dapAnChon = form[key];
                var cau = db.NGANHANGCAUHOIs.Single(x => x.MaCH == ch.MaCH);

                char? dapAn = string.IsNullOrEmpty(dapAnChon) ? (char?)null : dapAnChon[0];
                if (dapAn.HasValue && dapAn.Value == cau.DapAnDung)
                    soDung++;

                listBaiLam.Add(new BAILAM
                {
                    MaKQ = kq.MaKQ,
                    MaCH = ch.MaCH,
                    DapAnChon = dapAn
                });
            }

            // Lưu tất cả bài làm
            db.BAILAMs.InsertAllOnSubmit(listBaiLam);

            // ========================
            // 3️ Tính điểm & cập nhật kết quả
            // ========================
            double diem = (double)soDung / dsCauHoi.Count * 10;

            kq.SoCauDung = soDung;
            kq.Diem = (decimal)Math.Round(diem, 2);
            kq.TrangThai = "Đã nộp"; 
            db.SubmitChanges();

            // ========================
            // 4️ Thông báo & chuyển hướng
            // ========================
            TempData["Success"] = $"Nộp bài thành công! Bạn đúng {soDung}/{dsCauHoi.Count} câu, được {Math.Round(diem, 2)} điểm.";

            // Sau khi nộp xong thì chuyển đến trang kết quả
            return RedirectToAction("KetQuaThi", new { id = kq.MaKQ });
        }

        public ActionResult KetQuaThi(int id)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;
            var kq = db.KETQUATHIs.SingleOrDefault(x => x.MaKQ == id && x.MaHV == hv.MaHV);
            if (kq == null) return HttpNotFound();

            ViewBag.SoCauDung = kq.SoCauDung;
            ViewBag.Diem = kq.Diem;
            ViewBag.MaKQ = kq.MaKQ;

            return View();
        }

        // ============================
        // ĐIỂM THI
        // ============================
        public ActionResult DiemThi()
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;

            var raw = (from kq in db.KETQUATHIs
                       join lt in db.LICHTHIs on kq.MaLT equals lt.MaLT
                       join d in db.DETHIs on lt.MaD equals d.MaD
                       join mh in db.MONs on d.MaM equals mh.MaM
                       where kq.MaHV == hv.MaHV
                       select new
                       {
                           kq.MaKQ,
                           d.TenDe,
                           Mon = mh.TenMon,
                           kq.Diem,
                           kq.SoCauDung,
                           lt.NgayThi,
                           kq.TrangThai
                       }).ToList();

            var diem = raw.Select(x => new HocVienDiemThiVM
            {
                MaKQ = x.MaKQ,
                TenDe = x.TenDe,
                Mon = x.Mon,
                Diem = x.Diem.HasValue ? (double?)Convert.ToDouble(x.Diem.Value) : null,
                SoCauDung = x.SoCauDung ?? 0,
                NgayThi = x.NgayThi,
                TrangThai = x.TrangThai
            }).ToList();

            return View(diem);
        }

        // ============================
        // ĐỔI MẬT KHẨU
        // ============================
        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            return View();
        }

        [HttpPost]
        public ActionResult DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMK)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            int maTK = Convert.ToInt32(Session["MaTK"]);
            var tk = db.TAIKHOANs.SingleOrDefault(x => x.MaTK == maTK);

            // Kiểm tra mật khẩu cũ
            if (tk.MatKhau != matKhauCu)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng!";
                return View();
            }

            // Kiểm tra mật khẩu mới
            if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 5)
            {
                ViewBag.Error = "Mật khẩu mới phải ít nhất 5 ký tự!";
                return View();
            }

            // Kiểm tra xác nhận
            if (matKhauMoi != xacNhanMK)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp!";
                return View();
            }

            // Lưu mật khẩu mới
            tk.MatKhau = matKhauMoi.Trim();
            db.SubmitChanges();

            // ✅ Xóa session để buộc login lại
            Session.Clear();
            Session.Abandon();

            // Chuyển hướng về trang login
            return RedirectToAction("Login", "Home");
        }


        // ============================
        // XEM LẠI BÀI LÀM
        // ============================
        public ActionResult XemLaiBaiLam(int id)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;

            var kq = db.KETQUATHIs.SingleOrDefault(x => x.MaKQ == id && x.MaHV == hv.MaHV);
            if (kq == null)
                return HttpNotFound();

            var lt = db.LICHTHIs.Single(x => x.MaLT == kq.MaLT);
            var de = db.DETHIs.Single(x => x.MaD == lt.MaD);
            var mon = db.MONs.Single(x => x.MaM == de.MaM);

            var raw = (from ct in db.DETHI_CAUHOIs
                       join ch in db.NGANHANGCAUHOIs on ct.MaCH equals ch.MaCH
                       join bl in db.BAILAMs.Where(xx => xx.MaKQ == id)
                           on ct.MaCH equals bl.MaCH into gj
                       from bl in gj.DefaultIfEmpty()
                       where ct.MaD == de.MaD
                       orderby ct.STT
                       select new
                       {
                           ct.STT,
                           ch.NoiDung,
                           ch.DapAnA,
                           ch.DapAnB,
                           ch.DapAnC,
                           ch.DapAnD,
                           ch.DapAnDung,
                           DapAnChon = bl != null ? bl.DapAnChon : (char?)null
                       })
                       .ToList();

            var chiTiet = raw.Select(x => new XemLaiCauHoiVM
            {
                STT = x.STT,
                NoiDung = x.NoiDung,
                DapAnA = x.DapAnA,
                DapAnB = x.DapAnB,
                DapAnC = x.DapAnC,
                DapAnD = x.DapAnD,
                DapAnDung = x.DapAnDung,
                DapAnChon = x.DapAnChon
            }).ToList();

            var vm = new XemLaiBaiLamVM
            {
                MaKQ = kq.MaKQ,
                TenDe = de.TenDe,
                MonHoc = mon.TenMon,
                SoCauDung = kq.SoCauDung ?? 0,
                Diem = kq.Diem.HasValue ? Convert.ToDouble(kq.Diem.Value) : (double?)null,
                NgayThi = lt.NgayThi,
                CauHoiList = chiTiet
            };

            return View(vm);
        }

        // ============================
        // ĐĂNG XUẤT
        // ============================
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
