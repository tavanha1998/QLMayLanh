using ASP_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ASP_MVC.EF;
using System.Globalization;
using ASP_MVC.Areas.KTV.Models;

namespace ASP_MVC.Areas.KTV.Controllers
{
    [Authorize(Roles = "3")]
    public class HomeController : Controller
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        // GET: KTV/Home
        public ActionResult Index(int? lastMonth)
        {
            string name = HttpContext.User.Identity.Name;
            List<BienBanNghiemThu> list = new List<BienBanNghiemThu>();
            List<CTBienBanNghiemThu> listct = new List<CTBienBanNghiemThu>();
            if (!String.IsNullOrEmpty(name))
            {
                var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
                var ktv_bbnt = db.KTV_BBNT.Where(x => x.IDUser == ktv.ID);
                if (lastMonth == null)
                {
                    ktv_bbnt = ktv_bbnt.Where(x=>x.BienBanNghiemThu.NgayLap.Value.Month == DateTime.Now.Month);
                }
                else
                    ktv_bbnt = ktv_bbnt.Where(x => x.IDUser == ktv.ID && x.BienBanNghiemThu.NgayLap.Value.Month == lastMonth);
                foreach (var item in ktv_bbnt)
                {
                    BienBanNghiemThu bbnt = new BienBanNghiemThu(); 
                    bbnt = db.BienBanNghiemThus.Find(item.IDBBNT);
                    list.Add(bbnt);
                }
                foreach (BienBanNghiemThu bb in list)
                {
                    var ct = db.CTBienBanNghiemThus.Where(x=>x.IDBBNT == bb.ID);
                    foreach(var item in ct)
                    {
                        listct.Add(item);
                    }
                }
                ViewBag.BBNT = list;
                ViewBag.KTV = ktv_bbnt;
                ViewBag.NV = ktv;
            }
            
            return View(listct);
        }
        [ChildActionOnly]
        public ActionResult thangTruoc()
        {
            string name = HttpContext.User.Identity.Name;
            float diemtn = 0; // diem thang nay
            float diemtr = 0; // diem thang truoc
            if (!String.IsNullOrEmpty(name))
            {
                var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
                var diemThangNay = db.KTV_BBNT.Where(x => x.IDUser == ktv.ID && x.BienBanNghiemThu.NgayLap.Value.Month == DateTime.Now.Month);
                foreach(var item in diemThangNay)
                {
                    diemtn += float.Parse(item.Diem.ToString());
                }
                var diemThangTruoc = db.KTV_BBNT.Where(x => x.IDUser == ktv.ID && x.BienBanNghiemThu.NgayLap.Value.Month == DateTime.Now.Month - 1);
                foreach (var item in diemThangTruoc)
                {
                    diemtr += float.Parse(item.Diem.ToString());
                }
                ViewBag.NV = ktv;
            }
            ViewBag.diemtn = diemtn;
            ViewBag.diemtr = diemtr;
            
            return PartialView();
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        public ActionResult ChangePassWord(string Error)
        {
            if (!String.IsNullOrEmpty(Error))
                ViewBag.Error = Error;
            return View();
        }
        [HttpPost]
        public ActionResult ChangePass(string oldPass, string NewPass1, string NewPass2)
        {
            if (!String.IsNullOrEmpty(oldPass) && !String.IsNullOrEmpty(NewPass1) && !String.IsNullOrEmpty(NewPass2))
            {
                string name = HttpContext.User.Identity.Name;
                var rs = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status==1);
                if (NewPass1.Length < 5)
                    ViewBag.Error = "Mật khẩu có độ dài từ 5 kí tự";
                else if(rs != null)
                {
                    LoginModel model = new LoginModel();
                    if (rs.Password.Equals(model.CreateMD5(model.Base64Encode(oldPass))))
                    {
                        if (NewPass1.Equals(NewPass2))
                        {
                            rs.Password = model.CreateMD5(model.Base64Encode(NewPass1));
                            db.SaveChanges();
                            TempData["msg"] = "<script>alert('Thành công');</script>";
                            return RedirectToAction("Index", "Home");
                        }
                        else
                            ViewBag.Error = "Mật khẩu mới không khớp!";
                    }
                    else
                        ViewBag.Error = "Mật khẩu cũ không đúng";
                }
                else
                    ViewBag.Error = "Có lỗi xảy ra";
            }
            else
                ViewBag.Error = "Vui lòng nhập đủ thông tin";
            return RedirectToAction("ChangePassWord", "Home", new { ViewBag.Error });
        }

        [HttpPost]
        public JsonResult LoadDungCu(long idktv)
        {
            var ctpx = db.CTPhieuXuatKhoes.Where(x => x.PhieuXuatKho.IDKTV == idktv).OrderBy(x=>x.PhieuXuatKho.Status);
            List<DungCu> list = new List<DungCu>();
            foreach(var item in ctpx)
            {
                DungCu dc = new DungCu();
                dc.TenDC = item.KhoVatDung.TenVatDung;
                dc.NgayXuat = item.PhieuXuatKho.NgayXuat.Value.ToString();
                dc.NgayTra = item.PhieuXuatKho.NgayTra.HasValue? item.PhieuXuatKho.NgayTra.Value.ToString(): "";
                dc.GhiChu = item.KhoVatDung.GhiChu;
                if (item.PhieuXuatKho.Status == 1)
                    dc.TinhTrang = "Đã trả";
                else
                    dc.TinhTrang = "Chưa trả";
                list.Add(dc);
            }
            return Json(new
            {
                data = list,
                status=true
            },JsonRequestBehavior.AllowGet);
        }
    }
}
