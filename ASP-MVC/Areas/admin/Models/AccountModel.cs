using ASP_MVC.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
    public class AccountModel
    {
        private QLMayLanhEntities db = null;
        public AccountModel()
        {
            db = new QLMayLanhEntities();
        }

        public List<NhanVien> ListAll()
        {
            var rs = db.Database.SqlQuery<NhanVien>("Acc_List").ToList();
            return rs;
        }
    }
}