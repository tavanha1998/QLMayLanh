using ASP_MVC.Areas.KTV.Models;
using ASP_MVC.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_MVC.Areas.KTV.Controllers
{
    public class KhoDungCuController : Controller
    {
        // GET: KTV/KhoDungCu
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public ActionResult Index(string searchString, int page = 1, int pagesize = 20)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            IQueryable<PhieuXuatKho> model = db.PhieuXuatKhoes.Where(x=>x.IDKTV == ktv.ID);
            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.search = Request.QueryString["searchString"].ToString();
                try
                {
                    DateTime dt = Convert.ToDateTime(searchString);
                    model = model.Where(x => DbFunctions.TruncateTime(x.NgayXuat) == dt);
                }
                catch
                {
                    int a;
                    if (Int32.TryParse(searchString, out a))
                        model = model.Where(x => x.ID == a);
                }
            }
            return View(model.OrderBy(x => x.Status).ToPagedList(page, pagesize));
        }
        [HttpPost]
        public JsonResult LoadDungCu()
        {
            // string name = HttpContext.User.Identity.Name;
            //var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            var rs = db.KhoVatDungs.Where(x => x.Status == 1);
            List<KhoVatDung> list = new List<KhoVatDung>();
            foreach(KhoVatDung item in rs)
            {
                KhoVatDung vd = new KhoVatDung();
                vd.ID = item.ID;
                vd.TenVatDung = item.TenVatDung;
                list.Add(vd);
            }
            return Json(new { 
                data=list,status=true
            },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LapPhieu(List<int> listDC)
        {
            if(listDC == null)
                return Json(new
                {
                    status = false,
                    mess="Bạn chưa chọn dụng cụ"
                });
            else
            {
                string name = HttpContext.User.Identity.Name;
                var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
                if(ktv == null)
                    return Json(new
                    {
                        status = false,
                        mess = "Có lỗi xảy ra"
                    });
                PhieuXuatKho px = new PhieuXuatKho();
                px.IDKTV = ktv.ID;
                px.KiemDuyet = false;
                px.NgayXuat = DateTime.Now;
                px.Status = 0;
                foreach (int value in listDC)
                {
                    var vd = db.KhoVatDungs.Find(value);
                    if (vd == null || vd.Status == 2)
                        return Json(new
                        {
                            status = false,
                            mess = "Có lỗi xảy ra"
                        });
                    else
                        vd.Status = 2;
                    CTPhieuXuatKho ct = new CTPhieuXuatKho();
                    ct.IDVatDung = value;
                    px.CTPhieuXuatKhoes.Add(ct);
                }
                db.PhieuXuatKhoes.Add(px);
                db.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }
            
        }
        [HttpPost]
        public JsonResult CTPhieu(int idp)
        {
            var rs = db.CTPhieuXuatKhoes.Where(x => x.IDPhieuXuat == idp);
            if(rs == null)
                return Json(new
                {
                    status = true,
                });
            List<DungCu> list = new List<DungCu>();
            foreach(CTPhieuXuatKho item in rs)
            {
                DungCu dc = new DungCu();
                dc.TenDC = item.KhoVatDung.TenVatDung;
                dc.NgayXuat = item.PhieuXuatKho.NgayXuat.Value.ToString("dd/MM/yyyy");
                list.Add(dc);
            }
            return Json(new
            {
                data=list,
                status = true
            },JsonRequestBehavior.AllowGet);
        }
    }
}