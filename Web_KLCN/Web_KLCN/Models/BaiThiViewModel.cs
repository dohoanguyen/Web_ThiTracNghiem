using System.Collections.Generic;

namespace Web_KLCN.Models
{
    public class BaiThiViewModel
    {
        public int MaLT { get; set; }
        public string TenBaiThi { get; set; }
        public string MonHoc { get; set; }
        public int ThoiGian { get; set; }

        public List<BaiThiCauHoiVM> CauHoiList { get; set; }
    }

    public class BaiThiCauHoiVM
    {
        public int MaCH { get; set; }
        public string NoiDung { get; set; }

        public string DapAnA { get; set; }
        public string DapAnB { get; set; }
        public string DapAnC { get; set; }
        public string DapAnD { get; set; }
    }
}
