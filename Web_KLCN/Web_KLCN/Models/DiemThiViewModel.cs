using System;

namespace Web_KLCN.Models
{
    public class HocVienDiemThiVM
    {
        public int MaKQ { get; set; }        // ✅ thêm vào
        public string TenDe { get; set; }
        public string Mon { get; set; }

        public double? Diem { get; set; }
        public int SoCauDung { get; set; }

        public DateTime? NgayThi { get; set; }
        public string TrangThai { get; set; }
    }

}
