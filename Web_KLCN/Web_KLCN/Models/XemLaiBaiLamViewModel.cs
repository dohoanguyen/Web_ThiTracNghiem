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

        public string DapAnDung { get; set; }
        public string DapAnChon { get; set; }
        public string GiaiThich { get; set; }

        public int MaP { get; set; }                 // Radio - Checkbox - Textbox
        public int MaDoKho { get; set; }             
        public string TenDoKho { get; set; }         //Nhận biết  - Thông hiểu - Vận dụng - Vận dụng cao
        
        public List<BaiThiCauHoiVM.DapAnVM> ListYDung { get; set; } = new List<BaiThiCauHoiVM.DapAnVM>();
        public List<string> DapAnChonList { get; set; } = new List<string>();
        public List<string> DapAnDungList { get; set; } = new List<string>();
    }

    public class XemLaiBaiLamVM
    {
        public int MaKQ { get; set; }             
        public string TenDe { get; set; }            
        public string MonHoc { get; set; }           

        public int SoCauDung { get; set; }           
        public double? Diem { get; set; }            
        public DateTime? NgayThi { get; set; }       

        public List<XemLaiCauHoiVM> CauHoiList { get; set; } = new List<XemLaiCauHoiVM>();
    }
}
