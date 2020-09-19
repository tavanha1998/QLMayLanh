using ASP_MVC.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASP_MVC.Areas.admin.Models;
using System.Web.Script.Serialization;
using PagedList;
using System.Data;
using System.Web.Security;
using ASP_MVC.Models;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0,1,2")]
    public class AdminController : Controller
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        // GET: admin/Admin
        [NonAction]
        public void setViewBagQL()
        {
            ViewBag.QLNL = true;
        }
        // Loai 1: Sua chua/ vsml   Loai 3: Lap may     Loai 2: Thue
        public ActionResult Index()
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if(ktv.Loai != 0)
            {
                setViewBagQL();
            }
            IQueryable<YeuCauPhucVu> p = db.YeuCauPhucVus;
            List<YeuCauPhucVu> listPV = new List<YeuCauPhucVu>();
            //if (!string.IsNullOrEmpty(searchStringPV))
            //{
            //    ViewBag.searchPV = searchStringPV;
            //    p = p.Where(x => (x.KhachHang.HoTen.Contains(searchStringPV) || x.KhachHang.SDT.Contains(searchStringPV)) && (x.Loai == 1 || x.Loai == 3) && x.Status != 3 && x.Status != 4);
            //    listPV = p.ToList();
            //}
            listPV = db.YeuCauPhucVus.Where(x=>(x.Loai == 1 || x.Loai == 3) && x.Status != 3 && x.Status != 4).ToList();
            for (int i = 0; i < listPV.Count; i++)
            {
                if (listPV[i].NgayLamTiep.HasValue )
                {
                    if (listPV[i].NgayLamTiep <= DateTime.Now.Date && listPV[i].Status != 1)
                    {
                        listPV[i].Status = 2;
                        var rs = db.YeuCauPhucVus.Find(listPV[i].ID);
                        rs.Status = 2;
                        db.SaveChanges();
                    }
                    else if(listPV[i].NgayLamTiep > DateTime.Now.Date)
                    {
                        listPV[i].Status = 0;
                        var rs = db.YeuCauPhucVus.Find(listPV[i].ID);
                        rs.Status = 0;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (listPV[i].NgayBatDau <= DateTime.Now.Date && listPV[i].Status == 0)
                    {
                        listPV[i].Status = 2;
                        var rs = db.YeuCauPhucVus.Find(listPV[i].ID);
                        rs.Status = 2;
                        db.SaveChanges();
                    }
                    else if(listPV[i].NgayBatDau > DateTime.Now.Date)
                    {
                        listPV[i].Status = 0;
                        var rs = db.YeuCauPhucVus.Find(listPV[i].ID);
                        rs.Status = 0;
                        db.SaveChanges();
                    }

                }
            }

            List<YeuCauPhucVu> listCT = new List<YeuCauPhucVu>(); 
            //if (!string.IsNullOrEmpty(searchStringCT))
            //{
            //    ViewBag.searchCT = searchStringCT;
            //    p = p.Where(x => (x.KhachHang.HoTen.Contains(searchStringCT) || x.KhachHang.SDT.Contains(searchStringCT)) && x.Loai == 2 && x.Status != 3 && x.Status != 4);
            //    listCT = p.ToList();
            //}
            //else
                listCT= db.YeuCauPhucVus.Where(x => x.Loai == 2 && x.Status != 3 && x.Status != 4).ToList();
            for (int i = 0; i < listCT.Count; i++)
            {
                if (listCT[i].NgayDuKienTra.HasValue)
                {
                    if (listCT[i].NgayDuKienTra <= DateTime.Now.Date && listCT[i].Status != 1)
                    {
                        listCT[i].Status = 2;
                        var rs = db.YeuCauPhucVus.Find(listCT[i].ID);
                        rs.Status = 2;
                        db.SaveChanges();
                    }
                    else if(listCT[i].NgayDuKienTra > DateTime.Now.Date)
                    {
                        listCT[i].Status = 0;
                        var rs = db.YeuCauPhucVus.Find(listCT[i].ID);
                        rs.Status = 0;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (listCT[i].NgayBatDau <= DateTime.Now.Date && listCT[i].Status == 0)
                    {
                        listCT[i].Status = 2;
                        var rs = db.YeuCauPhucVus.Find(listCT[i].ID);
                        rs.Status = 2;
                        db.SaveChanges();
                    }
                    else if (listCT[i].NgayBatDau > DateTime.Now.Date)
                    {
                        listCT[i].Status = 0;
                        var rs = db.YeuCauPhucVus.Find(listCT[i].ID);
                        rs.Status = 0;
                        db.SaveChanges();
                    }

                }
            }
            
            ViewBag.YCCT = listCT.OrderByDescending(x => x.Status);
            var model = listPV.OrderByDescending(x=>x.Status);
            return View(model);
        }
        [HttpGet]
        public ActionResult Create()
        {
            List<KhachHang> list = db.KhachHangs.ToList();
            ViewBag.DSKH = new SelectList(list, "ID", "HoTen");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(YeuCauPhucVu ycpv)
        {
            ViewBag.DSKH = db.KhachHangs.ToList();
            YeuCauPhucVuModel model = new YeuCauPhucVuModel();
            if (ycpv.Loai==1)
            {
                if (ModelState.IsValid)
                {
                    ycpv.NgayLap = DateTime.Now.Date;
                    ycpv.Status = 0;
                    model.ThemYCPV(ycpv);
                    return RedirectToAction("Index");
                }
            }
            else if(ycpv.Loai==2 && ycpv.NgayDuKienTra >= DateTime.Now.Date)
            {
                if(ModelState.IsValid)
                {
                    ycpv.NgayLap = DateTime.Now.Date;
                    ycpv.Status = 0;
                    model.ThemYCPV(ycpv);
                    return RedirectToAction("Index");
                }
            }
            else if (ycpv.Loai == 3)
            {
                ycpv.NgayLap = DateTime.Now.Date;
                ycpv.Status = 0;
                model.ThemYCPV(ycpv);
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = "Vui lòng nhập ngày trả dự kiến hoặc sai định dạng!";
            }
            return View(ycpv);
        }

        [HttpPost]
        public JsonResult searchKH(string s)
        {
            List<KhachHang> rs = db.KhachHangs.Where(x => x.HoTen.Contains(s) || x.SDT.Contains(s)).ToList();
            ViewBag.dsKH = rs;
            return Json(new {
                status=true
            });
        }
        
        [HttpPost]
        public JsonResult ChangeStatus (int id)
        {
            var rs = new YeuCauPhucVuModel().ChangeStatus(id);
            return Json(new
            {
                status = rs
            });
        }
        [HttpPost]
        public JsonResult Delete (string model)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            YeuCauPhucVu ycpv = serializer.Deserialize<YeuCauPhucVu>(model);
            var rs = db.YeuCauPhucVus.Find(ycpv.ID);
            if (string.IsNullOrEmpty(ycpv.LiDo))
            {
                rs.LiDo = "không có";
            }
            else
                rs.LiDo = ycpv.LiDo;
            rs.NgayHoanThanh = DateTime.Now.Date;
            rs.Status = 4;
            db.SaveChanges();
            return Json(new
            {
                status = true,
            });
        }
        // Tại sao nó nằm đây?
        [ChildActionOnly]
        public ActionResult Search()
        {
            var rs = db.KhachHangs.ToList();
            return PartialView(rs);
        }
        public ActionResult YCHoanThanh()
        {
            IQueryable<YeuCauPhucVu> model = db.YeuCauPhucVus;
            //if (!string.IsNullOrEmpty(searchStringPV))
            //{
            //    model = model.Where(x => x.Status == 3 && (x.Loai == 1 || x.Loai == 3) && (x.KhachHang.HoTen.Contains(searchStringPV) || x.KhachHang.SDT.Contains(searchStringPV))).OrderByDescending(x => x.NgayHoanThanh);
            //}
            //else
                model = model.Where(x => x.Status == 3 && (x.Loai == 1 || x.Loai == 3)).OrderByDescending(x => x.NgayHoanThanh);

            //if (!string.IsNullOrEmpty(searchStringCT))
            //{
            //    ViewBag.YCCT = db.YeuCauPhucVus.Where(x => x.Status == 3 && x.Loai == 2 && (x.KhachHang.HoTen.Contains(searchStringCT) || x.KhachHang.SDT.Contains(searchStringCT))).OrderByDescending(x => x.NgayHoanThanh).ToPagedList(page, pagesize);
            //}
            //else
                ViewBag.YCCT = db.YeuCauPhucVus.Where(x => x.Status == 3 && x.Loai == 2).OrderByDescending(x => x.NgayHoanThanh);

            return View(model);
        }
        public ActionResult YCHuy()
        {
            IQueryable<YeuCauPhucVu> model = db.YeuCauPhucVus;
            //if (!string.IsNullOrEmpty( searchStringPV))
            //{
            //    model = model.Where(x => x.Status == 4 && (x.Loai == 1 || x.Loai == 3) && (x.KhachHang.HoTen.Contains(searchStringPV) || x.KhachHang.SDT.Contains(searchStringPV))).OrderByDescending(x => x.NgayHoanThanh);
            //}
            //else
                model = model.Where(x => x.Status == 4 && (x.Loai == 1 || x.Loai == 3)).OrderByDescending(x => x.NgayHoanThanh);

            //if (!string.IsNullOrEmpty(searchStringCT))
            //{
            //    ViewBag.YCCT = db.YeuCauPhucVus.Where(x => x.Status == 4 && x.Loai == 2 && (x.KhachHang.HoTen.Contains(searchStringCT) || x.KhachHang.SDT.Contains(searchStringCT))).OrderByDescending(x => x.NgayHoanThanh).ToPagedList(page, pagesize);
            //}
            //else
                ViewBag.YCCT = db.YeuCauPhucVus.Where(x => x.Status == 4 && x.Loai == 2).OrderByDescending(x => x.NgayHoanThanh);

            return View(model);
        }

        [HttpPost]
        public JsonResult timKH(int idkh)
        {
            List<KhachHang> list = new List<KhachHang>();
            var rs = db.KhachHangs.Find(idkh);
            return Json(rs.DiaChi, JsonRequestBehavior.AllowGet );
        }

        [HttpPost]
        public JsonResult Update(string ycpv, string ngaybatdau, string ngaylamtiep)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            YeuCauPhucVu ycpv_edit = serializer.Deserialize<YeuCauPhucVu>(ycpv);

            YeuCauPhucVu yeuCauPhucVu = db.YeuCauPhucVus.SingleOrDefault(x => x.ID == ycpv_edit.ID);
            yeuCauPhucVu.DiaChiPhucVu = ycpv_edit.DiaChiPhucVu;
            yeuCauPhucVu.YeuCau = ycpv_edit.YeuCau;
            if (yeuCauPhucVu.Loai == 1 || yeuCauPhucVu.Loai==3)
            {
                if (String.IsNullOrEmpty(ngaybatdau))
                {
                    return Json(new
                    {
                        status = false,
                        mess = "ngày bắt đầu không được để trống"
                    });
                }
                else
                {
                    try
                    {
                        yeuCauPhucVu.NgayBatDau = Convert.ToDateTime(ngaybatdau);
                    }
                    catch
                    {
                        return Json(new
                        {
                            status = false,
                            mess = "ngày bắt đầu sai định dạng"
                        });
                    }
                }
                if (String.IsNullOrEmpty(ngaylamtiep) || ngaylamtiep.ToLower().Equals("null"))
                    yeuCauPhucVu.NgayLamTiep = null;
                else
                {
                    try
                    {
                        yeuCauPhucVu.NgayLamTiep = Convert.ToDateTime(ngaylamtiep);
                    }
                    catch
                    {
                        return Json(new
                        {
                            status = false,
                            mess = "ngày làm tiếp sai định dạng"
                        });
                    }
                }
            }
            else if(yeuCauPhucVu.Loai == 2)
            {
                if (String.IsNullOrEmpty(ngaybatdau))
                {
                    return Json(new
                    {
                        status = false,
                        mess = "ngày bắt đầu không được để trống"
                    });
                }
                else
                {
                    try
                    {
                        yeuCauPhucVu.NgayBatDau = Convert.ToDateTime(ngaybatdau);
                    }
                    catch
                    {
                        return Json(new
                        {
                            status = false,
                            mess = "ngày bắt đầu sai định dạng"
                        });
                    }
                }
                if (String.IsNullOrEmpty(ngaylamtiep))
                {
                    return Json(new
                    {
                        status = false,
                        mess = "ngày dự kiến trả không được để trống"
                    });
                }
                else
                {
                    try
                    {
                        yeuCauPhucVu.NgayDuKienTra = Convert.ToDateTime(ngaylamtiep);
                    }
                    catch
                    {
                        return Json(new
                        {
                            status = false,
                            mess = "ngày dự kiến trả sai định dạng"
                        });
                    }
                }
            }

            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        [HttpPost]
        public JsonResult ThemKH(string ht, string sdt, string diachi)
        {
            KhachHang kh = new KhachHang();
            kh.HoTen = ht;
            kh.SDT = sdt;
            kh.DiaChi = diachi;
            db.KhachHangs.Add(kh);
            for (int i = 1; i <= 5; i++)
            {
                MayLanh ml = new MayLanh();
                ml.Ma = "ML" + i;
                ml.Status = 1;
                kh.MayLanhs.Add(ml);
            }
            db.SaveChanges();
            return Json(new {
                id=kh.ID,
                name=kh.HoTen,
                addr=kh.DiaChi,
                status =true
            });;
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
                var rs = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
                if (NewPass1.Length < 5)
                    ViewBag.Error = "Mật khẩu có độ dài từ 5 kí tự";
                else if (rs != null)
                {
                    LoginModel model = new LoginModel();
                    if (rs.Password.Equals(model.CreateMD5(model.Base64Encode(oldPass))))
                    {
                        if (NewPass1.Equals(NewPass2))
                        {
                            rs.Password = model.CreateMD5(model.Base64Encode(NewPass1));
                            db.SaveChanges();
                            //TempData["msg"] = "<script>alert('Thành công');</script>";
                            return RedirectToAction("Index", "Admin");
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
            return RedirectToAction("ChangePassWord", "Admin", new { ViewBag.Error });
        }
    }
}
