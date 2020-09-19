using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASP_MVC.EF;
using PagedList;
using System.Data.Entity;
using System.Data;
using ASP_MVC.Areas.admin.Models;
using System.Web.Script.Serialization;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0,2")]

    public class KhachHangController : Controller
    {
        // GET: admin/KhachHang
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public ActionResult Index()
        {
            IQueryable<KhachHang> model = db.KhachHangs;
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = searchString;
            //    model = model.Where(x => x.HoTen.Contains(searchString) || x.SDT.Contains(searchString) || x.DiaChi.Contains(searchString));
            //}
            return View(model.OrderByDescending(x => x.NgayPhucVu).ThenBy(x=>x.ID));
        }
        public ActionResult DSMay(long id)
        {
            return RedirectToAction("Index", "MayPhucVu", new { idkh = id });
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.KhachHangs.Add(khachHang);
                TestObject test = new TestObject();
                string exeption = test.TestKhachHangs(khachHang.ID, khachHang.HoTen, khachHang.SDT, khachHang.DiaChi);
                if(exeption.Equals("OK"))
                {
                    for(int i = 1; i <= 5; i++)
                    {
                        MayLanh ml = new MayLanh();
                        ml.Ma = "ML" + i;
                        ml.Status = 1;
                        khachHang.MayLanhs.Add(ml);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", exeption);
                }
            }
            return View(khachHang);
        }

        [HttpPost]
        public ActionResult Import(FormCollection formCollection)
        {
            var orderbyList = db.KhachHangs.ToList().OrderBy(x => x.ID);
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
                            var KhachHangDaTa = excelData.getData("KhachHang");
                            List<KhachHang> listkh = new List<KhachHang>();
                            dt = KhachHangDaTa.CopyToDataTable();
                            foreach (DataRow item in dt.Rows)
                            {
                                KhachHang khachHang = new KhachHang();
                                khachHang.HoTen = item["Họ tên"].ToString();
                                khachHang.SDT = item["Số điện thoại"].ToString();
                                khachHang.DiaChi = item["Địa chỉ"].ToString();
                                if (item["Loại"].ToString().ToLower().Equals("hợp đồng"))
                                    khachHang.Loai = true;
                                else
                                    khachHang.Loai = false;
                                listkh.Add(khachHang);
                            }
                            if (ModelState.IsValid)
                            {
                                if (listkh != null)
                                {
                                    for (int i = 0; i < listkh.Count; i++)
                                    {
                                        string sdt = listkh[i].SDT;
                                        KhachHang khtest = db.KhachHangs.SingleOrDefault(x => x.SDT == sdt);
                                        if (db.KhachHangs.SingleOrDefault(x => x.SDT == sdt) == null)
                                        {
                                            db.KhachHangs.Add(listkh[i]);
                                            db.SaveChanges();
                                        }
                                    }
                                    TempData["msg"] = "<script>alert('Thành công');</script>";
                                    return RedirectToAction("Index", orderbyList);
                                }
                            }
                        }
                        catch
                        {
                            ViewBag.Error = "Import thất bại";
                        }
                        

                        //var MayKhachData = excelData.getData("MayKhach");
                        //List<MayLanh> list = new List<MayLanh>();
                        //dt = MayKhachData.CopyToDataTable();
                        //foreach (DataRow item in dt.Rows)
                        //{
                        //    MayLanh mayPhucVu = new MayLanh();
                        //    string sdt = item["Số điện thoại"].ToString();
                        //    int idkh = db.KhachHangs.SingleOrDefault(x => x.SDT == sdt).ID;
                        //    mayPhucVu.IDKhachHang = idkh;
                        //    mayPhucVu.TenMay = item["Tên máy"].ToString();
                        //    mayPhucVu.Model = item["Nhãn hiệu"].ToString();
                        //    mayPhucVu.ViTri = item["Vị trí"].ToString();
                        //    mayPhucVu.CongSuat = item["Công suất"].ToString();
                        //    mayPhucVu.Status = 1;
                        //    list.Add(mayPhucVu);
                        //}
                        //TempData["mayPhucVu"] = list;
                        //if (ModelState.IsValid)
                        //{
                        //    if (list != null)
                        //    {
                        //        for (int i = 0; i < list.Count; i++)
                        //        {
                        //            if (db.MayLanhs.Find(list[i].ID) == null)
                        //            {
                        //                db.MayLanhs.Add(list[i]);
                        //                db.SaveChanges();
                        //            }
                        //        }
                        //        return RedirectToAction("Index", orderbyList.ToPagedList(1, 20));
                        //    }
                        //}
                    }
                    else
                    {
                        ViewBag.Error = "Vui lòng chọn file excel";
                        return View("Index", orderbyList);
                    }

                }
            }
            return View("Index", orderbyList);
        }

        [HttpPost]
        public JsonResult Update(string kh, string ngaypv)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            KhachHang khachHang_edit = serializer.Deserialize<KhachHang>(kh);

            TestObject test = new TestObject();
            string result = test.TestKhachHangs(khachHang_edit.ID, khachHang_edit.HoTen, khachHang_edit.SDT, khachHang_edit.DiaChi);
            if (result.Equals("OK"))
            {
                try
                {
                    KhachHang khachHang = db.KhachHangs.SingleOrDefault(x => x.ID == khachHang_edit.ID);
                    khachHang.SDT = khachHang_edit.SDT;
                    khachHang.HoTen = khachHang_edit.HoTen;
                    khachHang.DiaChi = khachHang_edit.DiaChi;
                    if (ngaypv != "null" && ngaypv != "")
                    {
                        khachHang.NgayPhucVu = Convert.ToDateTime(ngaypv);
                    }
                    else
                        khachHang.NgayPhucVu = null;
                    khachHang.Loai = khachHang_edit.Loai;
                    db.SaveChanges();
                    return Json(new
                    {
                        status = true
                    });
                }
                catch
                {
                    return Json(new
                    {
                        status = false,
                        mess = "Ngày phục vụ sai định dạng"
                    });
                }
            }
            else
                return Json(new
                {
                    status = false,
                    mess = result
                });
        }
    }
}