using System;
using System.Collections.Generic;

namespace Web_KLCN.Models
{
    public class XemLaiCauHoiVM
    {
        public int STT { get; set; }
        public string NoiDung { get; set; }

        public string DapAnA { get; set; }
        public string DapAnB { get; set; }
        public string DapAnC { get; set; }
        public string DapAnD { get; set; }

        public char? DapAnDung { get; set; }
        public char? DapAnChon { get; set; }
    }

    public class XemLaiBaiLamVM
    {
        public int MaKQ { get; set; }
        public string TenDe { get; set; }
        public string MonHoc { get; set; }

        public int SoCauDung { get; set; }
        public double? Diem { get; set; }
        public DateTime? NgayThi { get; set; }

        public List<XemLaiCauHoiVM> CauHoiList { get; set; }
    }
}
