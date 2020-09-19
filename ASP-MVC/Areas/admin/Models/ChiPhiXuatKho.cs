using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
    public class ChiPhiXuatKho
    {
        public int ID { get; set; }
        public string TenVatTu { get; set; }
        public double SoLuongLay { get; set; }
        public double SoLuongTT { get; set; }
        public double DonGia { get; set; }
        public double ThanhTien { get; set; }
        public string TenKTV { get; set; }
    }
}