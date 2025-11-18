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
        // TRANG CHỦ HỌC VIÊN
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

            var dsKh = (from hl in db.HOCVIEN_LOPHOCs
                        join lh in db.LOPHOCs on hl.MaL equals lh.MaL
                        join kh in db.KHOAHOCs on lh.MaK equals kh.MaK
                        where hl.MaHV == hv.MaHV
                        select new
                        {
                            lh.MaL,
                            kh.TenKhoaHoc,
                        }).ToList();

            ViewBag.DSKhoaHoc = dsKh;
            return View(hv);
        }

        // ============================
        // THÔNG TIN CÁ NHÂN
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThongTinCaNhan(HOCVIEN model)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            try
            {
                var hv = db.HOCVIENs.SingleOrDefault(x => x.MaHV == model.MaHV);
                if (hv == null) return HttpNotFound();

                hv.TenHV = model.TenHV?.Trim();
                hv.NgaySinh = model.NgaySinh;
                hv.GioiTinh = model.GioiTinh;
                hv.Email = model.Email?.Trim();
                hv.SDT = model.SDT?.Trim();
                hv.DiaChi = model.DiaChi?.Trim();

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

            var lichThi = (from lt in db.LICHTHIs
                           join lh in db.LOPHOCs on lt.MaL equals lh.MaL
                           join pd in db.HOCVIEN_LOPHOCs on lh.MaL equals pd.MaL
                           join d in db.DETHIs on lt.MaD equals d.MaD
                           join mh in db.MONs on d.MaM equals mh.MaM
                           where pd.MaHV == hv.MaHV
                           select new HocVienLichThiVM
                           {
                               MaLT = lt.MaLT,
                               TenDe = d.TenDe,
                               MonHoc = mh.TenMon,
                               NgayThi = lt.NgayThi,
                               GioBatDau = lt.GioBatDau,
                               GioKetThuc = lt.GioKetThuc,
                               TrangThai = lt.TrangThai
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
            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == id);
            if (lt == null) return HttpNotFound();

            var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
            var mon = db.MONs.SingleOrDefault(x => x.MaM == de.MaM);

            var cauHoiList = (from ct in db.DETHI_CAUHOIs
                              join ch in db.NGANHANGCAUHOIs on ct.MaCH equals ch.MaCH
                              join dk in db.DOKHOs on ch.MaDK equals dk.MaDK
                              where ct.MaD == de.MaD
                              orderby ct.STT
                              select new BaiThiCauHoiVM
                              {
                                  MaCH = ch.MaCH,
                                  NoiDung = ch.NoiDung,
                                  MaP = ch.MaP,
                                  DapAnA = ch.DapAnA,
                                  DapAnB = ch.DapAnB,
                                  DapAnC = ch.DapAnC,
                                  DapAnD = ch.DapAnD,
                                  MaDoKho = dk.MaDK,
                                  TenDoKho = dk.TenDoKho,

                              ListYDung = db.DAPANs
                                   .Where(d => d.MaCH == ch.MaCH)
                                   .Select(d => new BaiThiCauHoiVM.DapAnVM
                                   {
                                       MaPhuongAn = d.MaPhuongAn,
                                       NoiDung = d.NoiDung
                                   }).ToList()
                              }).ToList();

            var vm = new BaiThiViewModel
            {
                MaLT = lt.MaLT,
                TenBaiThi = de.TenDe,
                MonHoc = mon.TenMon,
                ThoiGian = de.ThoiGian ?? 30,
                CauHoiList = cauHoiList
            };

            return View(vm);
        }

        // ============================
        // NỘP BÀI THI
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NopBai(int MaLT, FormCollection form)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;
            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == MaLT);
            if (lt == null) return HttpNotFound();

            var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
            var dsCauHoi = db.DETHI_CAUHOIs.Where(x => x.MaD == de.MaD).ToList();
            if (!dsCauHoi.Any())
            {
                TempData["Error"] = "Đề thi chưa có câu hỏi.";
                return RedirectToAction("LichThi");
            }

            var kq = new KETQUATHI
            {
                MaHV = hv.MaHV,
                MaLT = MaLT,
                Diem = 0,
                TrangThai = "Đang làm"
            };
            db.KETQUATHIs.InsertOnSubmit(kq);
            db.SubmitChanges();

            decimal tongDiem = 0;
            int soDung = 0;
            var listBaiLam = new List<BAILAM>();

            foreach (var ch in dsCauHoi)
            {
                string key = "CauHoi_" + ch.MaCH;
                var cau = db.NGANHANGCAUHOIs.Single(x => x.MaCH == ch.MaCH);
                string dapAnChon = form[key];
                decimal diemCong = 0;

                if (cau.MaCH == 1 || cau.MaCH == 3) // 1 đáp án hoặc tự luận
                {
                    if (!string.IsNullOrEmpty(dapAnChon) &&
                        dapAnChon.Trim().ToLower() == cau.DapAnDung.Trim().ToLower())
                    {
                        diemCong = (cau.MaCH == 1) ? 0.25m : 0.5m;
                        soDung++;
                    }
                }
                else if (cau.MaCH == 2) // nhiều đáp án
                {
                    var chonArr = dapAnChon?.Split(',') ?? new string[0];
                    var dungArr = cau.DapAnDung?.Split(',') ?? new string[0];
                    int soDungCau = chonArr.Count(a => dungArr.Contains(a));

                    if (soDungCau == 1) diemCong = 0.1m;
                    else if (soDungCau == 2) diemCong = 0.25m;
                    else if (soDungCau == 3) diemCong = 0.5m;
                    else if (soDungCau == 4) diemCong = 1m;

                    if (soDungCau == dungArr.Length) soDung++;
                }

                tongDiem += diemCong;

                listBaiLam.Add(new BAILAM
                {
                    MaKQ = kq.MaKQ,
                    MaCH = ch.MaCH,
                    DapAnChon = dapAnChon
                });
            }

            db.BAILAMs.InsertAllOnSubmit(listBaiLam);
            kq.Diem = Math.Round(tongDiem, 2);
            kq.TrangThai = "Đã nộp";
            db.SubmitChanges();

            var vm = new KetQuaThiVM
            {
                TenDe = de.TenDe,
                MonHoc = db.MONs.SingleOrDefault(m => m.MaM == de.MaM)?.TenMon ?? "",
                SoCauDung = soDung,
                TongCau = dsCauHoi.Count,
                Diem = kq.Diem ?? 0
            };

            return View("KetQuaThi", vm);
        }


        // ============================
        // LỊCH SỬ ĐIỂM
        // ============================       
        public ActionResult DiemThi()
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;

            var dsDiem = (from kq in db.KETQUATHIs
                          join lt in db.LICHTHIs on kq.MaLT equals lt.MaLT
                          join de in db.DETHIs on lt.MaD equals de.MaD
                          join mh in db.MONs on de.MaM equals mh.MaM
                          where kq.MaHV == hv.MaHV
                          orderby kq.MaKQ descending
                          select new DiemThiVM
                          {
                              MaKQ = kq.MaKQ,
                              TenDe = de.TenDe,
                              MonHoc = mh.TenMon,
                              Diem = kq.Diem ?? 0,
                              SoCauDung = kq.BAILAMs.Count(b => b.DapAnChon == b.NGANHANGCAUHOI.DapAnDung), // số câu đúng
                              TongCau = kq.BAILAMs.Count(),
                              NgayThi = lt.NgayThi,
                              TrangThai = kq.TrangThai
                          }).ToList();

            return View(dsDiem);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult NopBai(int MaLT, FormCollection form)
        //{
        //    if (Session["MaTK"] == null)
        //        return RedirectToAction("Login", "Home");

        //    var hv = Session["HOCVIEN"] as HOCVIEN;
        //    var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == MaLT);
        //    if (lt == null) return HttpNotFound();

        //    var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
        //    if (de == null) return HttpNotFound();

        //    var dsCauHoi = db.DETHI_CAUHOIs.ToList().Where(x => x.MaD == de.MaD).ToList();
        //    if (!dsCauHoi.Any())
        //    {
        //        TempData["Error"] = "Đề thi chưa có câu hỏi.";
        //        return RedirectToAction("LichThi");
        //    }

        //    var kq = new KETQUATHI
        //    {
        //        MaHV = hv.MaHV,
        //        MaLT = MaLT,
        //        Diem = 0,
        //        TrangThai = "Đang làm"
        //    };
        //    db.KETQUATHIs.InsertOnSubmit(kq);
        //    db.SubmitChanges();

        //    decimal tongDiem = 0;
        //    int soDung = 0;
        //    var listBaiLam = new List<BAILAM>();

        //    foreach (var ch in dsCauHoi)
        //    {
        //        string key = "CauHoi_" + ch.MaCH;
        //        var cau = db.NGANHANGCAUHOIs.Single(x => x.MaCH == ch.MaCH);

        //        decimal diemCong = 0;

        //        if (cau.MaCH == 1)
        //        {
        //            // 1 đáp án
        //            string dapAnChon = form[key];
        //            if (!string.IsNullOrEmpty(dapAnChon) && dapAnChon == cau.DapAnDung)
        //            {
        //                diemCong = 0.25m;
        //                soDung++;
        //            }
        //        }
        //        else if (cau.MaCH == 2)
        //        {
        //            // Nhiều đáp án đúng
        //            var chonArr = form.GetValues(key) ?? new string[0];
        //            var dapAnDungArr = cau.DapAnDung.Split(','); // "A,B,C"
        //            int soDungCau = chonArr.Count(a => dapAnDungArr.Contains(a));

        //            if (soDungCau == 1) diemCong = 0.1m;
        //            else if (soDungCau == 2) diemCong = 0.25m;
        //            else if (soDungCau == 3) diemCong = 0.5m;
        //            else if (soDungCau == 4) diemCong = 1m;

        //            if (soDungCau == dapAnDungArr.Length) soDung++;
        //        }
        //        else if (cau.MaCH == 3)
        //        {
        //            // Tự luận / điền đáp án
        //            string dapAnChon = form[key];
        //            if (!string.IsNullOrEmpty(dapAnChon) &&
        //                dapAnChon.Trim().ToLower() == cau.DapAnDung.Trim().ToLower())
        //            {
        //                diemCong = 0.5m;
        //                soDung++;
        //            }
        //        }

        //        tongDiem += diemCong;

        //        listBaiLam.Add(new BAILAM
        //        {
        //            MaKQ = kq.MaKQ,
        //            MaCH = ch.MaCH,
        //            DapAnChon = form[key]
        //        });
        //    }

        //    db.BAILAMs.InsertAllOnSubmit(listBaiLam);

        //    kq.Diem = Math.Round(tongDiem, 2);
        //    kq.TrangThai = "Đã nộp";
        //    db.SubmitChanges();

        //    TempData["Success"] = $"Nộp bài thành công! Đúng {soDung}/{dsCauHoi.Count} câu, điểm {kq.Diem}.";
        //    return RedirectToAction("KetQuaThi", new { id = kq.MaKQ });
        //}

        // ============================
        // LỚP HỌC CỦA HỌC VIÊN
        // ============================
        public ActionResult LopHoc()
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

            // Lấy danh sách lớp học của học viên
            var dsLop = (from hl in db.HOCVIEN_LOPHOCs
                         join lh in db.LOPHOCs on hl.MaL equals lh.MaL
                         join kh in db.KHOAHOCs on lh.MaK equals kh.MaK
                         where hl.MaHV == hv.MaHV
                         select new LopHocVM
                         {
                             MaL = lh.MaL,
                             TenLop = lh.TenLop,
                             TenKhoaHoc = kh.TenKhoaHoc,
                         }).ToList();

            return View(dsLop);
        }

        // ============================
        // XEM LẠI BÀI LÀM
        // ============================
        public ActionResult XemLaiBaiLam(int id) // id = MaKQ (kết quả thi)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN;

            // Lấy kết quả thi
            var kq = db.KETQUATHIs.SingleOrDefault(x => x.MaKQ == id && x.MaHV == hv.MaHV);
            if (kq == null) return HttpNotFound();

            // Lấy đề thi và câu hỏi
            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == kq.MaLT);
            if (lt == null) return HttpNotFound();

            var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
            if (de == null) return HttpNotFound();

            var cauHoiList = (from bl in db.BAILAMs
                              join ch in db.NGANHANGCAUHOIs on bl.MaCH equals ch.MaCH
                              where bl.MaKQ == kq.MaKQ
                              orderby ch.MaCH
                              select new
                              {
                                  bl,
                                  ch
                              })
                  .AsEnumerable()  // chuyển sang LINQ to Objects
                  .Select(x => new XemLaiCauHoiVM
                  {
                      STT = x.ch.MaCH,
                      NoiDung = x.ch.NoiDung,
                      DapAnDung = string.IsNullOrEmpty(x.ch.DapAnDung) ? (char?)null : x.ch.DapAnDung[0],
                      DapAnChon = string.IsNullOrEmpty(x.bl.DapAnChon) ? (char?)null : x.bl.DapAnChon[0]
                  }).ToList();


            var vm = new XemLaiBaiLamVM
            {
                TenDe = de.TenDe,
                MonHoc = db.MONs.SingleOrDefault(m => m.MaM == de.MaM)?.TenMon ?? "",
                Diem = kq.Diem.HasValue ? (double)kq.Diem.Value : 0,
                SoCauDung = cauHoiList.Count(c => c.DapAnChon == c.DapAnDung),
                CauHoiList = cauHoiList
            }; 

            return View(vm);
        }

    }
}
