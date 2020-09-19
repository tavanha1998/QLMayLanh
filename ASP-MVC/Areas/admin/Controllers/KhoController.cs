using ASP_MVC.Areas.admin.Models;
using ASP_MVC.EF;
using OfficeOpenXml;
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
    [Authorize(Roles = "0,1")] 
    public class KhoController : Controller
    {
        [NonAction]
        public void setViewBagQL()
        {
            ViewBag.QLNL = true;
        }
        // dụng cụ: 0: hủy, 1: có sẵn, 2: đang mượn
        // GET: admin/Kho
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public ActionResult Index()
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            IQueryable<KhoVatDung> model = db.KhoVatDungs.Where(x=>x.Status != 0);
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    model = model.Where(x => x.TenVatDung.Contains(searchString));
            //}
            return View(model.OrderByDescending(x => x.Status));
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhoVatDung khoVatDung)
        {
            if (ModelState.IsValid)
            {
                khoVatDung.Status = 1;
                db.KhoVatDungs.Add(khoVatDung);

                string ngaymua;
                if (khoVatDung.NgayMua.HasValue)
                    ngaymua = khoVatDung.NgayMua.Value.ToString("dd/mm/yyyy");
                else
                    ngaymua = null;
                TestObject test = new TestObject();
                string exception = test.TestKho(khoVatDung.TenVatDung, ngaymua);
                if (exception == "OK")
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", exception);
                }
            }
            return View(khoVatDung);
        }
        public ActionResult Import(FormCollection formCollection)
        {
            var orderbyList = db.KhoVatDungs.ToList().OrderBy(x => x.ID);
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
                            var sData = excelData.getData("Kho");
                            List<KhoVatDung> list = new List<KhoVatDung>();
                            dt = sData.CopyToDataTable();
                            foreach (DataRow item in dt.Rows)
                            {
                                KhoVatDung dvsp = new KhoVatDung();
                                dvsp.TenVatDung = item["Tên vật dụng"].ToString();
                                if (String.IsNullOrEmpty(item["Ngày mua"].ToString()))
                                {
                                    dvsp.NgayMua = null;
                                }
                                else
                                {
                                    dvsp.NgayMua = Convert.ToDateTime(item["Ngày mua"].ToString());
                                }
                                dvsp.GhiChu = item["Ghi chú"].ToString();
                                dvsp.Status = 1;
                                list.Add(dvsp);
                            }
                            if (ModelState.IsValid)
                            {
                                if (list != null)
                                {
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        string tenvatdung = list[i].TenVatDung;
                                        KhoVatDung kho = db.KhoVatDungs.SingleOrDefault(x => x.TenVatDung == tenvatdung);
                                        if (db.KhoVatDungs.SingleOrDefault(x => x.TenVatDung == tenvatdung) == null)
                                        {
                                            db.KhoVatDungs.Add(list[i]);
                                            db.SaveChanges();
                                        }
                                    }
                                    TempData["msg"] = "<script>alert('Thành công');</script>";
                                    return RedirectToAction("Index", "Kho");
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
                        return View("Index", orderbyList);
                    }

                }
            }
            ViewBag.Error = "Vui lòng chọn file excel";
            return RedirectToAction("Index", "Kho");
        }

        [HttpPost]
        public JsonResult Update(string kho, string ngaymua)
        {
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                KhoVatDung kho_edit = serializer.Deserialize<KhoVatDung>(kho);

                if(!kho_edit.TenVatDung.Equals(""))
                {
                    try
                    {
                        KhoVatDung khoVatDung = db.KhoVatDungs.SingleOrDefault(x => x.ID == kho_edit.ID);
                        khoVatDung.TenVatDung = kho_edit.TenVatDung;
                        if (ngaymua == null || ngaymua == "null" || ngaymua == "")
                            khoVatDung.NgayMua = null;
                        else
                            khoVatDung.NgayMua = Convert.ToDateTime(ngaymua);
                        khoVatDung.GhiChu = kho_edit.GhiChu;
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
                            mess = "Ngày mua sai định dạng"
                        });
                    }
                    
                }
                else
                    return Json(new
                    {
                        status = false
                    });
            }
            catch
            {
                return Json(new
                {
                    status = false
                });
            }
        }

        [HttpPost]
        public JsonResult UpdatePhieuXuat(string px)
        {
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                PhieuXuatKho px_edit = serializer.Deserialize<PhieuXuatKho>(px);

                if (px_edit.IDKTV != 0)
                {
                    PhieuXuatKho phieuXuatKho = db.PhieuXuatKhoes.SingleOrDefault(x => x.ID == px_edit.ID);
                    phieuXuatKho.IDKTV = px_edit.IDKTV;
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
            catch
            {
                return Json(new
                {
                    status = false
                });
            }
        }
        [HttpGet]
        public ActionResult PhieuXuatKho()
        {
            IQueryable<PhieuXuatKho> model = db.PhieuXuatKhoes;
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    try
            //    {
            //        DateTime dt = Convert.ToDateTime(searchString);
            //        model = model.Where(x => DbFunctions.TruncateTime(x.NgayXuat) == dt);
            //    }
            //    catch
            //    {
            //        int a;
            //        if (Int32.TryParse(searchString, out a))
            //            model = model.Where(x => x.ID == a);
            //    }
            //}
            return View(model.OrderBy(x => x.Status));
        }
        [HttpGet]
        public ActionResult CreatePhieuXuatKho()
        {
            IQueryable<KhoVatDung> model = db.KhoVatDungs;
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    model = model.Where(x => x.TenVatDung.Contains(searchString));
            //}
            List<NhanVien> list = db.NhanViens.Where(x => x.Status == 1).ToList();
            ViewBag.KTV = new SelectList(list, "ID", "TenKTV");
            ViewBag.DSDungcu = model.Where(x => x.Status == 1).OrderBy(x => x.ID);
            return View();
        }
        

        [HttpPost]
        public JsonResult ThemPhieuXuat(int idktv)
        {
            //if(db.PhieuXuatKhoes.SingleOrDefault(x => (x.NgayXuat == dtnow && x.IDKTV == idktv)) == null)
            //{
                PhieuXuatKho phieuXuatKho = new PhieuXuatKho();
                phieuXuatKho.IDKTV = idktv;

                phieuXuatKho.NgayXuat = DateTime.Now;
                phieuXuatKho.KiemDuyet = false;
                phieuXuatKho.Status = 0;
                db.PhieuXuatKhoes.Add(phieuXuatKho);
                db.SaveChanges();

                return Json(new
                {
                    status = true,
                    idpx = phieuXuatKho.ID
                });
            //}
            //else
            //{
            //    return Json(new
            //    {
            //        status = false,
            //    });
            //}
            
        }

        [HttpPost]
        public JsonResult LuuCTPhieuXuat(int idvatdung, int idktv, int idpx)
        {
                //Lưu chi tiết
                CTPhieuXuatKho ct = new CTPhieuXuatKho();
                ct.IDPhieuXuat = idpx;
                ct.IDVatDung = idvatdung;
                ct.Status = 0;

                //Thay đổi Status vật dụng
                KhoVatDung vd = db.KhoVatDungs.SingleOrDefault(x => x.ID == idvatdung);
                vd.Status = 2;

                db.CTPhieuXuatKhoes.Add(ct);
                db.SaveChanges();
                return Json(new
                {
                    status = true,

                });
           
        }

        [HttpGet]
        public ActionResult CTPhieuXuatKho(int idphieuxuat)
        {
            List<CTPhieuXuatKho> list_ctphieuxuat = db.CTPhieuXuatKhoes.Where(x => x.IDPhieuXuat == idphieuxuat).ToList();
            ViewBag.idpx = idphieuxuat;
            int idktv = db.PhieuXuatKhoes.SingleOrDefault(x => x.ID == idphieuxuat).IDKTV;
            ViewBag.TennKTV = db.NhanViens.SingleOrDefault(x => x.ID == idktv).TenKTV;
            ViewBag.StatusPhieuXuat = db.PhieuXuatKhoes.SingleOrDefault(x => x.ID == idphieuxuat).Status;
            ViewBag.NgayXuat = db.PhieuXuatKhoes.SingleOrDefault(x => x.ID == idphieuxuat).NgayXuat.Value.ToString("dd/MM/yyyy");
            IQueryable<KhoVatDung> model = db.KhoVatDungs;
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    ViewBag.search = Request.QueryString["searchString"].ToString();
            //    model = model.Where(x => x.TenVatDung.Contains(searchString));
            //}
            ViewBag.DSDungcu = db.KhoVatDungs.Where(x => x.Status == 1).OrderBy(x => x.ID);
            return View(list_ctphieuxuat);
        }

        [HttpPost]
        public JsonResult btnDangMuon(int idvatdung)
        {
            int idpx = 0;
            List<CTPhieuXuatKho> listPX = db.CTPhieuXuatKhoes.Where(x => x.IDVatDung == idvatdung).ToList();
            foreach(CTPhieuXuatKho ctpx in listPX)
            {
                if(ctpx.PhieuXuatKho.Status == 0)
                {
                    idpx = ctpx.IDPhieuXuat;
                    break;
                }
            }
            return Json(new
            {
                status = true,
                idphieuxuat = idpx
            });
        }

        [HttpPost]
        public JsonResult ChangeStatusPhieu(int idphieuxuat)
        {
            PhieuXuatKho px = db.PhieuXuatKhoes.SingleOrDefault(x => x.ID == idphieuxuat);
            if (px.Status == 0)
            {
                px.Status = 1;
                px.NgayTra = DateTime.Now;

                List<CTPhieuXuatKho> listct = db.CTPhieuXuatKhoes.Where(x => x.IDPhieuXuat == idphieuxuat).ToList();
                foreach(CTPhieuXuatKho item in listct)
                {
                    KhoVatDung vatDung = db.KhoVatDungs.SingleOrDefault(x => x.ID == item.IDVatDung);
                    vatDung.Status = 1;
                    db.SaveChanges();
                }
            }
            else
            {
                px.Status = 0;
                px.NgayTra = null;

                List<CTPhieuXuatKho> listct = db.CTPhieuXuatKhoes.Where(x => x.IDPhieuXuat == idphieuxuat).ToList();
                foreach (CTPhieuXuatKho item in listct)
                {
                    KhoVatDung vatDung = db.KhoVatDungs.SingleOrDefault(x => x.ID == item.IDVatDung);
                    vatDung.Status = 2;
                    db.SaveChanges();
                }
            } 
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        [HttpPost]
        public JsonResult XoaChitietPhieuXuat(int idctpx)
        {
            CTPhieuXuatKho ctpx = db.CTPhieuXuatKhoes.SingleOrDefault(x => x.ID == idctpx);
            db.CTPhieuXuatKhoes.Remove(ctpx);

            KhoVatDung vatdung = db.KhoVatDungs.SingleOrDefault(x => x.ID == ctpx.IDVatDung);
            vatdung.Status = 1;

            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        [HttpPost]
        public JsonResult Delete(long id)
        {
            var rs = db.KhoVatDungs.Find(id);
            rs.Status = 0;
            db.SaveChanges();
            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
        public JsonResult DSNhanVien(string s)
        {
            List<NhanVien> list = new List<NhanVien>();
            var rs = db.NhanViens.Where(x => x.TenKTV.Contains(s) || x.SDT.Contains(s)).ToList();
            foreach (var item in rs)
            {
                NhanVien kh = new NhanVien();
                kh.ID = item.ID;
                kh.TenKTV = item.TenKTV;
                kh.SDT = item.SDT;
                list.Add(kh);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public void DownloadExcel(int id)
        {
            var px = db.PhieuXuatKhoes.Find(id);
            var query = from T0 in db.PhieuXuatKhoes
                        join T1 in db.CTPhieuXuatKhoes on T0.ID equals T1.IDPhieuXuat
                        join T2 in db.KhoVatDungs on T1.IDVatDung equals T2.ID
                        join T3 in db.NhanViens on T0.IDKTV equals T3.ID
                        where T0.ID == id
                        select new
                        {
                            T0.ID
                            ,T0.NgayXuat
                            ,T0.NgayTra
                            ,T3.TenKTV
                            ,T2.TenVatDung
                            ,T2.GhiChu
                        };
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            #region -- Header --

            Sheet.Cells["A1"].Style.Font.Bold = true;
            Sheet.Cells["A1"].Value = "BẢO AN GIA";
            Sheet.Cells["C1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            Sheet.Cells["C1"].Value = DateTime.Now.ToString("HH:mm, dd/MM/yyyy");
            Sheet.Cells["A3:C3"].Merge = true;
            Sheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A3"].Style.Font.Bold = true;
            Sheet.Cells["A3"].Value = "PHIẾU XUẤT VẬT DỤNG";
            Sheet.Cells["B5"].Value = "Mã phiếu xuất:";
            Sheet.Cells["B5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["A7"].Value = "Tên KTV:";
            Sheet.Cells["A8"].Value = "Ngày mượn:";
            Sheet.Cells["A9"].Value = "Ngày trả:";

            Sheet.Cells["B5:C5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
            Sheet.Cells["B5:C5"].Style.Font.Bold = true;
            Sheet.Cells["B7:C7"].Merge = true;
            Sheet.Cells["B8:C8"].Merge = true;
            Sheet.Cells["B9:C9"].Merge = true;
            Sheet.Cells["B11:C11"].Merge = true;


            Sheet.Cells["A11"].Value = "STT";
            Sheet.Cells["B11"].Value = "Tên vật dụng";
            Sheet.Cells["A11:B11"].Style.Font.Bold = true;
            Sheet.Cells["A11:B11"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            #endregion

            #region -- Details --
            int row = 12;
            int stt = 1;
            foreach (var item in query)
            {
                Sheet.Cells["C5"].Value = item.ID;
                Sheet.Cells["B7"].Value = item.TenKTV;
                Sheet.Cells["B8"].Value = item.NgayXuat.Value.ToString("HH:mm, dd/MM/yyyy");
                if(item.NgayTra != null)
                    Sheet.Cells["B9"].Value = item.NgayTra.Value.ToString("HH:mm, dd/MM/yyyy");

                Sheet.Cells[string.Format("A{0}", row)].Value = stt;
                Sheet.Cells[string.Format("B{0}", row) + ":" +string.Format("C{0}", row)].Merge = true;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.TenVatDung;
                row++;
                stt++;
            }
            #endregion

            #region -- Footer --

            Sheet.Cells["A11:" + string.Format("A{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("A{0}", row)].Value = "Ghi chú";
            Sheet.Cells[string.Format("A{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row) + ":" + string.Format("C{0}", row)].Merge = true;
            Sheet.Cells[string.Format("B{0}", row)].Value = query.FirstOrDefault().GhiChu;
            Sheet.Cells[string.Format("A{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            //Sheet.Cells[string.Format("B{0}", row) + ":" + string.Format("B{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("A{0}", row + 2)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 2)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("A{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("A{0}", row + 2)].Value = "Kỹ thuật viên";
            Sheet.Cells[string.Format("A{0}", row + 3)].Value = "(Ký ghi rõ họ tên)";
            Sheet.Cells[string.Format("C{0}", row + 2)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("C{0}", row + 2)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("C{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("C{0}", row + 2)].Value = "Người lập phiếu";
            Sheet.Cells[string.Format("C{0}", row + 3)].Value = "(Ký ghi rõ họ tên)";

            Sheet.Cells["A11" + ":" + string.Format("C{0}", row)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A11" + ":" + string.Format("C{0}", row)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A11" + ":" + string.Format("C{0}", row)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A11" + ":" + string.Format("C{0}", row)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            #endregion


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.AddHeader("Refresh", "1; url=/admin/VatTu/PhieuNhap");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.Flush();
            Response.End();
            px.KiemDuyet = true;
            db.SaveChanges();
        }

    }
}