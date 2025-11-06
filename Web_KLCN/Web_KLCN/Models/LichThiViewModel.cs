using System;

namespace Web_KLCN.Models
{
    public class HocVienLichThiVM
    {
        public int MaLT { get; set; }
        public string TenDe { get; set; }
        public string Mon { get; set; }

        public DateTime NgayThi { get; set; }
        public string GioBatDau { get; set; }
        public string GioKetThuc { get; set; }

        public string TrangThai { get; set; }   // Đang mở / Chưa mở / Đã thi…
    }
}
