using System;

namespace Web_KLCN.Models
{
    public class DiemThiVM
    {
        public int MaKQ { get; set; }          
        public string TenDe { get; set; }      
        public string MonHoc { get; set; }     
        public decimal Diem { get; set; }       
        public int SoCauDung { get; set; }      
        public int TongCau { get; set; }       
        public DateTime? NgayThi { get; set; }  
        public string TrangThai { get; set; }
    }
}
