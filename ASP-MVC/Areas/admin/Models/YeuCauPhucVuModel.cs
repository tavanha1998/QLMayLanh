using System;
using System.Collections.Generic;
using System.Linq;
using ASP_MVC.EF;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
    public class YeuCauPhucVuModel
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public int ChangeStatus(int id)
        {
            var ycpv = db.YeuCauPhucVus.Find(id);
            if (ycpv.Status == 2)
            {
                ycpv.Status = 1;
            }
            db.SaveChanges();
            return ycpv.Status.Value;
        }
        public void Delete(int id, string lydo)
        {
            var ycpv = db.YeuCauPhucVus.Find(id);
            ycpv.Status = 4;
            db.SaveChanges();
        }
         public void ThemYCPV (YeuCauPhucVu ycpv)
        {
            db.YeuCauPhucVus.Add(ycpv);
            db.SaveChanges();
        }
    }
}