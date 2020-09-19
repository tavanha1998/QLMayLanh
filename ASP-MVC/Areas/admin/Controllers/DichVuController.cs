using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ASP_MVC.Areas.admin.Models;
using ASP_MVC.EF;
using PagedList;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0")]

    public class DichVuController : Controller
    {
        // GET: admin/DichVu

        private QLMayLanhEntities db = new QLMayLanhEntities();
        public ActionResult Index(string status)
        {
            IQueryable<DichVu_SanPham> model = db.DichVu_SanPham.Where(x=>x.Status==true);
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    model = model.Where(x => (x.MaDV_SP.Contains(searchString) || x.TenDichVu_SanPham.Contains(searchString)) && x.Status == true);
            //}
            if (!string.IsNullOrEmpty(status) && status.Contains("Thành công"))
                ViewBag.Error = "Thành công";
            model = model.OrderByDescending(x => x.NgayApDung);
            return View(model);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DichVu_SanPham dvsp)
        {
            if (ModelState.IsValid)
            {
                dvsp.Status = true;
                TestObject test = new TestObject();
                string exception = test.TestDichVu(dvsp.MaDV_SP, dvsp.TenDichVu_SanPham,dvsp.NgayApDung.Value);
                
                if(exception.Equals("OK"))
                {
                    if (!dvsp.DonGia.HasValue || !dvsp.Diem.HasValue)
                    {
                        ModelState.AddModelError("", "Bạn chưa điền đơn giá hoặc điểm");
                    }
                    else
                    {
                        db.DichVu_SanPham.Add(dvsp);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ModelState.AddModelError("", exception);
                }
            }
            return View(dvsp);
        }

        [HttpPost]
        public JsonResult Update(string dvsp,string diem)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            DichVu_SanPham dvsp_edit = serializer.Deserialize<DichVu_SanPham>(dvsp);

            if(!dvsp_edit.TenDichVu_SanPham.Equals(""))
            {
                DichVu_SanPham dv = db.DichVu_SanPham.SingleOrDefault(x => x.ID == dvsp_edit.ID);
                dv.TenDichVu_SanPham = dvsp_edit.TenDichVu_SanPham;
                dv.Loai = dvsp_edit.Loai;
                dv.DonGia = dvsp_edit.DonGia;
                dv.Diem = Double.Parse(diem.ToString(), CultureInfo.InvariantCulture);
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
        [HttpGet]
        public ActionResult ExcelHandle()
        {
            return View();
        }
        public ActionResult Import(FormCollection formCollection)
        {
            var orderbyList = db.DichVu_SanPham.ToList().OrderBy(x => x.ID);
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
                        var sData = excelData.getData("DichVu_SanPham");
                        List<DichVu_SanPham> list = new List<DichVu_SanPham>();
                        dt = sData.CopyToDataTable();
                        foreach (DataRow item in dt.Rows)
                        {
                            DichVu_SanPham dvsp = new DichVu_SanPham();
                            dvsp.MaDV_SP = item["SKU"].ToString();
                            dvsp.TenDichVu_SanPham = item["Product/Service Name"].ToString();
                            if (item["Type"].ToString().ToUpper().Equals("SERVICE"))
                                dvsp.Loai = true;
                            else
                                dvsp.Loai = false;
                            if (item["Sales Price"].ToString() == null || item["Sales Price"].ToString() == "")
                                dvsp.DonGia = 0;
                            else
                                dvsp.DonGia = float.Parse(item["Sales Price"].ToString());
                            dvsp.NgayApDung = DateTime.Now.Date;
                            if (item["Scores"].ToString() == null || item["Scores"].ToString() == "")
                                dvsp.Diem = 0;
                            else
                                dvsp.Diem = float.Parse(item["Scores"].ToString());
                            dvsp.Status = true;
                            if(db.DichVu_SanPham.FirstOrDefault(x=>x.MaDV_SP.Contains(dvsp.MaDV_SP)) == null)
                            {
                                db.DichVu_SanPham.Add(dvsp);
                                db.SaveChanges();
                            }
                            else
                            {
                                var rs = db.DichVu_SanPham.SingleOrDefault(x => x.MaDV_SP.Contains(dvsp.MaDV_SP) && x.Status == true);
                                rs.Status = false;
                                db.DichVu_SanPham.Add(dvsp);
                                db.SaveChanges();
                            }
                            //list.Add(dvsp);
                            
                        }
                        string status = "Thành công";
                        TempData["msg"] = "<script>alert('Thành công');</script>";
                        return RedirectToAction("Index", "DichVu", new { status });
                        //TempData["DichVu_SanPham"] = list;
                        //if (ModelState.IsValid)
                        //{
                        //    if (list != null)
                        //    {
                        //        for (int i = 0; i < list.Count; i++)
                        //        {
                        //            List<DichVu_SanPham> dichVu_SanPham = db.DichVu_SanPham.Where(x => x.MaDV_SP.Contains(list[i].MaDV_SP.ToString())).ToList();
                        //            if (dichVu_SanPham == null)
                        //            {
                        //                db.DichVu_SanPham.Add(list[i]);
                        //                db.SaveChanges();
                        //            }
                        //            else
                        //            {

                        //            }
                        //        }
                        //        return RedirectToAction("Index", "DichVu");
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
            ViewBag.Error = "Vui lòng chọn file excel";
            return View("Index", orderbyList);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            var rs = db.DichVu_SanPham.Find(id);
            rs.Status = false;
            db.SaveChanges();
            return Json(new
            {
                status = true,
            });
        }
    }
}