using ASP_MVC.Areas.KTV.Models;
using ASP_MVC.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_MVC.Areas.KTV.Controllers
{
    public class KhoVatTuController : Controller
    {
        // GET: KTV/KhoVatTu
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public ActionResult Index(string searchString, int page = 1, int pagesize = 20)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            IQueryable<PhieuXuatVatTu_KTV> model = db.PhieuXuatVatTu_KTV.Where(x => x.Status == 1 && x.IDKTV == ktv.ID);
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
            setViewBag();
            return View(model.OrderBy(x => x.Status).ToPagedList(page, pagesize));
        }

        public void setViewBag()
        {
            var listycpv = db.YeuCauPhucVus.Where(x => x.Status != 3 && x.Status != 4).ToList().Select(d => new {
                ID = d.ID,
                Text = d.KhachHang.HoTen + " - " + d.YeuCau + " - " + d.NgayLap.Value.ToString("dd/MM/yyyy")
            });
            SelectList selectlistycpv = new SelectList(listycpv, "ID", "Text");
            ViewBag.selectlistycpv = selectlistycpv;

            List<VatTu> list = db.VatTus.Where(x => x.Status == 1 && x.SoLuong > 0).ToList();
            SelectList selectlist = new SelectList(list, "ID", "TenVatTu");
            ViewBag.selectlist = selectlist;
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

        [HttpPost]
        public JsonResult ThemCTPX(int idvattu, int soluong)
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
        public JsonResult LapPhieuXuat(int idycpv, string ghichu, List<CTPhieuXuatVatTu_KTV> listCT)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv == null)
                return Json(new
                {
                    status = false,
                    mess = "Có lỗi xảy ra"
                });
            PhieuXuatVatTu_KTV px = new PhieuXuatVatTu_KTV();
            px.GhiChu = ghichu;
            px.IDKTV = ktv.ID;
            px.IDYeuCauPV = idycpv;
            px.KiemDuyet = false;
            px.Status = 1;
            px.NgayXuat = DateTime.Now;
            foreach(CTPhieuXuatVatTu_KTV value in listCT)
            {
                VatTu vt = db.VatTus.SingleOrDefault(x => x.ID == value.IDVatTu);
                if (vt.SoLuong >= value.SLLay)
                    vt.SoLuong = vt.SoLuong - value.SLLay;
                else
                {
                    return Json(new
                    {
                        status = false,
                        mess = "Số lượng tồn kho chỉ còn: " + vt.SoLuong
                    });
                }
                value.DonGia = Double.Parse(value.DonGiaJSON.ToString(), CultureInfo.InvariantCulture);
                px.CTPhieuXuatVatTu_KTV.Add(value);
            }
            db.PhieuXuatVatTu_KTV.Add(px);
            db.SaveChanges();
            return Json(new {
                status=true,
            });
        }

        [HttpPost]
        public JsonResult CTPhieu(int idp)
        {
            var rs = db.CTPhieuXuatVatTu_KTV.Where(x => x.IDPX_KTV == idp);
            if (rs == null)
                return Json(new
                {
                    status = true,
                });
            List<CTPX_KTV> list = new List<CTPX_KTV>();
            foreach (CTPhieuXuatVatTu_KTV item in rs)
            {
                CTPX_KTV ct = new CTPX_KTV();
                ct.TenVatTu = item.VatTu.TenVatTu;
                ct.SLLay = item.SLLay.Value;
                ct.SLTra = item.SLTra.HasValue?item.SLTra.Value : 0;
                ct.SLThucTe = item.SLThucTe.HasValue ? item.SLThucTe.Value : 0;
                ct.DonVi = item.VatTu.DonVi;
                ct.GiaBan = item.DonGia.Value;
                if (ct.SLThucTe == 0)
                    ct.ThanhTien = ct.SLLay * ct.GiaBan;
                else
                    ct.ThanhTien = ct.SLThucTe * ct.GiaBan;
                list.Add(ct);
            }
            return Json(new
            {
                data = list,
                status = true
            }, JsonRequestBehavior.AllowGet);
        }
    }
}