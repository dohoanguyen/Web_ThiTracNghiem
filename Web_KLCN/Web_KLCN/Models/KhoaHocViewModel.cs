using System;
using System.Collections.Generic;

namespace Web_KLCN.Models
{
    public class KhoaHocVM
    {
        public int MaKH { get; set; }
        public string TenKhoaHoc { get; set; }
        public string DanhMuc { get; set; }
        public string MoTa { get; set; }
        public string CapDo { get; set; }
        public string AnhKhoaHoc { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoHocVien { get; set; }
    }
}
