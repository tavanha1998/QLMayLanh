using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
    public class ListKTVphutrach
    {
        public int ID { get; set; }
        public string TenKTV { get; set; }
        public float Diem { get; set; }
        public string DanhGia { get; set; }
    }
}