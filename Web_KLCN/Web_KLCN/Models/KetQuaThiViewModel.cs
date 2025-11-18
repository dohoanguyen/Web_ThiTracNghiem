using System;

namespace Web_KLCN.Models
{
    public class KetQuaThiVM
    {
        public string TenDe { get; set; }       // Tên đề thi
        public string MonHoc { get; set; }      // Tên môn học
        public int SoCauDung { get; set; }      // Số câu đúng
        public int TongCau { get; set; }        // Tổng số câu
        public decimal Diem { get; set; }       // Điểm đạt được
    }
}
