using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
	public class CTBBNTnew
	{
        public int ID { get; set; }
        public string TenDVSP { get; set; }
        public double SoLuong { get; set; }
        public double DonGia { get; set; }
        public string Ma { get; set; }
        public long CPDauVao { get; set; }
        public string GhiChu { get; set; }
        public string NgayPV { get; set; }
    }
}