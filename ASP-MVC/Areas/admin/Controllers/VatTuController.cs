using ASP_MVC.Areas.admin.Models;
using ASP_MVC.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using OfficeOpenXml;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0,1")]

    public class VatTuController : Controller
    {
        // GET: admin/VatTu
        private QLMayLanhEntities db = new QLMayLanhEntities();
        #region -- Index --
        [NonAction]
        public void setViewBagQL()
        {
            ViewBag.QLNL = true;
        }
        public ActionResult Index(string searchString, int page = 1, int pagesize = 20)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            IQueryable<VatTu> model = db.VatTus.Where(x => x.Status != 0); // 0 : Hủy
            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.search = Request.QueryString["searchString"].ToString();
                model = model.Where(x => x.TenVatTu.Contains(searchString));
            }
            return View(model.OrderBy(x => x.ID).ToPagedList(page, pagesize));
        }
        #endregion

        #region -- Create --
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VatTu vt)
        {
            if (ModelState.IsValid)
            {
                var rs = db.VatTus.Where(x => x.TenVatTu.Equals(vt.TenVatTu)).ToList();
                if (rs.Count() == 0)
                {
                    if (vt.SoLuong == null)
                        vt.SoLuong = 0;
                    vt.Status = 1;
                    db.VatTus.Add(vt);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                    ModelState.AddModelError("TenVatTu", "Tên vật tư đã tồn tại, vui lòng kiểm tra lại");
            }
            return View(vt);
        }
        [HttpPost]
        public JsonResult ThemVT(string vattu)
        {
            var rs = db.VatTus.Where(x => x.TenVatTu.Equals(vattu)).ToList();
            if (rs.Count() == 0)
            {
                VatTu vt = new VatTu();
                vt.Status = 1;
                vt.TenVatTu = vattu;
                vt.SoLuong = 0;
                db.VatTus.Add(vt);
                db.SaveChanges();
                return Json(new
                {
                    id = vt.ID,
                    tenvt = vt.TenVatTu,
                    status = true
                });
            }
            else
                return Json(new
                {
                    status = false
                });
        }
        #endregion

        [HttpPost]
        public JsonResult Delete(long id)
        {
            var rs = db.VatTus.Find(id);
            rs.Status = 0;
            db.SaveChanges();
            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
        public JsonResult Update(int id, int giaban, string donvi, int sl)
        {
            try
            {
                VatTu vt = db.VatTus.Find(id);
                vt.GiaBan = giaban;
                vt.DonVi = donvi;
                vt.SoLuong = sl;
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
                    status = false
                });
            }

            }
        public ActionResult Import(FormCollection formCollection)
        {
            var orderbyList = db.VatTus.ToList().OrderBy(x => x.ID);
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
                            var sData = excelData.getData("VatTu");
                            List<VatTu> list = new List<VatTu>();
                            dt = sData.CopyToDataTable();
                            foreach (DataRow item in dt.Rows)
                            {
                                VatTu dvsp = new VatTu();
                                dvsp.TenVatTu = item["Tên vật tư"].ToString();
                                if (String.IsNullOrEmpty(item["Số lượng"].ToString()))
                                {
                                    dvsp.SoLuong = 0;
                                }
                                else
                                    dvsp.SoLuong = float.Parse(item["Số lượng"].ToString());
                                if (String.IsNullOrEmpty(item["Đơn vị"].ToString()))
                                {
                                    dvsp.DonVi = null;
                                }
                                else
                                    dvsp.DonVi = item["Đơn vị"].ToString();
                                if (String.IsNullOrEmpty(item["Đơn giá"].ToString()))
                                {
                                    dvsp.GiaBan = 0;
                                }
                                else
                                    dvsp.GiaBan = int.Parse(item["Đơn giá"].ToString());
                                if (String.IsNullOrEmpty(item["Ghi chú"].ToString()))
                                {
                                    dvsp.GhiChu = null;
                                }
                                else
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
                                        string tenvatdung = list[i].TenVatTu;
                                        VatTu vt = db.VatTus.SingleOrDefault(x => x.TenVatTu == tenvatdung);
                                        if (db.VatTus.SingleOrDefault(x => x.TenVatTu == tenvatdung) == null)
                                        {
                                            db.VatTus.Add(list[i]);
                                            db.SaveChanges();
                                        }
                                    }
                                    TempData["msg"] = "<script>alert('Thành công');</script>";
                                    return RedirectToAction("Index", "VatTu");
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
                        return View("Index", orderbyList.ToPagedList(1, 20));
                    }

                }
            }
            return RedirectToAction("Index", "VatTu");
        }

        public ActionResult PhieuNhap(string searchString, int page = 1, int pagesize = 20)
        {
            IQueryable<PhieuNhapVatTu> model = db.PhieuNhapVatTus;
            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.search = Request.QueryString["searchString"].ToString();
                try
                {
                    DateTime dt = Convert.ToDateTime(searchString);
                    model = model.Where(x => DbFunctions.TruncateTime(x.NgayNhap) == dt);
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

        public ActionResult CTPhieuNhap(int idphieunhap)
        {
            List<CTPhieuNhapVatTu> list_ctphieunhap = db.CTPhieuNhapVatTus.Where(x => x.IDPhieuNhap == idphieunhap).ToList();
            ViewBag.idpn = idphieunhap;
            ViewBag.NgayNhap = db.PhieuNhapVatTus.SingleOrDefault(x => x.ID == idphieunhap).NgayNhap.Value.ToString("dd/MM/yyyy");
            
            return View(list_ctphieunhap);
        }

        public ActionResult CreatePhieuNhap()
        {
            List<VatTu> listvattu = db.VatTus.Where(x => x.Status == 1).ToList();
            SelectList selectlistvattu = new SelectList(listvattu, "ID", "TenVatTu");
            ViewBag.selectlist = selectlistvattu;
            return View();
        }

        [HttpPost]
        public JsonResult ThemCTPN(int idvattu)
        {
            if (idvattu != 0)
            {
                try
                {
                    var checkvattu = db.VatTus.SingleOrDefault(x => x.ID == idvattu);
                    if (checkvattu == null)
                        return Json(new { status = false });
                    else
                    {
                        VatTu vt = new VatTu();
                        vt.ID = idvattu;
                        vt.TenVatTu = checkvattu.TenVatTu;
                        vt.DonVi = checkvattu.DonVi;
                        //vt.SoLuong = checkvattu.SoLuong;
                        //vt.GiaBan = checkvattu.GiaBan;
                        //vt.GhiChu = checkvattu.GhiChu;
                        //vt.Status = checkvattu.Status;
                        return Json(new
                        {
                            status = true,
                            vattu = vt
                        });
                    }
                        
                }
                catch
                {
                    return Json(new
                    {
                        status = false
                    });
                }

            }
            else
                return Json(new
                {
                    status = false
                });
        }

        [HttpPost]
        public JsonResult TaoPhieuNhap(string ghichu)
        {
            PhieuNhapVatTu phieuNhapVatTu = new PhieuNhapVatTu();
            phieuNhapVatTu.NgayNhap = DateTime.Now;
            phieuNhapVatTu.GhiChu = ghichu;
            db.PhieuNhapVatTus.Add(phieuNhapVatTu);
            db.SaveChanges();

            return Json(new
            {
                status = true,
                idpn = phieuNhapVatTu.ID
            });
        }

        [HttpPost]
        public JsonResult TaoCTPhieuNhap(int idpn, int idvattu, float soluong, int dongia, string ncc, string ghichu)
        {
            CTPhieuNhapVatTu ct = new CTPhieuNhapVatTu();
            ct.IDPhieuNhap = idpn;
            ct.IDVatTu = idvattu;
            ct.SoLuong = soluong;
            ct.DonGia = dongia;
            ct.NCC = ncc;
            ct.GhiChu = ghichu;
            ct.Status = 1;
            VatTu vt = db.VatTus.SingleOrDefault(x => x.ID == idvattu);
            if (vt.SoLuong != null && vt.GiaBan != null)
            {
                vt.GiaBan = Convert.ToInt32((vt.SoLuong.Value * vt.GiaBan.Value + soluong * dongia) / (vt.SoLuong + soluong));
                vt.GiaNhap = vt.GiaBan;
                vt.SoLuong = vt.SoLuong + soluong;
            }
            else
            {
                vt.GiaNhap = dongia;
                vt.GiaBan = dongia;
                vt.SoLuong = vt.SoLuong + soluong;
            }
            db.CTPhieuNhapVatTus.Add(ct);
            db.SaveChanges();
            return Json(new
            {
                status = true,

            });
        }

        [HttpPost]
        public JsonResult UpdatePhieuNhap(int id, string ghichu)
        {
            try
            {
                PhieuNhapVatTu vt = db.PhieuNhapVatTus.SingleOrDefault(x => x.ID == id);
                vt.GhiChu = ghichu;
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
                    status = false
                });
            }

            
        }

        #region -- PhieuXuat_KTV

        [HttpPost]
        public JsonResult updateCT(int id, string SLTra)
        {
            CTPhieuXuatVatTu_KTV ct = db.CTPhieuXuatVatTu_KTV.Find(id);
            VatTu vt = db.VatTus.Find(ct.IDVatTu);
            ct.SLTra = Double.Parse(SLTra.ToString(), CultureInfo.InvariantCulture);
            ct.SLThucTe = ct.SLLay - ct.SLTra;
            vt.SoLuong = vt.SoLuong + ct.SLTra;
            if (ct.SLThucTe > 0)
            {
                db.SaveChanges();
                return Json(new
                {
                    status = true,
                    mess = "Thành công"
                });
            }
            else
                return Json(new
                {
                    status = false,
                    mess = "Số lượng trả phải nhỏ hơn số lượng lấy đi"
                });
        }
        public ActionResult PhieuXuat_KTV(string searchString, int page = 1, int pagesize = 20)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            IQueryable<PhieuXuatVatTu_KTV> model = db.PhieuXuatVatTu_KTV.Where(x=>x.Status==1);
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
            return View(model.OrderByDescending(x => x.NgayXuat).ToPagedList(page, pagesize));
        }

        public ActionResult CTPhieuXuat_KTV(int idphieuxuat)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            List<CTPhieuXuatVatTu_KTV> list_ctphieuxuat = db.CTPhieuXuatVatTu_KTV.Where(x => x.IDPX_KTV == idphieuxuat).ToList();
            ViewBag.idphieu = idphieuxuat;
            ViewBag.NgayXuat = db.PhieuXuatVatTu_KTV.SingleOrDefault(x => x.ID == idphieuxuat).NgayXuat.Value.ToString("dd/MM/yyyy");
            ViewBag.KTV = db.PhieuXuatVatTu_KTV.SingleOrDefault(x => x.ID == idphieuxuat).NhanVien.TenKTV.ToString();
            return View(list_ctphieuxuat);
        }

        public ActionResult CreatePhieuXuat_KTV()
        {
            List<NhanVien> listktv = db.NhanViens.ToList();
            SelectList selectlistktv = new SelectList(listktv, "ID", "TenKTV");
            ViewBag.selectlistktv = selectlistktv;

            var listycpv = db.YeuCauPhucVus.Where(x => x.Status != 3 && x.Status != 4).ToList().Select(d=> new {
                ID = d.ID,
                Text= d.KhachHang.HoTen + " - " + d.YeuCau + " - " +d.NgayLap.Value.ToString("dd/MM/yyyy")
            });
            SelectList selectlistycpv = new SelectList(listycpv, "ID", "Text");
            ViewBag.selectlistycpv = selectlistycpv;

            List<VatTu> list = db.VatTus.Where(x => x.Status == 1 && x.SoLuong > 0).ToList();
            SelectList selectlist = new SelectList(list, "ID", "TenVatTu");
            ViewBag.selectlist = selectlist;
            return View();
        }

        [HttpPost]
        public JsonResult ThemCTPX_KTV(int idvattu, int soluong)
        {
            if (idvattu != 0)
            {
                try
                {
                    var checkvattu = db.VatTus.SingleOrDefault(x => x.ID == idvattu);
                    if (checkvattu == null || checkvattu.SoLuong < soluong)
                        return Json(new { status = false, mess = "Số lượng tồn kho chỉ còn: " + checkvattu.SoLuong });
                    else
                    {
                        VatTu vt = new VatTu();
                        vt.ID = idvattu;
                        vt.TenVatTu = checkvattu.TenVatTu;
                        vt.DonVi = checkvattu.DonVi;
                        return Json(new
                        {
                            status = true,
                            vattu = vt
                        });
                    }
                }
                catch
                {
                    return Json(new
                    {
                        status = false
                    });
                }

            }
            else
                return Json(new
                {
                    status = false
                });
        }

        [HttpPost]
        public JsonResult TaoPhieuXuat_KTV(int idktv,int idycpv, string ghichu, List<CTPhieuXuatVatTu_KTV> list)
        {
            PhieuXuatVatTu_KTV px = new PhieuXuatVatTu_KTV();
            px.NgayXuat = DateTime.Now;
            px.IDKTV = idktv;
            px.IDYeuCauPV = idycpv;
            px.GhiChu = ghichu;
            px.Status = 1;
            px.KiemDuyet = false;
            foreach(CTPhieuXuatVatTu_KTV item in list)
            {
                item.DonGia = Double.Parse(item.DonGiaJSON, CultureInfo.InvariantCulture);
                item.SLTra = 0;
                item.SLThucTe = item.SLLay;
                VatTu vt = db.VatTus.SingleOrDefault(x => x.ID == item.IDVatTu);
                if (vt.SoLuong >= item.SLLay)
                    vt.SoLuong = vt.SoLuong - item.SLLay;
                else
                {
                    return Json(new
                    {
                        status = false,
                        mess = vt.TenVatTu + " xuất quá slg tồn kho!" 
                    });
                }
                px.CTPhieuXuatVatTu_KTV.Add(item);
            }
            db.PhieuXuatVatTu_KTV.Add(px);
            db.SaveChanges();

            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
        public JsonResult TaoCTPhieuXuat_KTV(int idphieu, int idvattu, float soluong, float dongia)
        {
            CTPhieuXuatVatTu_KTV ct = new CTPhieuXuatVatTu_KTV();
            ct.IDPX_KTV = idphieu;
            ct.IDVatTu = idvattu;
            ct.SLLay = soluong;
            ct.DonGia = dongia;
            ct.Status = 1;

            VatTu vt = db.VatTus.SingleOrDefault(x => x.ID == idvattu);
            if(vt.SoLuong >= soluong)
                vt.SoLuong = vt.SoLuong - soluong;
            else
            {
                return Json(new
                {
                    status = false,
                    mess = "Số lượng tồn kho chỉ còn: " + vt.SoLuong
                });
            }

            db.CTPhieuXuatVatTu_KTV.Add(ct);
            db.SaveChanges();
            return Json(new
            {
                status = true,

            });
        }

        [HttpPost]
        public JsonResult UpdatePhieuXuat_KTV(int id, string ghichu)
        {
            try
            {
                PhieuXuatVatTu_KTV vt = db.PhieuXuatVatTu_KTV.SingleOrDefault(x => x.ID == id);
                vt.GhiChu = ghichu;
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
                    status = false
                });
            }
        }

        [HttpPost]
        public JsonResult autoDonGia(int idvattu)
        {
            var vt = db.VatTus.Find(idvattu);
            return Json(new
            {
                status = true,
                dongia = vt.GiaBan
            });
        }
        [NonAction]
        public void DeleteCTPX_KTV(List<CTPhieuXuatVatTu_KTV> ct)
        {
            foreach (CTPhieuXuatVatTu_KTV item in ct)
            {
                var vt = db.VatTus.Find(item.IDVatTu);
                if (item.SLThucTe > 0)
                    vt.SoLuong += item.SLThucTe;
                else
                    vt.SoLuong += item.SLLay;
                db.CTPhieuXuatVatTu_KTV.Remove(item);
            }
        }

        [HttpPost]
        public JsonResult DeleteCTPhieu(int idct)
        {
            var ct = db.CTPhieuXuatVatTu_KTV.Find(idct);
            var vt = db.VatTus.Find(ct.IDVatTu);
            if (ct.SLThucTe > 0)
                vt.SoLuong += ct.SLThucTe;
            else
                vt.SoLuong += ct.SLLay;
            db.CTPhieuXuatVatTu_KTV.Remove(ct);
            db.SaveChanges();
            return Json(new { status = true });
        }

        public JsonResult DeletePX_KTV(int id)
        {
            var px = db.PhieuXuatVatTu_KTV.Find(id);
            var ct = db.CTPhieuXuatVatTu_KTV.Where(x => x.IDPX_KTV == px.ID).ToList();
            if (ct.Count == 0)
                db.PhieuXuatVatTu_KTV.Remove(px);
            else
            {
                DeleteCTPX_KTV(ct);
            }
            db.PhieuXuatVatTu_KTV.Remove(px);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        #endregion
        
        #region -- PhieuXuat_Khach --
        public ActionResult PhieuXuat_Khach(string searchString, int page = 1, int pagesize = 20)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            IQueryable<PhieuXuatVatTu_Khach> model = db.PhieuXuatVatTu_Khach.Where(x=>x.Status==1);
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
            return View(model.OrderByDescending(x => x.NgayXuat).ToPagedList(page, pagesize));
        }

        public ActionResult CreatePhieuXuat_Khach()
        {
            List<VatTu> list = db.VatTus.Where(x => x.Status == 1 && x.SoLuong > 0).ToList();
            SelectList selectlist = new SelectList(list, "ID", "TenVatTu");
            ViewBag.selectlist = selectlist;
            return View();
        }

        [HttpPost]
        public JsonResult TaoPhieuXuat_Khach(string tenKH, string ghichu, List<CTPhieuXuatVatTu_Khach> list)
        {
            PhieuXuatVatTu_Khach px = new PhieuXuatVatTu_Khach();
            px.NgayXuat = DateTime.Now;
            px.TenKhach = tenKH;
            px.GhiChu = ghichu;
            px.Status = 1;
            foreach (CTPhieuXuatVatTu_Khach item in list)
            {
                item.DonGia = Double.Parse(item.DonGiaJSON, CultureInfo.InvariantCulture);
                VatTu vt = db.VatTus.SingleOrDefault(x => x.ID == item.IDVatTu);
                if (vt.SoLuong >= item.SoLuong)
                    vt.SoLuong = vt.SoLuong - item.SoLuong;
                else
                {
                    return Json(new
                    {
                        status = false,
                        mess = "Số lượng tồn kho chỉ còn: " + vt.SoLuong
                    });
                }
                px.CTPhieuXuatVatTu_Khach.Add(item);
            }
            db.PhieuXuatVatTu_Khach.Add(px);
            db.SaveChanges();

            return Json(new
            {
                status = true,
               
            });
        }

        [HttpPost]
        public JsonResult TaoCTPhieuXuat_Khach(int idphieu, int idvattu, float soluong, float dongia)
        {
            CTPhieuXuatVatTu_Khach ct = new CTPhieuXuatVatTu_Khach();
            ct.IDPX_Khach = idphieu;
            ct.IDVatTu = idvattu;
            ct.SoLuong = soluong;
            ct.DonGia = dongia;
            ct.Status = 1;

            VatTu vt = db.VatTus.SingleOrDefault(x => x.ID == idvattu);
            if (vt.SoLuong >= soluong)
                vt.SoLuong = vt.SoLuong - soluong;
            else
            {
                return Json(new
                {
                    status = false,
                    mess = "Số lượng tồn kho chỉ còn: " + vt.SoLuong
                });
            }

            db.CTPhieuXuatVatTu_Khach.Add(ct);
            db.SaveChanges();
            return Json(new
            {
                status = true,

            });
        }

        [HttpPost]
        public JsonResult UpdatePhieuXuat_Khach(int id, string ghichu, string tenkhach)
        {
            try
            {
                PhieuXuatVatTu_Khach vt = db.PhieuXuatVatTu_Khach.SingleOrDefault(x => x.ID == id);
                vt.GhiChu = ghichu;
                vt.TenKhach = tenkhach;
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
                    status = false
                });
            }
        }


        //public JsonResult ThemCTPX_Khach(int idvattu, int soluong)
        //{
        //    if (idvattu != 0)
        //    {
        //        try
        //        {
        //            var checkvattu = db.VatTus.SingleOrDefault(x => x.ID == idvattu);
        //            if (checkvattu == null || checkvattu.SoLuong < soluong)
        //                return Json(new { status = false, mess = "Số lượng tồn kho chỉ còn: " + checkvattu.SoLuong });
        //            else
        //            {
        //                VatTu vt = new VatTu();
        //                vt.ID = idvattu;
        //                vt.TenVatTu = checkvattu.TenVatTu;
        //                return Json(new
        //                {
        //                    status = true,
        //                    vattu = vt
        //                });
        //            }
        //        }
        //        catch
        //        {
        //            return Json(new
        //            {
        //                status = false
        //            });
        //        }

        //    }
        //    else
        //        return Json(new
        //        {
        //            status = false
        //        });
        //}
        [HttpPost]
        public JsonResult DeleteCTPhieu_Khach(int idct)
        {
            var ct = db.CTPhieuXuatVatTu_Khach.Find(idct);
            var vt = db.VatTus.Find(ct.IDVatTu);
            if (ct.SoLuong > 0)
                vt.SoLuong += ct.SoLuong;
            else
                vt.SoLuong += ct.SoLuong;
            db.CTPhieuXuatVatTu_Khach.Remove(ct);
            db.SaveChanges();
            return Json(new { status = true });
        }
        [HttpPost]
        public JsonResult DeletePX_Khach(int id)
        {
            var px = db.PhieuXuatVatTu_Khach.Find(id);
            var ct = db.CTPhieuXuatVatTu_Khach.Where(x => x.IDPX_Khach == id);
            foreach(CTPhieuXuatVatTu_Khach item in ct)
            {
                var vt = db.VatTus.Find(item.IDVatTu);
                vt.SoLuong += item.SoLuong;
                db.CTPhieuXuatVatTu_Khach.Remove(item);
            }
            db.PhieuXuatVatTu_Khach.Remove(px);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        public ActionResult CTPhieuXuat_Khach(int idphieuxuat)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            List<CTPhieuXuatVatTu_Khach> list_ctphieuxuat = db.CTPhieuXuatVatTu_Khach.Where(x => x.IDPX_Khach == idphieuxuat).ToList();
            ViewBag.idphieu = idphieuxuat;
            ViewBag.NgayXuat = db.PhieuXuatVatTu_Khach.SingleOrDefault(x => x.ID == idphieuxuat).NgayXuat.Value.ToString("dd/MM/yyyy");
            ViewBag.KH = db.PhieuXuatVatTu_Khach.SingleOrDefault(x => x.ID == idphieuxuat).TenKhach.ToString();
            return View(list_ctphieuxuat);
        }

        #endregion

        [HttpPost]
        public JsonResult LayVatTu(int idvattu)
        {
            var vattu = db.VatTus.SingleOrDefault(x => x.ID == idvattu);
            if (vattu != null)
            {
                VatTu vt = new VatTu();
                vt.ID = idvattu;
                vt.TenVatTu = vattu.TenVatTu;
                vt.SoLuong = vattu.SoLuong;
                vt.DonVi = vattu.DonVi;
                vt.GiaBan = vattu.GiaBan;
                vt.GhiChu = vattu.GhiChu;
                vt.Status = vattu.Status;
                return Json(new
                {
                    status = true,
                    vattu = vt
                });
            }
            else
                return Json(new
                {
                    status = false
                });
        }

        #region -- DownloadExcel --

        [HttpGet]
        public void DownloadExcel_PhieuNhap(int id)
        {
            if(id == 0)
            {
                id = db.PhieuNhapVatTus.OrderByDescending(x => x.ID).First().ID;
            }
            var query = from T0 in db.PhieuNhapVatTus
                        join T1 in db.CTPhieuNhapVatTus on T0.ID equals T1.IDPhieuNhap
                        join T2 in db.VatTus on T1.IDVatTu equals T2.ID
                        where T0.ID == id
                        select new
                        {
                            T0.ID
                            ,T0.NgayNhap
                            ,T2.TenVatTu
                            ,T1.SoLuong
                            ,T1.DonGia
                            ,T2.DonVi
                            ,T1.NCC
                            ,T0.GhiChu
                        };
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            #region -- Header --

            Sheet.Cells["A1"].Style.Font.Bold = true;
            Sheet.Cells["A1"].Value = "BẢO AN GIA";
            Sheet.Cells["F1"].Value = DateTime.Now.ToString("HH:mm, dd/MM/yyyy");
            Sheet.Cells["A3:F3"].Merge = true;
            Sheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A3"].Style.Font.Bold = true;
            Sheet.Cells["A3"].Value = "PHIẾU NHẬP";
            Sheet.Cells["E5"].Value = "Mã phiếu nhập:";
            Sheet.Cells["B5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["A6"].Value = "Ngày nhập:";

            Sheet.Cells["E5:F5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
            Sheet.Cells["E5:F5"].Style.Font.Bold = true;
            Sheet.Cells["A8"].Value = "STT";
            Sheet.Cells["B8"].Value = "Tên vật tư";
            Sheet.Cells["C8"].Value = "Số lượng";
            Sheet.Cells["D8"].Value = "Đơn giá";
            Sheet.Cells["E8"].Value = "Đơn vị";
            Sheet.Cells["F8"].Value = "Nhà cung cấp";
            Sheet.Cells["A8:F8"].Style.Font.Bold = true;
            Sheet.Cells["A8:F8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            #endregion

            #region -- Details --
            int row = 9;
            int stt = 1;
            foreach (var item in query)
            {
                Sheet.Cells["F5"].Value = item.ID;
                Sheet.Cells["B6"].Value = item.NgayNhap.Value.ToString("dd/MM/yyyy");

                Sheet.Cells[string.Format("A{0}", row)].Value = stt;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.TenVatTu;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.SoLuong;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.DonGia;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.DonVi;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.NCC;
                row++;
                stt++;
            }
            #endregion

            #region -- Footer --

            Sheet.Cells["A9:" + string.Format("A{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("A{0}", row)].Value = "Ghi chú";
            Sheet.Cells[string.Format("A{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row)].Value = query.FirstOrDefault().GhiChu;
            Sheet.Cells[string.Format("B{0}", row)+":"+string.Format("F{0}", row)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("B{0}", row) + ":" + string.Format("F{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("B{0}", row + 2)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 2)].Value = "Người giao hàng";
            Sheet.Cells[string.Format("B{0}", row + 3)].Value = "(Ký ghi rõ họ tên)";
            Sheet.Cells[string.Format("E{0}", row + 2)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("E{0}", row + 2)].Value = "Người lập phiếu";
            Sheet.Cells[string.Format("E{0}", row + 3)].Value = "(Ký ghi rõ họ tên)";

            Sheet.Cells["A8" + ":" + string.Format("F{0}", row - 1 )].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A8" + ":" + string.Format("F{0}", row - 1)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A8" + ":" + string.Format("F{0}", row - 1)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A8" + ":" + string.Format("F{0}", row - 1)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            #endregion

            //Sau khi export thì thay đổi status phiếu nhập
            PhieuNhapVatTu phieuNhap = db.PhieuNhapVatTus.Find(id);
            phieuNhap.Status = 1;
            db.SaveChanges();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.AddHeader("Refresh", "1; url=/admin/VatTu/PhieuNhap");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.Flush();
            Response.End();
            
        }

        [HttpGet]
        public void DownloadExcel_PhieuXuatKTV(int id)
        {
            var px = db.PhieuXuatVatTu_KTV.Find(id);
            var query = from T0 in db.PhieuXuatVatTu_KTV
                        join T1 in db.CTPhieuXuatVatTu_KTV on T0.ID equals T1.IDPX_KTV
                        join T2 in db.VatTus on T1.IDVatTu equals T2.ID
                        join T3 in db.YeuCauPhucVus on T0.IDYeuCauPV equals T3.ID
                        join T4 in db.NhanViens on T0.IDKTV equals T4.ID
                        where T0.ID == id
                        select new
                        {
                            T0.ID
                            ,T0.NgayXuat
                            ,T4.TenKTV
                            ,T3.YeuCau
                            ,T3.KhachHang.HoTen
                            ,T2.TenVatTu
                            ,T1.SLLay
                            ,T1.SLTra
                            ,T1.SLThucTe
                            ,T1.DonGia
                            ,T2.DonVi
                            ,T0.GhiChu
                        };
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            #region -- Header --

            Sheet.Cells["A1"].Style.Font.Bold = true;
            Sheet.Cells["A1"].Value = "BẢO AN GIA";
            Sheet.Cells["H1"].Value = DateTime.Now.ToString("HH:mm, dd/MM/yyyy");
            Sheet.Cells["A3:F3"].Merge = true;
            Sheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A3"].Style.Font.Bold = true;
            Sheet.Cells["A3"].Value = "PHIẾU XUẤT (KTV)";
            Sheet.Cells["F4"].Value = "Mã phiếu xuất:";
            Sheet.Cells["B5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["A6"].Value = "Ngày xuất:";
            Sheet.Cells["A7"].Value = "Tên KTV:";
            Sheet.Cells["F6"].Value = "Tên khách:";
            Sheet.Cells["F7"].Value = "Yêu cầu phục vụ:";

            Sheet.Cells["F4:G4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
            Sheet.Cells["F4:G4"].Style.Font.Bold = true;
            Sheet.Cells["A10"].Value = "STT";
            Sheet.Cells["B10"].Value = "Tên vật tư";
            Sheet.Cells["C10"].Value = "Số lượng lấy đi";
            Sheet.Cells["D10"].Value = "Số lượng trả";
            Sheet.Cells["E10"].Value = "Số lượng thực tế";
            Sheet.Cells["F10"].Value = "Đơn giá";
            Sheet.Cells["G10"].Value = "Đơn vị";
            Sheet.Cells["H10"].Value = "Thành tiền";
            Sheet.Cells["A10:H10"].Style.Font.Bold = true;
            Sheet.Cells["A10:H10"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            #endregion

            #region -- Details --
            int row = 11;
            int stt = 1;
            double tongtien = 0;
            foreach (var item in query)
            {
                Sheet.Cells["G4"].Value = item.ID;
                Sheet.Cells["B6"].Value = item.NgayXuat.Value.ToString("dd/MM/yyyy");
                Sheet.Cells["B7"].Value = item.TenKTV;
                Sheet.Cells["G6"].Value = item.HoTen;
                Sheet.Cells["G7"].Value = item.YeuCau;

                Sheet.Cells[string.Format("A{0}", row)].Value = stt;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.TenVatTu;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.SLLay;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.SLTra;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.SLThucTe;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.DonGia;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.DonVi;
                if (item.SLThucTe == null || item.SLThucTe == 0)
                {
                    Sheet.Cells[string.Format("H{0}", row)].Value = item.SLLay * item.DonGia;
                    tongtien = tongtien + (double.Parse(item.SLLay.ToString()) * double.Parse(item.DonGia.ToString()));
                }
                else
                {
                    Sheet.Cells[string.Format("H{0}", row)].Value = item.SLThucTe * item.DonGia;
                    tongtien = tongtien + (double.Parse(item.SLThucTe.ToString()) * double.Parse(item.DonGia.ToString()));
                }
                row++;
                stt++;
            }
            #endregion

            #region -- Footer --

            Sheet.Cells["A11:" + string.Format("A{0}", row + 1)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("G{0}", row)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("A{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row)].Value = "Tổng tiền";
            Sheet.Cells[string.Format("H{0}", row)].Value = tongtien;
            Sheet.Cells[string.Format("A{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("H{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("A{0}", row + 1)].Value = "Ghi chú";
            Sheet.Cells[string.Format("A{0}", row + 1)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 1)].Value = query.FirstOrDefault().GhiChu;
            Sheet.Cells[string.Format("B{0}", row + 1) + ":" + string.Format("H{0}", row + 1)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("B{0}", row + 1) + ":" + string.Format("H{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("B{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("B{0}", row + 4)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("B{0}", row + 3)].Value = "Kỹ thuật viên";
            Sheet.Cells[string.Format("B{0}", row + 4)].Value = "(Ký ghi rõ họ tên)";
            Sheet.Cells[string.Format("G{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("G{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("G{0}", row + 4)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("G{0}", row + 3)].Value = "Người lập phiếu";
            Sheet.Cells[string.Format("G{0}", row + 4)].Value = "(Ký ghi rõ họ tên)";

            Sheet.Cells["A10" + ":" + string.Format("H{0}", row - 1)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A10" + ":" + string.Format("H{0}", row - 1)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A10" + ":" + string.Format("H{0}", row - 1)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A10" + ":" + string.Format("H{0}", row - 1)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            #endregion

            //Sau khi export thì thay đổi status phiếu nhập
            //PhieuNhapVatTu phieuNhap = db.PhieuNhapVatTus.Find(id);
            //phieuNhap.Status = 1;
            //db.SaveChanges();

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

        [HttpGet]
        public void DownloadExcel_PhieuXuatKhach(int id)
        {
            var query = from T0 in db.PhieuXuatVatTu_Khach
                        join T1 in db.CTPhieuXuatVatTu_Khach on T0.ID equals T1.IDPX_Khach
                        join T2 in db.VatTus on T1.IDVatTu equals T2.ID
                        where T0.ID == id
                        select new
                        {
                            T0.ID
                            ,T0.NgayXuat
                            ,T0.TenKhach
                            ,T2.TenVatTu
                            ,T1.SoLuong
                            ,T1.DonGia
                            ,T2.DonVi
                            ,T0.GhiChu
                        };
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            #region -- Header --

            Sheet.Cells["A1"].Style.Font.Bold = true;
            Sheet.Cells["A1"].Value = "BẢO AN GIA";
            Sheet.Cells["F1"].Value = DateTime.Now.ToString("HH:mm, dd/MM/yyyy");
            Sheet.Cells["A3:F3"].Merge = true;
            Sheet.Cells["A3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A3"].Style.Font.Bold = true;
            Sheet.Cells["A3"].Value = "PHIẾU XUẤT (Khách)";
            Sheet.Cells["E5"].Value = "Mã phiếu xuất:";
            Sheet.Cells["B5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["A6"].Value = "Ngày xuất:";
            Sheet.Cells["A7"].Value = "Tên khách hàng:";

            Sheet.Cells["E5:F5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
            Sheet.Cells["E5:F5"].Style.Font.Bold = true;
            Sheet.Cells["A10"].Value = "STT";
            Sheet.Cells["B10"].Value = "Tên vật tư";
            Sheet.Cells["C10"].Value = "Số lượng";
            Sheet.Cells["D10"].Value = "Đơn giá";
            Sheet.Cells["E10"].Value = "Đơn vị";
            Sheet.Cells["F10"].Value = "Thành tiền";
            Sheet.Cells["A10:F10"].Style.Font.Bold = true;
            Sheet.Cells["A10:F10"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            #endregion

            #region -- Details --
            int row = 11;
            int stt = 1;
            double tongtien = 0;
            foreach (var item in query)
            {
                Sheet.Cells["F5"].Value = item.ID;
                Sheet.Cells["B6"].Value = item.NgayXuat.Value.ToString("dd/MM/yyyy");
                Sheet.Cells["B7"].Value = item.TenKhach;

                Sheet.Cells[string.Format("A{0}", row)].Value = stt;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.TenVatTu;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.SoLuong;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.DonGia;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.DonVi;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.SoLuong * item.DonGia;
                tongtien = tongtien + (double.Parse(item.SoLuong.ToString()) * double.Parse(item.DonGia.ToString()));
                row++;
                stt++;
            }
            #endregion

            #region -- Footer --

            Sheet.Cells["A11:" + string.Format("A{0}", row + 1)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("E{0}", row)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("A{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row)].Value = "Tổng tiền";
            Sheet.Cells[string.Format("F{0}", row)].Value = tongtien;
            Sheet.Cells[string.Format("A{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("F{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("A{0}", row + 1)].Value = "Ghi chú";
            Sheet.Cells[string.Format("A{0}", row + 1)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 1)].Value = query.FirstOrDefault().GhiChu;
            Sheet.Cells[string.Format("B{0}", row + 1) + ":" + string.Format("F{0}", row + 1)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("B{0}", row + 1) + ":" + string.Format("F{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("B{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("B{0}", row + 4)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("B{0}", row + 3)].Value = "Khách hàng";
            Sheet.Cells[string.Format("B{0}", row + 4)].Value = "(Ký ghi rõ họ tên)";
            Sheet.Cells[string.Format("E{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("E{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("E{0}", row + 4)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells[string.Format("E{0}", row + 3)].Value = "Người lập phiếu";
            Sheet.Cells[string.Format("E{0}", row + 4)].Value = "(Ký ghi rõ họ tên)";

            Sheet.Cells["A10" + ":" + string.Format("F{0}", row - 1)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A10" + ":" + string.Format("F{0}", row - 1)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A10" + ":" + string.Format("F{0}", row - 1)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A10" + ":" + string.Format("F{0}", row - 1)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            #endregion

            //Sau khi export thì thay đổi status phiếu nhập
            PhieuNhapVatTu phieuNhap = db.PhieuNhapVatTus.Find(id);
            phieuNhap.Status = 1;
            db.SaveChanges();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.AddHeader("Refresh", "1; url=/admin/VatTu/PhieuNhap");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.Flush();
            Response.End();

        }

        #endregion

        [HttpPost]
        public JsonResult LoadLichSu(int id)
        {
            var rs = db.CTPhieuNhapVatTus.Where(x => x.IDVatTu == id);
            List<LichSuVatTu> list = new List<LichSuVatTu>();
            foreach(CTPhieuNhapVatTu item in rs)
            {
                LichSuVatTu ls = new LichSuVatTu();
                ls.MaPN=item.IDPhieuNhap;
                ls.NgayNhap=item.PhieuNhapVatTu.NgayNhap.Value.ToString("dd/MM/yyyy");
                ls.TenVatTu=item.VatTu.TenVatTu;
                ls.SoLuong=item.SoLuong.Value;
                ls.GiaMua = item.DonGia.Value;
                ls.NCC=item.NCC;
                list.Add(ls);
            }
            return Json(new { data=list,status=true},JsonRequestBehavior.AllowGet);
        }
    }
}