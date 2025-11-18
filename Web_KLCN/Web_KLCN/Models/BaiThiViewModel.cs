using System.Collections.Generic;

namespace Web_KLCN.Models
{
    /// <summary>
    /// ViewModel cho trang làm bài thi của học viên
    /// </summary>
    public class BaiThiViewModel
    {
        public int MaLT { get; set; }                // Mã lượt thi
        public string TenBaiThi { get; set; }        // Tên bài thi (VD: "Đề thi giữa kì Toán 12")
        public string MonHoc { get; set; }           // Môn học
        public int ThoiGian { get; set; }            // Thời gian làm bài (phút)
        public int TongSoCau { get; set; }           // Tổng số câu trong bài thi

        public List<BaiThiCauHoiVM> CauHoiList { get; set; } = new List<BaiThiCauHoiVM>();
    }

    /// <summary>
    /// ViewModel cho từng câu hỏi trong bài thi
    /// </summary>
    public class BaiThiCauHoiVM
    {
        public int MaCH { get; set; }
        public string NoiDung { get; set; }

        public string DapAnA { get; set; }
        public string DapAnB { get; set; }
        public string DapAnC { get; set; }
        public string DapAnD { get; set; }

        public string DapAnDung { get; set; }
        public string DapAnChon { get; set; }

        public int ThuTu { get; set; }

        public int MaP { get; set; } // 1: Một đáp án, 2: Nhiều đáp án, 3: Tự luận

        public int MaDoKho { get; set; }    // 1-4
        public string TenDoKho { get; set; } //Nhận biết, Thông hiểu, Vận dụng, Vận dụng cao

        public List<DapAnVM> ListYDung { get; set; } = new List<DapAnVM>();

        public class DapAnVM
        {
            public string MaPhuongAn { get; set; } // I, II, III, IV
            public string NoiDung { get; set; }     // Nội dung đáp án
            public int DungSai { get; set; }        // 1 = đúng, 0 = sai
        }
    }
}
