using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ASP_MVC.Areas.admin.Models;
using ASP_MVC.EF;
using PagedList;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0,1,2")]

    public class MayChoThueController : Controller
    {
        // GET: admin/MayLanh
        [NonAction]
        public void setViewBagQL()
        {
            ViewBag.QLNL = true;
        }
        // GET: admin/MayLanh
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public ActionResult Index()
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            IQueryable<MayLanh> model = db.MayLanhs.Where(x=>x.IDKhachHang==1 && (x.Status == 1 || x.Status == 2 || x.Status == 0));
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    model = model.Where(x => x.TenMay.Contains(searchString) || x.Model.Contains(searchString));
            //}
            return View(model.OrderBy(x => x.ID));
        }

        [NonAction]
        public bool checkMa(string ma)
        {
            if (db.MayLanhs.Where(x => x.Ma.ToUpper().Equals(ma.ToUpper()) && x.IDKhachHang == 1 && x.Status != 4).Count() > 0)
                return false;
            return true;
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MayLanh mayLanh)
        {
            if (ModelState.IsValid)
            {
                if (checkMa(mayLanh.Ma) == false)
                {
                    ModelState.AddModelError("Ma", "Mã đã tồn tại");
                }
                else
                {
                    mayLanh.IDKhachHang = 1;
                    TestObject test = new TestObject();
                    string ngaymua;
                    if (mayLanh.NgayMua.HasValue)
                        ngaymua = mayLanh.NgayMua.Value.ToString("dd/mm/yyyy");
                    else
                        ngaymua = null;
                    string exception = test.TestMayChoThues(mayLanh.TenMay);
                    if (exception == "OK")
                    {
                        mayLanh.Ma = mayLanh.Ma.ToUpper();
                        db.MayLanhs.Add(mayLanh);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", exception);
                    }
                }
        }
            return View(mayLanh);
        }
        
        [HttpPost]
        public JsonResult DropDownDV(string s)
        {
            //long idkh = long.Parse(Request.QueryString["kh"].ToString());
            List<DichVu_SanPham> list = new List<DichVu_SanPham>();
            var rs = db.DichVu_SanPham.Where(x => (x.MaDV_SP.Contains(s) || x.TenDichVu_SanPham.Contains(s)) && x.Status == true).ToList();
            foreach (var item in rs)
            {
                DichVu_SanPham kh = new DichVu_SanPham();
                kh.ID = item.ID;
                kh.TenDichVu_SanPham = item.TenDichVu_SanPham;
                list.Add(kh);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Import(FormCollection formCollection)
        {
            var orderbyList = db.MayLanhs.Where(x => x.IDKhachHang == 1).ToList().OrderBy(x => x.ID);
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
                            var sData = excelData.getData("MayChoThue");
                            List<MayLanh> list = new List<MayLanh>();
                            dt = sData.CopyToDataTable();
                            foreach (DataRow item in dt.Rows)
                            {
                                MayLanh dvsp = new MayLanh();
                                string ma = item["Mã máy"].ToString();
                                int count = db.MayLanhs.Where(x => x.KhachHang.ID == 1 && x.Ma.ToUpper().Equals(ma.ToUpper()) && x.Status == 1).Count();
                                if (count == 0)
                                {
                                    dvsp.Ma = ma;
                                    dvsp.TenMay = item["Tên máy"].ToString();
                                    dvsp.Model = item["Nhãn hiệu"].ToString();
                                    dvsp.CongSuat = item["Công suất"].ToString();
                                    if (String.IsNullOrEmpty(item["Ngày mua"].ToString()))
                                    {
                                        dvsp.NgayMua = null;
                                    }
                                    else
                                    {
                                        dvsp.NgayMua = Convert.ToDateTime(item["Ngày mua"].ToString());
                                    }
                                    dvsp.IDKhachHang = 1;
                                    if (item["Tình trạng"].ToString().ToUpper().Equals("ĐANG SỬA"))
                                        dvsp.Status = 0;
                                    else if (item["Tình trạng"].ToString().ToUpper().Equals("CÓ SẴN"))
                                        dvsp.Status = 1;
                                    else
                                        dvsp.Status = 2;
                                    list.Add(dvsp);
                                }
                                else
                                {
                                    TempData["msg"] = "<script>alert('Mã đã tồn tại, vui lòng kiểm tra dữ liệu');</script>";
                                    return View("Index", db.MayLanhs);
                                }
                            }
                            TempData["MayLanh"] = list;
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
                                    TempData["msg"] = "<script>alert('Thành công');</script>";
                                    return RedirectToAction("Index", "MayChoThue");
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
                        return RedirectToAction("Index", "MayChoThue");
                    }

                }
                else
                {
                    ViewBag.Error = "Vui lòng chọn file excel";
                    return RedirectToAction("Index", "MayChoThue");
                }
            }
            return RedirectToAction("Index", "MayChoThue");
        }

        [HttpPost]
        public JsonResult Update(string mct, string ngaymua)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            MayLanh MayLanh_edit = serializer.Deserialize<MayLanh>(mct);
            TestObject test = new TestObject();
            string result = test.TestMayChoThues(MayLanh_edit.TenMay);

            if (result.Equals("OK"))
            {
                MayLanh MayLanh = db.MayLanhs.SingleOrDefault(x => x.ID == MayLanh_edit.ID);
                MayLanh.TenMay = MayLanh_edit.TenMay;
                MayLanh.Model = MayLanh_edit.Model;
                MayLanh.CongSuat = MayLanh_edit.CongSuat;
                MayLanh.IDDichVu = MayLanh_edit.IDDichVu;
                MayLanh.GhiChu = MayLanh_edit.GhiChu;
                MayLanh.Status = MayLanh_edit.Status;
                try
                {
                    if (ngaymua != "null" && ngaymua != "")
                        MayLanh.NgayMua = Convert.ToDateTime(ngaymua);
                    else
                        MayLanh.NgayMua = null;
                }
                catch
                {
                    return Json(new
                    {
                        status = false,
                        mess = "Ngày mua sai định dạng"
                    });
                }
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
                    mess = "Tên máy và mã máy không được để trống"
                });

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
        public ActionResult LichSuSuaChua(int idmay, string ngaybd, string ngaykt)
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
                         // other assignments
                         //Id = ml.ID,
                         //Tenmay = ml.TenMay,
                         //Model = ml.Model,
                         //Congsuat = ml.CongSuat,
                         //Vitri = ml.ViTri,
                         //Iddichvu = ctbbnt.IDDichVu,
                         //Ngaythuchien = bbnt.NgayLap.ToString()
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
            return View(listlichSuMays.OrderBy(x => x.Ngaythuchien));
        }

        public ActionResult LichSuChoThue(int idmay, string ngaybd, string ngaykt)
        {
            ViewBag.IDmay = idmay;
            List<LichSuMay> listlichSuMays = new List<LichSuMay>();
            var rs = from ml in db.MayLanhs
                     join ctbbnt_ml in db.CTBBNT_MayLanh on ml.ID equals ctbbnt_ml.IDMay
                     join ctbbnt in db.CTBienBanNghiemThus on ctbbnt_ml.IDCTBBNT equals ctbbnt.ID
                     join bbnt in db.BienBanNghiemThus on ctbbnt.IDBBNT equals bbnt.ID
                     join dv in db.DichVu_SanPham on ctbbnt.IDDichVu equals dv.ID
                     where bbnt.Status == true && ctbbnt_ml.IDMay == idmay && bbnt.YeuCauPhucVu.Loai == 2
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
                         // other assignments
                         //Id = ml.ID,
                         //Tenmay = ml.TenMay,
                         //Model = ml.Model,
                         //Congsuat = ml.CongSuat,
                         //Vitri = ml.ViTri,
                         //Iddichvu = ctbbnt.IDDichVu,
                         //Ngaythuchien = bbnt.NgayLap.ToString()
                     };

            foreach (var item in rs)
            {
                LichSuMay lsm = new LichSuMay(item.id, item.idbbnt, item.tenmay, item.tenkhach, item.model, item.congsuat, item.vitri, item.iddichvu, item.tendichvu, Convert.ToDateTime(item.ngaythuchien));
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
            return View(listlichSuMays.OrderBy(x => x.Ngaythuchien));
        }

    }
}