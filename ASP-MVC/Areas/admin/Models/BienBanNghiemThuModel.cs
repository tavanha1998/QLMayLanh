using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASP_MVC.EF;

namespace ASP_MVC.Areas.admin.Models
{
    public class BienBanNghiemThuModel
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public int ThemBBNT(int idyc)
        {
            BienBanNghiemThu bbnt = new BienBanNghiemThu();
            bbnt.IDYC = idyc;
            bbnt.NgayLap = DateTime.Now.Date;
            bbnt.Status = true;
            bbnt.DoanhThu = 0;
            bbnt.Diem = 0;
            db.BienBanNghiemThus.Add(bbnt);
            db.SaveChanges();
            return bbnt.ID;
        }
    }
}