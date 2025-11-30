using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
                           join kqt in db.KETQUATHIs
                               .Where(k => k.MaHV == hv.MaHV)
                               on lt.MaLT equals kqt.MaLT into kqJoin
                           from kqt in kqJoin.DefaultIfEmpty()  // Left join
                           where pd.MaHV == hv.MaHV
                           select new HocVienLichThiVM
                           {
                               MaKQ = kqt != null ? (int?)kqt.MaKQ : null, // null nếu chưa có kết quả
                               MaLT = lt.MaLT,
                               TenDe = d.TenDe,
                               MonHoc = mh.TenMon,
                               NgayThi = lt.NgayThi,
                               GioBatDau = lt.GioBatDau,
                               GioKetThuc = lt.GioKetThuc,
                               TrangThai = lt.TrangThai,
                               TrangThaiLT = kqt.TrangThai
                           }).ToList();


            return View(lichThi);
        }

        // ============================
        // LÀM BÀI THI
        // ============================
        // ============================
        // LÀM BÀI THI (GET) – ĐÃ FIX 100% LỖI RELOAD MẤT ĐÁP ÁN
        // ============================
        // =============================
        // LÀM BÀI THI – CHẮC CHẮN KHÔNG MẤT ĐÁP ÁN KỂ CẢ F5 100 LẦN
        // =============================
        [HttpGet]
        public ActionResult LamBai(int id)
        {
            if (Session["MaTK"] == null) return RedirectToAction("Login", "Home");

            var hv = Session["HOCVIEN"] as HOCVIEN ?? db.HOCVIENs.Single(x => x.MaTK == (int)Session["MaTK"]);
            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == id);
            if (lt == null || lt.TrangThai != "Đang diễn ra") return HttpNotFound();

            var de = db.DETHIs.Single(x => x.MaD == lt.MaD);
            var mon = db.MONs.SingleOrDefault(m => m.MaM == de.MaM);

            // === LẤY HOẶC TẠO KETQUATHI + BẮT ĐẦU ĐẾM GIỜ CHỈ 1 LẦN DUY NHẤT ===
            var kq = db.KETQUATHIs
                       .FirstOrDefault(k => k.MaHV == hv.MaHV &&
                                           k.MaLT == id &&
                                           k.TrangThai == "Chưa nộp");

            if (kq == null)
            {
                // Xóa bản ghi cũ nếu bị treo
                db.KETQUATHIs.DeleteAllOnSubmit(
                    db.KETQUATHIs.Where(k => k.MaHV == hv.MaHV && k.MaLT == id)
                );

                kq = new KETQUATHI
                {
                    MaHV = hv.MaHV,
                    MaLT = id,
                    ThoiGianBatDau = DateTime.Now,   // ← CHỈ GHI 1 LẦN DUY NHẤT KHI BẮT ĐẦU THI!!!
                    TrangThai = "Chưa nộp",
                    Diem = 0
                };
                db.KETQUATHIs.InsertOnSubmit(kq);
                db.SubmitChanges();
            }
            // → Nếu đã có rồi → GIỮ NGUYÊN ThoiGianBatDau → F5, tắt máy, tắt mạng → VẪN TRỪ GIỜ ĐÚNG!!!

            // === TÍNH THỜI GIAN CÒN LẠI CHUẨN TỪ SERVER (CHỐNG GIAN LẬN 100%) ===
            var thoiGianThiGiay = (de.ThoiGian ?? 30) * 60; // phút → giây
            var daSuDung = (DateTime.Now - kq.ThoiGianBatDau.Value).TotalSeconds;
            var conLaiGiay = thoiGianThiGiay - daSuDung;

            // NẾU HẾT GIỜ → TỰ ĐỘNG NỘP BÀI LUÔN (KHÔNG CHO VÀO LẠI)
            if (conLaiGiay <= 0)
            {
                kq.TrangThai = "Đã nộp";
                kq.ThoiGianNop = DateTime.Now;
                db.SubmitChanges();
                TempData["Info"] = "Hết thời gian làm bài! Hệ thống đã tự động nộp bài cho bạn.";
                return RedirectToAction("NopBai", new { MaLT = id });
            }

            // === TRUYỀN RA VIEW ĐỂ JS HIỂN THỊ ĐỒNG HỒ CHUẨN ===
            ViewBag.ThoiGianConLaiGiay = (int)Math.Floor(conLaiGiay); // số giây còn lại
            ViewBag.ThoiGianThiPhut = de.ThoiGian ?? 30;             // để hiển thị ban đầu nếu cần

            // === LOAD CÂU HỎI + BÀI LÀM NHƯ CŨ ===
            var baiLam = db.BAILAMs.Where(b => b.MaKQ == kq.MaKQ).ToList();

            var cauHoiList = (from ct in db.DETHI_CAUHOIs
                              join ch in db.NGANHANGCAUHOIs on ct.MaCH equals ch.MaCH
                              join dk in db.DOKHOs on ch.MaDK equals dk.MaDK into dkj
                              from dk in dkj.DefaultIfEmpty()
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
                                  TenDoKho = dk != null ? dk.TenDoKho : "",
                                  ListYDung = db.DAPANs
                                      .Where(d => d.MaCH == ch.MaCH)
                                      .Select(d => new BaiThiCauHoiVM.DapAnVM
                                      {
                                          MaPhuongAn = d.MaPhuongAn,
                                          NoiDung = d.NoiDung
                                      }).ToList()
                              }).ToList();

            foreach (var ch in cauHoiList)
            {
                var daChon = baiLam.FirstOrDefault(b => b.MaCH == ch.MaCH);
                if (daChon != null) ch.DapAnChon = daChon.DapAnChon;
            }

            var vm = new BaiThiViewModel
            {
                MaLT = id,
                TenDe = de.TenDe,
                MonHoc = mon?.TenMon ?? "",
                ThoiGian = de.ThoiGian ?? 30,
                CauHoiList = cauHoiList
            };

            return View(vm);
        }

        // =============================
        // LƯU NHÁP – CHẠY MƯỢT KHÔNG LỖI
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LuuNgayDapAn(int MaLT, int MaCH, string DapAnChon)
        {
            try
            {
                if (Session["MaTK"] == null) return Json(new { success = false });

                var hv = Session["HOCVIEN"] as HOCVIEN
                         ?? db.HOCVIENs.First(x => x.MaTK == (int)Session["MaTK"]);

                var kq = db.KETQUATHIs.FirstOrDefault(k =>
                    k.MaHV == hv.MaHV &&
                    k.MaLT == MaLT &&
                    k.TrangThai == "Chưa nộp");

                if (kq == null) return Json(new { success = false });

                var bl = db.BAILAMs.FirstOrDefault(b => b.MaKQ == kq.MaKQ && b.MaCH == MaCH);

                if (string.IsNullOrWhiteSpace(DapAnChon))
                {
                    if (bl != null) db.BAILAMs.DeleteOnSubmit(bl);
                }
                else
                {
                    if (bl == null)
                    {
                        bl = new BAILAM { MaKQ = kq.MaKQ, MaCH = MaCH, DapAnChon = DapAnChon.Trim() };
                        db.BAILAMs.InsertOnSubmit(bl);
                    }
                    else bl.DapAnChon = DapAnChon.Trim();
                }

                db.SubmitChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
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

            var hv = Session["HOCVIEN"] as HOCVIEN
                     ?? db.HOCVIENs.Single(x => x.MaTK == (int)Session["MaTK"]);

            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == MaLT);
            if (lt == null) return HttpNotFound();

            var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
            if (de == null) return HttpNotFound();

            var dsCauHoi = db.DETHI_CAUHOIs.Where(x => x.MaD == de.MaD).ToList();
            if (!dsCauHoi.Any())
            {
                TempData["Error"] = "Đề thi chưa có câu hỏi.";
                return RedirectToAction("LichThi");
            }

            // FIX CHÍNH: CHỈ LẤY KETQUATHI, KHÔNG BAO GIỜ TẠO MỚI Ở ĐÂY
            // CHỈ LẤY BẢN GHI ĐANG LÀM (Chưa nộp) → ĐẢM BẢO VÀO ĐOẠN TÍNH THỜI GIAN
            var kq = db.KETQUATHIs.FirstOrDefault(k => k.MaHV == hv.MaHV
                                                     && k.MaLT == MaLT
                                                     && k.TrangThai == "Chưa nộp");

            if (kq == null)
            {
                TempData["Error"] = "Không thể nộp bài! Phiên làm bài đã hết hạn hoặc không tồn tại.";
                return RedirectToAction("LichThi");
            }

            if (kq == null)
            {
                // Trường hợp này KHÔNG NÊN xảy ra nếu hệ thống chạy đúng
                // Nhưng nếu có → báo lỗi, không tạo mới → bảo vệ ThoiGianBatDau thật
                TempData["Error"] = "Không tìm thấy phiên làm bài của bạn. Có thể bạn chưa bắt đầu thi hoặc đã thoát sớm.";
                return RedirectToAction("LichThi");
            }

            // Nếu đã nộp rồi → chặn luôn
            if (kq.TrangThai == "Đã nộp")
            {
                TempData["Info"] = "Bạn đã nộp bài rồi, không thể nộp lại!";
                return RedirectToAction("LichThi");
            }

            // Xóa bài làm cũ nếu có (do reload trang nộp lại)
            var old = db.BAILAMs.Where(b => b.MaKQ == kq.MaKQ);
            db.BAILAMs.DeleteAllOnSubmit(old);
            db.SubmitChanges();

            // ================== PHẦN CHẤM ĐIỂM + LƯU BÀI LÀM (GIỮ NGUYÊN CỦA MÀY) ==================
            decimal tongDiem = 0;
            int soDung = 0;
            var listBaiLam = new List<BAILAM>();

            foreach (var ch in dsCauHoi)
            {
                string key = "CauHoi_" + ch.MaCH;
                var cau = db.NGANHANGCAUHOIs.Single(x => x.MaCH == ch.MaCH);
                string dapAnChon = "";

                if (cau.MaP == 2) // checkbox
                {
                    var arr = form.GetValues(key);
                    dapAnChon = arr != null ? string.Join(",", arr) : "";
                }
                else
                {
                    dapAnChon = form[key] ?? "";
                }

                decimal diemCong = 0;
                if (cau.MaP == 1)
                {
                    if (!string.IsNullOrEmpty(dapAnChon) &&
                        dapAnChon.Trim().ToLower() == cau.DapAnDung.Trim().ToLower())
                    {
                        diemCong = 0.25m;
                        soDung++;
                    }
                }
                else if (cau.MaP == 2)
                {
                    var arr = form.GetValues(key) ?? new string[0];
                    string rawDapAn = string.Join(",", arr);

                    if (string.IsNullOrWhiteSpace(rawDapAn) || rawDapAn.Trim().ToUpper() == "NULL")
                    {
                        dapAnChon = "NULL";
                        diemCong = 0m;
                    }
                    else
                    {
                        dapAnChon = rawDapAn;
                        var chonArr = arr.Select(x => x.Trim().ToUpper()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        var allY = db.DAPANs.Where(d => d.MaCH == cau.MaCH).ToList();
                        int soYDung = 0;
                        foreach (var y in allY)
                        {
                            bool hvChon = chonArr.Contains(y.MaPhuongAn.Trim().ToUpper());
                            bool laDung = y.DungSai == true;
                            if ((laDung && hvChon) || (!laDung && !hvChon))
                                soYDung++;
                        }
                        switch (soYDung)
                        {
                            case 1: diemCong = 0.1m; break;
                            case 2: diemCong = 0.25m; break;
                            case 3: diemCong = 0.5m; break;
                            case 4: diemCong = 1m; soDung++; break;
                            default: diemCong = 0; break;
                        }
                    }
                }
                else if (cau.MaP == 3)
                {
                    if (!string.IsNullOrEmpty(dapAnChon))
                    {
                        string daChon = new string(dapAnChon.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLower();
                        string daDung = new string(cau.DapAnDung.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLower();
                        if (daChon == daDung)
                        {
                            var mon = db.MONs.SingleOrDefault(m => m.MaM == de.MaM);
                            diemCong = (mon != null && mon.TenMon.Contains("Toán")) ? 0.5m : 0.25m;
                            soDung++;
                        }
                    }
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

            // Chỉ ghi thời gian nộp 1 lần
            if (kq.ThoiGianNop == null)
            {
                kq.ThoiGianNop = DateTime.Now;

                // TÍNH + GÁN THỜI GIAN LÀM BÀI TRƯỚC KHI SUBMIT!!!
                if (kq.ThoiGianBatDau.HasValue)
                {
                    var thoiGianLam = kq.ThoiGianNop.Value - kq.ThoiGianBatDau.Value;
                    kq.ThoiGianLamBai = TimeSpan.FromTicks(thoiGianLam.Ticks); // hoặc chỉ: = thoiGianLam;
                }
                else
                {
                    kq.ThoiGianLamBai = TimeSpan.Zero;
                }
            }

            db.SubmitChanges();
            // ==============================================================================

            var vm = new KetQuaThiVM
            {
                MaKQ = kq.MaKQ,
                TenDe = de.TenDe,
                MonHoc = db.MONs.SingleOrDefault(m => m.MaM == de.MaM)?.TenMon ?? "",
                SoCauDung = soDung,
                TongCau = dsCauHoi.Count,
                Diem = kq.Diem ?? 0,
                ThoiGianLamBai = kq.ThoiGianLamBai
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

        //Lưu nháp
        // ============================

        // ============================
        // XEM LẠI BÀI LÀM
        // ============================
        public ActionResult XemLaiBaiLam(int id)
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

            var kq = db.KETQUATHIs.SingleOrDefault(x => x.MaKQ == id && x.MaHV == hv.MaHV);
            if (kq == null) return HttpNotFound();

            var lt = db.LICHTHIs.SingleOrDefault(x => x.MaLT == kq.MaLT);
            if (lt == null) return HttpNotFound();

            var de = db.DETHIs.SingleOrDefault(x => x.MaD == lt.MaD);
            if (de == null) return HttpNotFound();

            var tatCaCauHoi = db.DETHI_CAUHOIs
                .Where(ct => ct.MaD == de.MaD)
                .OrderBy(ct => ct.STT)
                .Select(ct => ct.MaCH)
                .ToList();

            var cauHoiList = tatCaCauHoi.Select(maCH =>
            {
                var ch = db.NGANHANGCAUHOIs.Single(x => x.MaCH == maCH);
                var dk = db.DOKHOs.SingleOrDefault(d => d.MaDK == ch.MaDK);
                var bl = db.BAILAMs.FirstOrDefault(b => b.MaKQ == kq.MaKQ && b.MaCH == maCH);

                var dapAnChonStr = bl?.DapAnChon;
                var dapAnChonList = string.IsNullOrEmpty(dapAnChonStr)
                    ? new List<string>()
                    : dapAnChonStr.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

                var dapAnDungList = string.IsNullOrEmpty(ch.DapAnDung)
                    ? new List<string>()
                    : ch.DapAnDung.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

                return new BaiThiCauHoiVM
                {
                    MaCH = ch.MaCH,
                    NoiDung = ch.NoiDung,
                    DapAnA = ch.DapAnA,
                    DapAnB = ch.DapAnB,
                    DapAnC = ch.DapAnC,
                    DapAnD = ch.DapAnD,
                    DapAnDung = ch.DapAnDung,
                    DapAnChon = dapAnChonStr,
                    GiaiThich = ch.GiaiThich,
                    MaP = ch.MaP,
                    MaDoKho = dk?.MaDK ?? 0,
                    TenDoKho = dk?.TenDoKho ?? "",
                    ListYDung = db.DAPANs.Where(d => d.MaCH == ch.MaCH)
                        .Select(d => new BaiThiCauHoiVM.DapAnVM
                        {
                            MaPhuongAn = d.MaPhuongAn,
                            NoiDung = d.NoiDung,
                            DungSai = d.DungSai == true ? 1 : 0
                        }).ToList(),
                    DapAnChonList = dapAnChonList,
                    DapAnDungList = dapAnDungList
                };
            }).ToList();

            // TÍNH SỐ CÂU ĐÚNG CHUẨN 100% – ĐÃ FIX 2 LỖI
            // TÍNH SỐ CÂU ĐÚNG – CHUẨN 100% DÙ DÙNG DapAnDung HAY BẢNG DAPAN
            // === THAY TOÀN BỘ ĐOẠN TÍNH soCauDung BẰNG ĐOẠN NÀY ===
            int soCauDung = 0;
            foreach (var c in cauHoiList)
            {
                // Bỏ qua nếu chưa làm câu đó
                if (string.IsNullOrWhiteSpace(c.DapAnChon) ||
                    c.DapAnChon.Trim().ToUpper() == "NULL")
                    continue;

                bool cauHoanToanDung = false;

                // Phần I: trắc nghiệm 1 đáp án
                if (c.MaP == 1)
                {
                    cauHoanToanDung = string.Equals(c.DapAnChon?.Trim(), c.DapAnDung?.Trim(),
                                                   StringComparison.OrdinalIgnoreCase);
                }
                // Phần III: tự luận
                else if (c.MaP == 3)
                {
                    string chon = new string(c.DapAnChon.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                    string dung = new string((c.DapAnDung ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLower();
                    cauHoanToanDung = chon == dung;
                }
                // Phần II: chỉ đúng khi làm đúng 100% (4/4 ý)
                else if (c.MaP == 2)
                {
                    // Lấy danh sách ý đúng từ bảng DAPAN (ưu tiên)
                    var yDungList = db.DAPANs
                        .Where(d => d.MaCH == c.MaCH && d.DungSai == true)
                        .Select(d => d.MaPhuongAn.Trim().ToUpper())
                        .ToList();

                    // Nếu chưa có DAPAN → fallback về DapAnDung
                    if (!yDungList.Any() && !string.IsNullOrEmpty(c.DapAnDung))
                    {
                        yDungList = c.DapAnDung
                            .Split(',')
                            .Select(x => x.Trim().ToUpper())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToList();
                    }

                    // Danh sách học viên chọn
                    var chonList = c.DapAnChon
                        .Split(',')
                        .Select(x => x.Trim().ToUpper())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    // === QUY TẮC VÀNG: PHẢI CHỌN ĐÚNG VÀ ĐỦ, KHÔNG THIẾU KHÔNG THỪA ===
                    cauHoanToanDung = chonList.Count == yDungList.Count &&
                                      chonList.All(x => yDungList.Contains(x)) &&
                                      yDungList.All(x => chonList.Contains(x));
                }

                if (cauHoanToanDung)
                    soCauDung++;
            }

            var vm = new BaiThiViewModel
            {
                MaKQ = kq.MaKQ,
                TenDe = de.TenDe,
                MonHoc = db.MONs.SingleOrDefault(m => m.MaM == de.MaM)?.TenMon ?? "",
                ThoiGian = de.ThoiGian ?? 30,
                CauHoiList = cauHoiList,
                TongSoCau = cauHoiList.Count,
                SoCauDung = soCauDung,
                Diem = kq.Diem ?? 0,
                XemDapAn = lt.XemDapAn == true
            };

            return View(vm);
        }

        // ================================
        // 5. Đổi mật khẩu
        // ================================
        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            ViewData["Title"] = "Đổi mật khẩu";
            return View();
        }

        [HttpPost]
        public ActionResult DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMK)
        {
            if (Session["MaTK"] == null)
                return RedirectToAction("Login", "Home");

            int maTK = (int)Session["MaTK"];

            var taiKhoan = db.TAIKHOANs.SingleOrDefault(t => t.MaTK == maTK);
            if (taiKhoan == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản!";
                return View();
            }

            // 1️. Kiểm tra mật khẩu cũ
            if (taiKhoan.MatKhau.Trim() != matKhauCu)
            {
                ViewBag.Error = "Mật khẩu cũ không chính xác!";
                return View();
            }

            // 2️. Kiểm tra mật khẩu mới
            if (matKhauMoi.Length < 8)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 8 ký tự!";
                return View();
            }

            // 3️. Kiểm tra xác nhận
            if (matKhauMoi != xacNhanMK)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            // 4️. Cập nhật mật khẩu
            taiKhoan.MatKhau = matKhauMoi;
            db.SubmitChanges();

            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }

        public ActionResult KhoaHoc()
        {
            ViewData["Title"] = "Khóa học - TUQ Education";

            return View();
        }

        public ActionResult LichHoc(string mondayDateStr)
        {
            //// 1. Chuyển đổi chuỗi ngày nhận được từ JS (YYYY-MM-DD) sang DateTime
            //// Cần đảm bảo bạn đã include System.Globalization
            //if (!DateTime.TryParseExact(mondayDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            //                            DateTimeStyles.None, out DateTime mondayDate))
            //{
            //    return Json(new { success = false, message = "Định dạng ngày không hợp lệ." }, JsonRequestBehavior.AllowGet);
            //}

            //// 2. Tính ngày Chủ Nhật của tuần đó
            //DateTime sundayDate = mondayDate.AddDays(6);

            //// 3. Lấy MaHV
            //if (Session["MaHV"] == null)
            //{
            //    return Json(new { success = false, message = "Mã học viên không tồn tại trong Session." }, JsonRequestBehavior.AllowGet);
            //}
            //int maHv = (int)Session["MaHV"];

            //// 4. Truy vấn Lịch học - Bỏ qua trường phân loại lớp học (LoaiHoc)
            //var lichHocData = (from hl in db.HOCVIEN_LOPHOCs
            //                   where hl.MaHV == maHv
            //                   join l in db.LOPHOCs on hl.MaL equals l.MaL
            //                   join k in db.KHOAHOCs on l.MaK equals k.MaK
            //                   // Lọc theo NgayBatDau/NgayKetThuc của KHÓA HỌC
            //                   where k.NgayBatDau <= sundayDate && k.NgayKetThuc >= mondayDate

            //                   join m in db.MONs on l.MaM equals m.MaM
            //                   join gv in db.GIAOVIENs on l.MaGV equals gv.MaGV
            //                   join c in db.CAHOCs on l.MaC equals c.MaC
            //                   join ln in db.LICHNGAYs on l.MaLN equals ln.MaLN
            //                   select new
            //                   {
            //                       TenLop = l.TenLop,
            //                       TenMon = m.TenMon,
            //                       TenGV = gv.TenGV,
            //                       MaCa = l.MaC,
            //                       GioBatDau = c.GioBatDau,
            //                       GioKetThuc = c.GioKetThuc,
            //                       Thu1 = ln.Thu1,
            //                       Thu2 = ln.Thu2,
            //                       Thu3 = ln.Thu3
            //                   }).ToList();

            //// 5. Chuẩn bị dữ liệu trả về cho JS
            //var result = lichHocData.Select(x => new
            //{
            //    TenMon = x.TenMon,
            //    TenLop = x.TenLop,
            //    TenGV = x.TenGV,
            //    GioBatDau = x.GioBatDau.ToString(@"hh\:mm"),
            //    GioKetThuc = x.GioKetThuc.ToString(@"hh\:mm"),
            //    Thu1 = x.Thu1,
            //    Thu2 = x.Thu2,
            //    Thu3 = x.Thu3,
            //    MaCa = x.MaCa,
            //});

            //return Json(result, JsonRequestBehavior.AllowGet);

            ViewData["Title"] = "Lịch học - TUQ Education";

            return View();
        }
    }
}
