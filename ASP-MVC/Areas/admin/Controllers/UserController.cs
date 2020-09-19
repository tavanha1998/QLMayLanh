using ASP_MVC.Models;
using ASP_MVC.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ASP_MVC.Areas.admin.Models;
using System.Text.RegularExpressions;
using ASP_MVC.Areas.KTV.Models;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0")]

    public class UserController : Controller
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        // GET: admin/MayBaoTri
        public ActionResult Index()
        {
            IQueryable<NhanVien> model = db.NhanViens.Where(x=>x.Status != 2);
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    model = model.Where(x => x.TenKTV.Contains(searchString) || x.SDT.Contains(searchString));
            //}
            return View(model.OrderBy(x => x.ID));
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NhanVien nhanVien, LoginModel model)
        {
            if (ModelState.IsValid)
            {
                string reg = @"^[a-zA-Z]{1}[a-zA-Z0-9]{0,23}$";
                if (Regex.IsMatch(nhanVien.Username,reg) && nhanVien.Username.Length > 5)
                {
                    nhanVien.Password = model.CreateMD5(model.Base64Encode("Aa123456"));
                    db.NhanViens.Add(nhanVien);
                    TestObject test = new TestObject();
                    string exception = test.TestNhanViens(nhanVien.Username, nhanVien.TenKTV, nhanVien.SDT, true);
                    if (exception.Equals("OK"))
                    {
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", exception);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Username không hợp lệphải có từ 6 kí tự và không dấu");
                }
            }
            return View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NhanVien nhanVien_edit, int id)
        {
            if (ModelState.IsValid)
            {
                NhanVien nhanVien = db.NhanViens.Find(id);

                nhanVien.Username = nhanVien_edit.Username;
                nhanVien.TenKTV = nhanVien_edit.TenKTV;
                nhanVien.SDT = nhanVien_edit.SDT;
                nhanVien.Loai = nhanVien_edit.Loai;
                nhanVien.Status = nhanVien_edit.Status;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public JsonResult RenewPassword(int idktv)
        {
            LoginModel model = new LoginModel();
            NhanVien nhanVien = db.NhanViens.Find(idktv);
            nhanVien.Password = model.CreateMD5(model.Base64Encode("Aa123456"));
            db.SaveChanges();
            return Json(new { status = true, mess = "Thành công" });
                //TempData["msg"] = "<script>alert('Reset thành công');</script>";
                
        }

        [HttpPost]
        public JsonResult ResetAll(int idktv)
        {
            LoginModel model = new LoginModel();
            NhanVien nhanVien = db.NhanViens.Find(idktv);
            nhanVien.Status = 2;
            NhanVien newNhanVien = new NhanVien();
            newNhanVien.TenKTV=nhanVien.TenKTV;
            newNhanVien.Loai = nhanVien.Loai;
            newNhanVien.Password = model.CreateMD5(model.Base64Encode("Aa123456"));
            newNhanVien.SDT = nhanVien.SDT;
            newNhanVien.Status = 1;
            newNhanVien.Username = nhanVien.Username;
            db.NhanViens.Add(newNhanVien);
            db.SaveChanges();
            return Json(new { status = true, mess = "Thành công" });
            //TempData["msg"] = "<script>alert('Reset thành công');</script>";

        }

        [HttpPost]
        public JsonResult Update(string nv)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            NhanVien nhanVien_edit = serializer.Deserialize<NhanVien>(nv);

            if(!nhanVien_edit.TenKTV.Equals("") && !nhanVien_edit.SDT.Equals(""))
            {
                NhanVien nhanVien = db.NhanViens.SingleOrDefault(x => x.ID == nhanVien_edit.ID);
                nhanVien.SDT = nhanVien_edit.SDT;
                nhanVien.TenKTV = nhanVien_edit.TenKTV;
                nhanVien.Loai = nhanVien_edit.Loai;
                nhanVien.Status = nhanVien_edit.Status;
                db.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }
            else
                return Json(new
                {
                    status = false
                });
        }
        [HttpPost]
        public JsonResult LoadDiem(int idnv)
        {
            var ktv = db.NhanViens.Find(idnv);
            int thang = 1;
            float diem = 0;
            List<DSDiemKTV> list = new List<DSDiemKTV>();
            if(ktv != null)
            {
                var ktv_bbnt = db.KTV_BBNT.Where(x => x.IDUser == ktv.ID);
                for(int i = 1;i<=12;i++)
                {
                    ktv_bbnt = ktv_bbnt.Where(x => x.BienBanNghiemThu.NgayLap.Value.Month == i);
                    DSDiemKTV ds = new DSDiemKTV();
                    foreach (var item in ktv_bbnt)
                    {
                        diem = diem + float.Parse(item.Diem.Value.ToString());
                    }
                    ds.Thang = thang++;
                    ds.Diem = diem;
                    list.Add(ds);
                    diem = 0;
                }
            }
            return Json(new
            {
                data=list,
                status=true
            });
        }

        [HttpPost]
        public JsonResult LoadDungCu(long idktv)
        {
            var ctpx = db.CTPhieuXuatKhoes.Where(x => x.PhieuXuatKho.IDKTV == idktv && x.PhieuXuatKho.Status == 0);
            List<DungCu> list = new List<DungCu>();
            foreach (var item in ctpx)
            {
                DungCu dc = new DungCu();
                dc.TenDC = item.KhoVatDung.TenVatDung;
                dc.NgayXuat = item.PhieuXuatKho.NgayXuat.Value.ToString();
                dc.GhiChu = item.KhoVatDung.GhiChu;
                list.Add(dc);
            }
            return Json(new
            {
                data = list,
                status = true
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
