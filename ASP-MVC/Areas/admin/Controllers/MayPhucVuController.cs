using ASP_MVC.Areas.admin.Models;
using ASP_MVC.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0,2")]

    public class MayPhucVuController : Controller
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        // GET: admin/MayBaoTri
        [NonAction]
        public void setViewBagQL()
        {
            ViewBag.QLNL = true;
        }
        public ActionResult Index(long idkh)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            IQueryable<MayLanh> model = db.MayLanhs.Where(x => x.IDKhachHang == idkh && x.Status == 1);
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    model = model.Where(x => x.TenMay.Contains(searchString) || x.ViTri.Contains(searchString) || x.Model.Contains(searchString));
            //}
            ViewBag.KH = db.KhachHangs.Find(idkh);
            var rs = model.Where(x => x.IDKhachHang == idkh);
            return View(rs.OrderBy(x => x.ID));
        }
        [HttpGet]
        public ActionResult Create(int idkh)
        {
            if (ViewBag.KH == null)
                ViewBag.KH = db.KhachHangs.Find(idkh);
            return View();
        }
        [NonAction]
        public bool checkMa(string ma, int idkh)
        {
            if (db.MayLanhs.Where(x => x.Ma.ToUpper().Equals(ma.ToUpper()) && x.IDKhachHang == idkh && x.Status != 4).Count() > 0)
                return false;
            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MayLanh mayPhucVu, int idkh)
        {
            
            if (ModelState.IsValid)
            {
                if (checkMa(mayPhucVu.Ma,idkh) == false)
                {
                    ModelState.AddModelError("Ma", "Mã đã tồn tại");
                }
                else
                {
                    mayPhucVu.IDKhachHang = idkh;
                    mayPhucVu.Ma = mayPhucVu.Ma.ToUpper();
                    mayPhucVu.Status = 1;
                    db.MayLanhs.Add(mayPhucVu);
                    TestObject test = new TestObject();
                    string exception = test.TestMayPhucVus(mayPhucVu.TenMay, mayPhucVu.ViTri);
                    if (exception == "OK")
                    {
                        db.SaveChanges();
                        return RedirectToAction("Index", "MayPhucVu", new { idkh = idkh });
                    }
                    else
                    {
                        ModelState.AddModelError("", exception);
                    }
                }
            }
            ViewBag.KH = db.KhachHangs.Find(idkh);
            return View(mayPhucVu);
        }

        

        [HttpPost]
        public JsonResult Update(string mayphucvu,int idkh)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            MayLanh mayPhucVu_edit = serializer.Deserialize<MayLanh>(mayphucvu);
            TestObject test = new TestObject();
            string result = test.TestMayPhucVus(mayPhucVu_edit.TenMay, mayPhucVu_edit.ViTri);
            if (result.Equals("OK"))
            {
                MayLanh mayPhucVu = db.MayLanhs.SingleOrDefault(x => x.ID == mayPhucVu_edit.ID);
                mayPhucVu.TenMay = mayPhucVu_edit.TenMay;
                mayPhucVu.Model = mayPhucVu_edit.Model;
                mayPhucVu.ViTri = mayPhucVu_edit.ViTri;
                mayPhucVu.CongSuat = mayPhucVu_edit.CongSuat;
                db.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }
            else
                return Json(new
                {
                    status = false,
                    mess = "Tên máy, vị trí không được để trống"
                });
        }
        public ActionResult LichSuSuaChua(int idkh, int idmay, string ngaybd, string ngaykt)
        {
            ViewBag.IDmay = idmay;
            //MayLanh may = db.MayLanhs.Find(idmay);
            List<LichSuMay> listlichSuMays = new List<LichSuMay>();
            var rs = from ml in db.MayLanhs
                     join ctbbnt_ml in db.CTBBNT_MayLanh on ml.ID equals ctbbnt_ml.IDMay
                     join ctbbnt in db.CTBienBanNghiemThus on ctbbnt_ml.IDCTBBNT equals ctbbnt.ID
                     join bbnt in db.BienBanNghiemThus on ctbbnt.IDBBNT equals bbnt.ID
                     join dv in db.DichVu_SanPham on ctbbnt.IDDichVu equals dv.ID
                     where bbnt.Status == true && ctbbnt_ml.IDMay == idmay && bbnt.YeuCauPhucVu.Loai == 1
                     select new
                     {
                         id = ml.ID,
                         idbbnt = ctbbnt.IDBBNT,
                         tenmay = ml.TenMay,
                         model = ml.Model,
                         congsuat = ml.CongSuat,
                         vitri = ml.ViTri,
                         iddichvu = ctbbnt.IDDichVu,
                         tendichvu = dv.TenDichVu_SanPham,
                         ngaythuchien = bbnt.NgayLap,
                         tenkhach = bbnt.YeuCauPhucVu.KhachHang.HoTen
                     };

            foreach (var item in rs)
            {
                LichSuMay lsm = new LichSuMay(item.id, item.idbbnt, item.tenmay,item.tenkhach, item.model, item.congsuat, item.vitri, item.iddichvu, item.tendichvu, Convert.ToDateTime(item.ngaythuchien));
                listlichSuMays.Add(lsm);
            }
            if (!String.IsNullOrEmpty(ngaybd) && !String.IsNullOrEmpty(ngaybd))
            {
                try
                {
                    DateTime bd = Convert.ToDateTime(ngaybd);
                    DateTime kt = Convert.ToDateTime(ngaykt);
                    ViewBag.Ngaybd = bd.ToString("dd/MM/yyyy");
                    ViewBag.Ngaykt = kt.ToString("dd/MM/yyyy");
                    listlichSuMays = listlichSuMays.Where(x => x.Ngaythuchien.Date >= bd.Date && x.Ngaythuchien.Date <= kt.Date).ToList();
                }
                catch
                {

                }
            }
            ViewBag.KH = db.KhachHangs.Find(idkh);
            return View(listlichSuMays.OrderBy(x => x.Ngaythuchien));
        }


        [HttpPost]
        public JsonResult Delete(long id)
        {
            var rs = db.MayLanhs.Find(id);
            rs.Status = 4;
            db.SaveChanges();
            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
        public ActionResult Import(FormCollection formCollection, int idkh)
        {
            var orderbyList = db.MayLanhs.ToList().OrderBy(x => x.ID);
            if (Request != null)
            {
                DataTable dt = new DataTable();
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    if (file.FileName.EndsWith("xls") || file.FileName.EndsWith("xlsx"))
                    {
                        string fileName = file.FileName;
                        string path = Server.MapPath("~/ExcelFiles/" + fileName);
                        file.SaveAs(path);
                        if (System.IO.File.Exists(Server.MapPath("~/ExcelFiles/" + fileName)))
                        {
                            System.IO.File.Delete(Server.MapPath("~/ExcelFiles/" + fileName));
                        }
                        file.SaveAs(path);
                        var excelData = new ExcelData(path);
                        try
                        {
                            var MayKhachData = excelData.getData("MayKhach");
                            List<MayLanh> list = new List<MayLanh>();
                            dt = MayKhachData.CopyToDataTable();
                            foreach (DataRow item in dt.Rows)
                            {
                                MayLanh mayPhucVu = new MayLanh();
                                string sdt = item["Số điện thoại"].ToString();
                                int idkhachhang = db.KhachHangs.SingleOrDefault(x => x.SDT == sdt).ID;
                                mayPhucVu.IDKhachHang = idkhachhang;
                                string ma = item["Mã máy"].ToString();
                                int count = db.MayLanhs.Where(x => x.KhachHang.ID == idkhachhang && x.Ma.ToUpper().Equals(ma.ToUpper()) && x.Status == 1).Count();
                                if (count == 0)
                                {
                                    mayPhucVu.Ma = ma;
                                    mayPhucVu.TenMay = item["Tên máy"].ToString();
                                    mayPhucVu.Model = item["Nhãn hiệu"].ToString();
                                    mayPhucVu.ViTri = item["Vị trí"].ToString();
                                    mayPhucVu.CongSuat = item["Công suất"].ToString();
                                    mayPhucVu.Status = 1;
                                    list.Add(mayPhucVu);
                                }
                                else
                                {
                                    TempData["msg"] = "<script>alert('Mã đã tồn tại, vui lòng kiểm tra dữ liệu');</script>";
                                    return View("Index", db.MayLanhs);
                                }
                            }
                            TempData["mayPhucVu"] = list;
                            if (ModelState.IsValid)
                            {
                                if (list != null)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        if (db.MayLanhs.Find(list[i].ID) == null)
                                        {
                                            db.MayLanhs.Add(list[i]);
                                            db.SaveChanges();
                                        }
                                    }
                                    var modelss = orderbyList;
                                    TempData["msg"] = "<script>alert('Thành công');</script>";
                                    return RedirectToAction("Index", "MayPhucVu", new { idkh, modelss });
                                }
                            }
                        }
                        catch
                        {
                            ViewBag.Error = "Import thất bại";
                        }

                    }
                    else
                    {
                        ViewBag.Error = "Vui lòng chọn file excel";
                        return View("Index", db.MayLanhs);
                    }

                }
                else
                {
                    ViewBag.Error = "Vui lòng chọn file excel";
                    return View("Index", orderbyList);
                }
            }
            //return View("Index", db.MayLanhs.ToList());
            var models = orderbyList;
            return RedirectToAction("Index", "MayPhucVu", new { idkh, models  });
        }
    }

}