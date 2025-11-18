using System;
using System.Collections.Generic;

namespace Web_KLCN.Models
{
    // ViewModel cho từng câu hỏi khi xem lại
    public class XemLaiCauHoiVM
    {
        public int STT { get; set; }                  // Số thứ tự câu hỏi
        public string NoiDung { get; set; }           // Nội dung câu hỏi

        public string DapAnA { get; set; }
        public string DapAnB { get; set; }
        public string DapAnC { get; set; }
        public string DapAnD { get; set; }

        public char? DapAnDung { get; set; }         // Đáp án đúng (A/B/C/D)
        public char? DapAnChon { get; set; }         // Đáp án học viên chọn

        public int MaP { get; set; }                 // Loại câu hỏi: 1=1 đáp án, 2=nhiều đáp án, 3=textbox
        public int MaDoKho { get; set; }             // Mã độ khó
        public string TenDoKho { get; set; }         // Tên độ khó (Nhận biết / Thông hiểu / ...)
    }

    // ViewModel tổng quan bài làm
    public class XemLaiBaiLamVM
    {
        public int MaKQ { get; set; }                // Mã kết quả
        public string TenDe { get; set; }            // Tên đề thi
        public string MonHoc { get; set; }           // Môn học

        public int SoCauDung { get; set; }           // Số câu trả lời đúng
        public double? Diem { get; set; }            // Điểm số
        public DateTime? NgayThi { get; set; }       // Ngày thi

        public List<XemLaiCauHoiVM> CauHoiList { get; set; } = new List<XemLaiCauHoiVM>();
    }
}
