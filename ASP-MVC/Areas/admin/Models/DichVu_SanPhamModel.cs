using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASP_MVC.EF;
namespace ASP_MVC.Areas.admin.Models
{
    public class DichVu_SanPhamModel
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public double? TimGia(string id)
        {
            var rs = db.DichVu_SanPham.Find(id);
            return rs.DonGia;
        }
    }
}