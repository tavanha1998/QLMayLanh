//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP_MVC.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class KTV_BBNT
    {
        public int ID { get; set; }
        public int IDBBNT { get; set; }
        public int IDUser { get; set; }
        public Nullable<double> Diem { get; set; }
        public string DanhGia { get; set; }
        public Nullable<bool> Status { get; set; }
    
        public virtual BienBanNghiemThu BienBanNghiemThu { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}