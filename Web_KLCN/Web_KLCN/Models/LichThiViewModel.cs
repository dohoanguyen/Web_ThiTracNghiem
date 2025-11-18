using System;

namespace Web_KLCN.Models
{
    /// <summary>
    /// ViewModel hiển thị lịch thi của học viên
    /// </summary>
    public class HocVienLichThiVM
    {
        public int MaLT { get; set; }            // Mã lượt thi (LUOTTHI.MaLT)
        public string TenDe { get; set; }        // Tên đề thi (DETHI.TenDe)
        public string MonHoc { get; set; }       // Tên môn học (DETHI.MonHoc)

        public DateTime NgayThi { get; set; }    // Ngày thi (LICHTHI.NgayThi)
        public TimeSpan GioBatDau { get; set; }  // Giờ bắt đầu thi
        public TimeSpan GioKetThuc { get; set; } // Giờ kết thúc thi

        public string TrangThai { get; set; }    // "Chưa mở", "Đang mở", "Đã thi"

        /// <summary>
        /// Helper hiển thị giờ dưới dạng chuỗi (ví dụ "08:30")
        /// </summary>
        public string GioBatDauStr => GioBatDau.ToString(@"hh\:mm");
        public string GioKetThucStr => GioKetThuc.ToString(@"hh\:mm");
    }
}
