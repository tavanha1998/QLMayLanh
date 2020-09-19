using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
    public class LichSuVatTu
    {
        public int MaPN { get; set; }
        public string NgayNhap { get; set; }
        public string TenVatTu { get; set; }
        public double SoLuong { get; set; }
        public int GiaMua { get; set; }
        public string NCC { get; set; }
    }
}