using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASP_MVC.EF;
using System.Web.Mvc;
using System.Web.UI;
using ASP_MVC.Areas.admin.Models;
using System.Web.Script.Serialization;
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ASP_MVC.Areas.admin.Controllers
{
    [Authorize(Roles = "0,2")]
    public class BienBanNghiemThuController : Controller
    {
        // GET: admin/BienBanNghiemThu
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public ActionResult Index(long stt, long kh)
        {
            string name = HttpContext.User.Identity.Name;
            var ktv = db.NhanViens.SingleOrDefault(x => x.Username.Equals(name) && x.Status == 1);
            if (ktv.Loai != 0)
            {
                setViewBagQL();
            }
            var ycpv = db.YeuCauPhucVus.Find(stt);
            if(ycpv.Status ==  3)
            {
                ViewBag.Hidden = true;
            }
            else
                ViewBag.Hidden = false;
            List<BienBanNghiemThu> list = db.BienBanNghiemThus.Where(x => x.IDYC == stt && x.Status == true).ToList();
            List<CTBienBanNghiemThu> listct = db.CTBienBanNghiemThus.Where(x => x.BienBanNghiemThu.IDYC == stt).ToList();
            if (list.Count > 0 && listct.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].DoanhThu = listct.Where(x=>x.IDBBNT == list[i].ID).Sum(x => x.DonGia.Value * x.SoLuong.Value);
                    list[i].Diem = listct.Where(x => x.IDBBNT == list[i].ID).Sum(x => x.Diem.Value * x.SoLuong.Value);
                    BienBanNghiemThu bbnt = db.BienBanNghiemThus.Find(list[i].ID);
                    bbnt.DoanhThu = list[i].DoanhThu;
                    bbnt.Diem = list[i].Diem;
                    db.SaveChanges();
                }
                //ViewBag.BBNT = list;

            }
            ViewBag.IDKH = kh;
            ViewBag.KH = db.KhachHangs.Find(kh);
            ViewBag.IDYC = stt;
            ViewBag.KTV = db.NhanViens.Where(x => x.Loai != 0 && x.Status == 1).ToList();
            ViewBag.BBNT = db.BienBanNghiemThus.Where(x => x.IDYC == stt && x.Status == true).ToList();
            ViewBag.CTBBNT = db.CTBienBanNghiemThus.Where(x => x.BienBanNghiemThu.IDYC == stt && x.BienBanNghiemThu.Status == true).ToList();
            ViewBag.CTPhieuXuatVT = db.CTPhieuXuatVatTu_KTV.Where(x => x.PhieuXuatVatTu_KTV.IDYeuCauPV == stt).ToList();
            var rs = db.CTBienBanNghiemThus.Where(x => x.BienBanNghiemThu.IDYC == stt).ToList();
            return View(rs);
        }
        [NonAction]
        public void setViewBagQL()
        {
            ViewBag.QLNL = true;
        }
        [NonAction]
        public void setViewBag(long kh)
        {
            List<DichVu_SanPham> dv_sp = db.DichVu_SanPham.Where(x => x.Status == true).ToList();
            ViewBag.IDDichVu = dv_sp;
            
            ViewBag.KH = db.KhachHangs.Find(kh);
        }

        public ActionResult ThemBBNT(long stt, long kh)
        {
            var ycpv = db.YeuCauPhucVus.Find(stt);
            if (ycpv.SoNgay == null)
            {
                ycpv.SoNgay = 1;
                db.SaveChanges();
            }
            else
            {
                ycpv.SoNgay += 1;
                db.SaveChanges();
            }
            BienBanNghiemThuModel bbnt = new BienBanNghiemThuModel();
            bbnt.ThemBBNT((int)stt);
            return RedirectToAction("Index","BienBanNghiemThu", new {stt,kh});
        }

        [HttpPost]
        public JsonResult timDV(string madv)
        {
            var rs = db.DichVu_SanPham.SingleOrDefault(x=>x.MaDV_SP.Equals(madv) && x.Status==true);
            return Json(new { status=true });
        }

        [HttpGet]
        public ActionResult Create(long stt, long kh)
        {
            //long id = long.Parse(Request.QueryString["kh"].ToString());
            var ycpv = db.YeuCauPhucVus.Find(stt);
            var dvsp = db.DichVu_SanPham.Where(x=>x.Status==true).ToList().Select(d=> new {
                ID= d.ID,
                Text= d.MaDV_SP + ". " + d.TenDichVu_SanPham
            });
            ViewBag.IDBBNT = int.Parse(RouteData.Values["id"].ToString());
            ViewBag.IDKH = kh;
            ViewBag.IDYC = stt;//long.Parse(Request.QueryString["stt"].ToString());
            ViewBag.DVSP = new SelectList(dvsp, "ID", "Text");
            
            if (ycpv.Loai == 2)
            {
                ViewBag.DSMayPhucVu = db.MayLanhs.Where(x => x.IDKhachHang == 1 && x.Status == 1).ToList();
            }
            else
                ViewBag.DSMayPhucVu = db.MayLanhs.Where(x => x.IDKhachHang == kh && x.Status == 1).ToList();
            setViewBag(kh);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CTBienBanNghiemThu ct)
        {
            BienBanNghiemThuModel bbnt = new BienBanNghiemThuModel();
            DichVu_SanPhamModel dvsp = new DichVu_SanPhamModel();
            long idkh = long.Parse(Request.QueryString["kh"].ToString());
            ViewBag.IDKH = idkh;
            long idyc = long.Parse(Request.QueryString["stt"].ToString());
            ViewBag.IDYC = idyc;
            int idbbnt = int.Parse(RouteData.Values["id"].ToString());
            if (ModelState.IsValid)
            {
                ct.IDBBNT = idbbnt;
                db.CTBienBanNghiemThus.Add(ct);
                db.SaveChanges();
                //return RedirectToAction("Index", new { stt = idyc, kh = idkh });
            }
            setViewBag(idkh);
            return View(ct);
        }

        [HttpPost]
        public JsonResult NewDVSP(string dvsp)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            DichVu_SanPham dvspnew = serializer.Deserialize<DichVu_SanPham>(dvsp);

            var rs = db.DichVu_SanPham.SingleOrDefault(x => x.MaDV_SP.Equals(dvspnew.MaDV_SP) && x.Status == true);
            if(rs == null)
            {
                if (dvspnew.DonGia > 0 && dvspnew.Diem > 0)
                {
                    dvspnew.Status = true;
                    dvspnew.NgayApDung = DateTime.Now;
                    db.DichVu_SanPham.Add(dvspnew);
                    db.SaveChanges();
                    return Json(new { status = true, mess = "Thêm thành công",ma=dvspnew.MaDV_SP, ten = dvspnew.TenDichVu_SanPham },JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { status = false, mess = "Đơn giá và điểm không được để trống và phải > 0" });
            }
            return Json(new {status = false, mess="Mã đã tồn tại" });
        }
        public JsonResult Success(long idyc)
        {
            var ycpv = db.YeuCauPhucVus.Find(idyc);
            if(ycpv.Loai == 2)
            {
                List<CTBBNT_MayLanh> ct = db.CTBBNT_MayLanh.Where(x => x.CTBienBanNghiemThu.BienBanNghiemThu.IDYC == idyc).ToList();
                foreach (CTBBNT_MayLanh item in ct)
                {
                    var may = db.MayLanhs.SingleOrDefault(x => x.ID == item.IDMay);
                    may.Status = 1;
                    db.SaveChanges();
                }

            }
            ycpv.Status = 3;
            ycpv.NgayHoanThanh = DateTime.Now.Date;
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        public JsonResult XoaCT(long idct)
        {
            
            var ct = db.CTBienBanNghiemThus.Find(idct);
            BienBanNghiemThu bbnt = db.BienBanNghiemThus.Find(ct.IDBBNT);
            bbnt.DoanhThu -= ct.DonGia.Value*ct.SoLuong.Value;
            bbnt.Diem -= ct.Diem.Value*ct.SoLuong.Value;
            YeuCauPhucVu yc = db.YeuCauPhucVus.Single(x => x.ID == bbnt.IDYC);
            List<CTBBNT_MayLanh> list = db.CTBBNT_MayLanh.Where(x => x.IDCTBBNT == ct.ID).ToList();
            foreach (CTBBNT_MayLanh ctml in list)
            {
                if (yc.Loai == 2)
                    ctml.MayLanh.Status = 1;
                db.CTBBNT_MayLanh.Remove(ctml);
            }
            db.CTBienBanNghiemThus.Remove(ct);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        [HttpPost]
        public JsonResult XoaBBNT(long idbbnt)
        {
            var bbnt = db.BienBanNghiemThus.Find(idbbnt);
            var ctbbnt = db.CTBienBanNghiemThus.Where(x => x.IDBBNT == idbbnt);
            if(ctbbnt.Count() > 0)
            {
                return Json(new
                {
                    status = false,
                    mess="Vui lòng đảm bảo đã xóa chi tiết BBNT"
                });
            }
            bbnt.Status = false;
            var ycpv = db.YeuCauPhucVus.SingleOrDefault(x => x.ID == bbnt.IDYC);
            ycpv.SoNgay -= 1;
            //bbnt.DoanhThu = bbnt.DoanhThu - (ct.DichVu_SanPham.DonGia * ct.SoLuong);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        public JsonResult Complete(long idyc, DateTime? ngaylamtiep)
        {
            var ycpv = db.YeuCauPhucVus.Find(idyc);
            if (!String.IsNullOrEmpty(ngaylamtiep.ToString()))
            {
                ycpv.NgayLamTiep = Convert.ToDateTime(ngaylamtiep.HasValue ? ngaylamtiep.Value.ToString("dd/MM/yyyy") : "null");
                db.SaveChanges();
            }
            else
            {
                ycpv.NgayLamTiep = null;
                db.SaveChanges();
            }
            return Json(new
            {
                status = true
            });
        }
        

        public JsonResult LoadKTV(long idbbnt)
        {
            
            List<ListKTVphutrach> list = new List<ListKTVphutrach>();
            List<KTV_BBNT> rs = db.KTV_BBNT.Where(x=>x.IDBBNT == idbbnt).ToList();
            foreach(KTV_BBNT item in rs)
            {
                ListKTVphutrach ktv = new ListKTVphutrach();
                ktv.ID = item.ID;
                ktv.TenKTV = item.NhanVien.TenKTV;
                ktv.Diem = float.Parse(item.Diem.ToString());
                ktv.DanhGia = item.DanhGia;
                list.Add(ktv);
            }
            return Json(new {
                data = list,
                status = true
            },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ThemKTV(long idktv, long idbbnt, string diem, string dg)
        {
            var rs = db.NhanViens.Find(idktv);
            if (rs == null)
                return Json(new { status = false });
            else
            {
                KTV_BBNT a = new KTV_BBNT();
                a.IDBBNT = (int)idbbnt;
                a.IDUser = (int)idktv;
                a.Diem = Double.Parse(diem.ToString(), CultureInfo.InvariantCulture);
                a.DanhGia = dg;
                a.Status = true;
                db.KTV_BBNT.Add(a);
                db.SaveChanges();
                return Json(new
                {
                    status = true,
                    id = a.ID,
                    data = rs.TenKTV
                });
            }
        }
        [HttpPost]
        public JsonResult XoaKTV(long id)
        {
            var rs = db.KTV_BBNT.Find(id);
            db.KTV_BBNT.Remove(rs);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }
        [HttpPost]
        public JsonResult DropDownMayPV(string s, long idkh, long idyc)
        {
            //long idkh = long.Parse(Request.QueryString["kh"].ToString());
            var ycpv = db.YeuCauPhucVus.Find(idyc);
            List<MayLanh> list = new List<MayLanh>();
            if (ycpv.Loai != 2)
            {
                var rs = db.MayLanhs.Where(x => x.IDKhachHang == idkh && x.Status == 1 && (x.TenMay.Contains(s) || x.Model.Contains(s) || x.ViTri.Contains(s))).ToList();
                foreach (var item in rs)
                {
                    MayLanh kh = new MayLanh();
                    kh.ID = item.ID;
                    kh.TenMay = item.TenMay;
                    kh.Model = item.Model;
                    kh.ViTri = item.ViTri;
                    list.Add(kh);
                }
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var rs = db.MayLanhs.Where(x => x.IDKhachHang == 1 && x.Status == 1 && (x.TenMay.Contains(s) || x.Model.Contains(s) || x.ViTri.Contains(s))).ToList();
                foreach (var item in rs)
                {
                    MayLanh kh = new MayLanh();
                    kh.ID = item.ID;
                    kh.TenMay = item.TenMay;
                    kh.Model = item.Model;
                    kh.ViTri = item.ViTri;
                    list.Add(kh);
                }
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DropDownDV(string s)
        {
            //long idkh = long.Parse(Request.QueryString["kh"].ToString());
            List<DichVu_SanPham> list = new List<DichVu_SanPham>();
            var rs = db.DichVu_SanPham.Where(x => x.Status == true && (x.MaDV_SP.Contains(s) || x.TenDichVu_SanPham.Contains(s))).ToList();
            foreach (var item in rs)
            {
                DichVu_SanPham kh = new DichVu_SanPham();
                kh.ID = item.ID;
                kh.MaDV_SP = item.MaDV_SP;
                kh.TenDichVu_SanPham = item.TenDichVu_SanPham;
                list.Add(kh);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult DropDownKTV(string s)
        {
            //long idkh = long.Parse(Request.QueryString["kh"].ToString());
            List<NhanVien> list = new List<NhanVien>();
            var rs = db.NhanViens.Where(x => x.TenKTV.Contains(s) || x.SDT.Contains(s)).ToList();
            foreach (var item in rs)
            {
                NhanVien nv = new NhanVien();
                nv.ID = item.ID;
                nv.TenKTV = item.TenKTV;
                nv.SDT = item.SDT;
                list.Add(nv);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        //Kiểm tra tồn tại và status là 1: có sẵn
        public bool checkMay(string maylanh, int idkh)
        {
            if(maylanh.ToUpper().Equals("ALL"))
            {
                return true;
            }
            List<string> tenmay = maylanh.Split(',').ToList();
            foreach (string s in tenmay)
            {
                var ml = db.MayLanhs.Where(x => x.Ma.ToUpper().Equals(s.ToUpper()) && x.IDKhachHang == idkh && x.Status == 1).Count();
                if (ml == 0)
                    return false;
                //////o day
            }
            return true;
        }

        [HttpPost]
        public JsonResult ThemCTBBNT(string iddv, int idkh, long idyc, string maylanh)
        {
            //long idkh = long.Parse(Request.QueryString["kh"].ToString());
            var ycpv = db.YeuCauPhucVus.Find(idyc);
            if (ycpv.Loai != 2)
            {
                if (checkMay(maylanh, idkh) == false) 
                {
                    return Json(new
                    {
                        status = false,
                        mess = "Không tìm thấy mã máy lạnh"
                    });
                }
                var dv = db.DichVu_SanPham.SingleOrDefault(x=>x.MaDV_SP.Equals(iddv));
                if (dv == null)
                    return Json(new { status = false, mess = "Không tìm thấy dịch vụ" });
                else
                {
                    DichVu_SanPham dvsp = new DichVu_SanPham();
                    dvsp.ID = dv.ID;
                    dvsp.MaDV_SP = dv.MaDV_SP;
                    dvsp.TenDichVu_SanPham = dv.TenDichVu_SanPham;
                    return Json(new
                    {
                        status = true,
                        datadv = dvsp
                    });
                }
            }
            else
            {
                if(checkMay(maylanh,1) == false)
                    return Json(new
                    {
                        status = false,
                        mess="Không tìm thấy mã máy lạnh"
                    });
                var dv = db.DichVu_SanPham.SingleOrDefault(x => x.MaDV_SP.Equals(iddv));
                if (dv == null)
                    return Json(new { status = false, mess="Không tìm thấy dịch vụ" });
                else
                {
                    DichVu_SanPham dvsp = new DichVu_SanPham();
                    dvsp.ID = dv.ID;
                    dvsp.MaDV_SP = dv.MaDV_SP;
                    dvsp.TenDichVu_SanPham = dv.TenDichVu_SanPham;
                    return Json(new
                    {
                        status = true,
                        datadv = dvsp
                    });
                }
            }
        }

        public void themMayLanhThuocCTBBNT(string maylanh, int idkh, CTBienBanNghiemThu ct)
        {
            List<string> tenmay = maylanh.Split(',').ToList();
            foreach (string s in tenmay)
            {
                CTBBNT_MayLanh cTBBNT_MayLanh = new CTBBNT_MayLanh();
                MayLanh ml = db.MayLanhs.SingleOrDefault(x => x.Ma.ToUpper().Equals(s.ToUpper()) && x.IDKhachHang == idkh && x.Status == 1);
                //if (ml != null)
                //{
                    cTBBNT_MayLanh.IDCTBBNT = ct.ID;
                    cTBBNT_MayLanh.IDMay = ml.ID;
                    if (idkh == 1 && ct.BienBanNghiemThu.YeuCauPhucVu.Loai == 2)
                        ml.Status = 2;
                    db.CTBBNT_MayLanh.Add(cTBBNT_MayLanh);
                    db.SaveChanges();
                //}

            }
        }

        [HttpPost]
        public JsonResult LuuCTBBNT(List<CTBienBanNghiemThu>list, int idkh, int idbbnt)
        {
            var bbnt = db.BienBanNghiemThus.Find(idbbnt);
            var yeucau = db.YeuCauPhucVus.SingleOrDefault(x => x.ID == bbnt.IDYC);
            foreach (CTBienBanNghiemThu item in list)
            {
                item.IDBBNT = idbbnt;
                if(item.Diem == null)
                    item.Diem = Double.Parse(item.DiemJSON.ToString(), CultureInfo.InvariantCulture);
                if (item.MayLanh.ToUpper().Equals("ALL"))
                {
                    string s = "";
                    if (yeucau.Loai == 2)
                    {
                        idkh = 1;
                    }
                    List<MayLanh> listML = db.MayLanhs.Where(x => x.IDKhachHang == idkh && x.Status == 1).ToList();
                    if (listML.Count == 0)
                        return Json(new { status = false, mess = "Không tìm thấy mã máy" });
                    for (int i = 0; i < listML.Count; i++)
                    {
                        if (s == "")
                            s = listML[i].Ma;
                        else
                            s = s + "," + listML[i].Ma;
                    }
                    item.MayLanh = s;
                }
                //ct.IDMay = int.Parse(idmay);
                //var dv = db.DichVu_SanPham.Find(ct.IDDichVu);
                //BienBanNghiemThu bbnt = db.BienBanNghiemThus.Find(idbbnt);
                //bbnt.DoanhThu = bbnt.DoanhThu + tt;
                db.CTBienBanNghiemThus.Add(item);
                db.SaveChanges();
                try
                {
                    if (yeucau.Loai == 2)
                    {
                        themMayLanhThuocCTBBNT(item.MayLanh, 1, item);
                    }
                    else
                        themMayLanhThuocCTBBNT(item.MayLanh, idkh, item);
                }
                catch
                {
                    return Json(new
                    {
                        status = false,
                        mess = "Máy hiện không có sẵn trong kho"
                    });
                }
            }
            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
        public JsonResult Update(string ctbbnt,string diem, int kh, int ycpv)
        {
            YeuCauPhucVu yeucau = db.YeuCauPhucVus.Find(ycpv);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            CTBienBanNghiemThu ctbbnt_edit = serializer.Deserialize<CTBienBanNghiemThu>(ctbbnt);
            CTBienBanNghiemThu ct = db.CTBienBanNghiemThus.SingleOrDefault(x => x.ID == ctbbnt_edit.ID);
            //ct.IDMay = ctbbnt_edit.IDMay;
            ct.MayLanh = ctbbnt_edit.MayLanh;
            ct.SoLuong = ctbbnt_edit.SoLuong;
            ct.GhiChu = ctbbnt_edit.GhiChu;
            ct.CPDauVao = ctbbnt_edit.CPDauVao;
            ct.Diem = Double.Parse(diem.ToString(), CultureInfo.InvariantCulture);
            ct.DonGia = ctbbnt_edit.DonGia;
            List<CTBBNT_MayLanh> list = db.CTBBNT_MayLanh.Where(x => x.IDCTBBNT == ct.ID).ToList();
            foreach (CTBBNT_MayLanh ctml in list)
            {
                MayLanh ml = db.MayLanhs.SingleOrDefault(x => x.ID == ctml.IDMay);
                ml.Status = 1;
                db.CTBBNT_MayLanh.Remove(ctml);
            }
            try {
                if (yeucau.Loai == 2)
                {
                    themMayLanhThuocCTBBNT(ct.MayLanh, 1, ct);
                }
                else
                    themMayLanhThuocCTBBNT(ct.MayLanh, kh, ct);
            }
            catch
            {
                return Json(new
                {
                    status = false,
                    mess = "Vui lòng kiểm tra mã máy"
                });
            }
            
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        [HttpPost]
        public JsonResult LoadBBNT(int idyc)
        {
            var rs = db.CTBienBanNghiemThus.Where(x => x.BienBanNghiemThu.IDYC == idyc).ToList();
            double tongtien = 0;
            bool s = double.TryParse(db.BienBanNghiemThus.Where(x => x.IDYC == idyc).Sum(x => x.DoanhThu.HasValue? x.DoanhThu : 0).ToString(),out tongtien);
            if (rs != null)
            {
                List<CTBBNTnew> list = new List<CTBBNTnew>();
                foreach (CTBienBanNghiemThu item in rs)
                {
                    CTBBNTnew ct = new CTBBNTnew();
                    ct.ID = item.ID;
                    ct.TenDVSP = item.DichVu_SanPham.TenDichVu_SanPham;
                    ct.DonGia = int.Parse(item.DonGia.ToString());
                    ct.SoLuong = int.Parse(item.SoLuong.ToString());
                    ct.Ma = item.MayLanh;
                    ct.CPDauVao = item.CPDauVao.HasValue? item.CPDauVao.Value : 0;
                    ct.GhiChu = item.GhiChu;
                    ct.NgayPV = item.BienBanNghiemThu.NgayLap.Value.ToString("dd/MM/yyyy");
                    list.Add(ct);
                }
                return Json(new
                {
                    status = true,
                    data = list,
                    tt=tongtien.ToString("N0"),
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    status = false
                });
            }
        }

        [HttpGet]
        public JsonResult LoadCPPX(int idyc)
        {
            int index = 0;
            var rs = db.CTPhieuXuatVatTu_KTV.Where(x => x.PhieuXuatVatTu_KTV.YeuCauPhucVu.ID == idyc).ToList();
            double tt = 0;
            if (rs != null)
            {
                List<ChiPhiXuatKho> list = new List<ChiPhiXuatKho>();
                foreach(CTPhieuXuatVatTu_KTV item in rs)
                {
                    ChiPhiXuatKho ct = new ChiPhiXuatKho();
                    ct.ID=++index;
                    ct.TenVatTu = item.VatTu.TenVatTu;
                    ct.TenKTV =item.PhieuXuatVatTu_KTV.NhanVien.TenKTV;
                    ct.DonGia = float.Parse(item.DonGia.ToString());
                    ct.SoLuongLay = float.Parse(item.SLLay.ToString());
                    if (item.SLThucTe != null || item.SLThucTe > 0)
                        ct.SoLuongTT = float.Parse(item.SLThucTe.ToString());
                    else
                        ct.SoLuongTT = float.Parse(item.SLLay.ToString());
                    ct.ThanhTien = ct.DonGia * ct.SoLuongTT;
                    tt = tt + ct.ThanhTien;
                    list.Add(ct);
                }
                return Json(new
                {
                    status = true,
                    tt = tt.ToString("N0"),
                    data = list
                }, JsonRequestBehavior.AllowGet);;
            }
            else
            {
                return Json(new
                {
                    status = false
                });
            }
        }

        [HttpPost]
        public JsonResult ThemChiPhi(int idyc, string tenCP, int soluong, int giadauvao, int dongia, string ghichu)
        {
            ChiPhiKhac cpk = new ChiPhiKhac();
            cpk.IDYCPV = idyc;
            cpk.TenChiPhi = tenCP;
            cpk.SoLuong = soluong;
            cpk.GiaDauVao = giadauvao;
            cpk.DonGia = dongia;
            cpk.GhiChu = ghichu;
            db.ChiPhiKhacs.Add(cpk);
            db.SaveChanges();
            return Json(new
            {
                status = true,
                id = cpk.ID
            }) ;
        }

        [HttpPost]
        public JsonResult XoaChiPhi(long id)
        {
            var rs = db.ChiPhiKhacs.Find(id);
            db.ChiPhiKhacs.Remove(rs);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        [HttpPost]
        public JsonResult LoadCPKhac(int idyc)
        {
            var rs = db.ChiPhiKhacs.Where(x => x.IDYCPV == idyc).ToList();
            double tt = 0;
            if (rs != null)
            {
                List<ChiPhiKhac> list = new List<ChiPhiKhac>();
                foreach (ChiPhiKhac item in rs)
                {
                    ChiPhiKhac ct = new ChiPhiKhac();
                    ct.ID = item.ID;
                    ct.IDYCPV = item.IDYCPV;
                    ct.TenChiPhi = item.TenChiPhi;
                    ct.GiaDauVao = item.GiaDauVao;
                    ct.DonGia = int.Parse(item.DonGia.ToString());
                    ct.SoLuong = int.Parse(item.SoLuong.ToString());
                    ct.GhiChu = item.GhiChu;
                    tt = tt + (ct.DonGia.Value * ct.SoLuong.Value);
                    list.Add(ct);
                }
                return Json(new
                {
                    status = true,
                    tt = tt.ToString("N0"),
                    data = list,
                }, JsonRequestBehavior.AllowGet);;
            }
            else
            {
                return Json(new
                {
                    status = false
                });
            }
        }

        [HttpPost]
        public JsonResult updateMaBBNT(int idbbnt, long ma)
        {
            BienBanNghiemThu bbnt = db.BienBanNghiemThus.Find(idbbnt);
            bbnt.Ma = ma;
            db.SaveChanges();
            return Json(new{status =true, mess ="Cập nhật thành công" });
        }

        [HttpPost]
        public JsonResult getDV(string madv)
        {
            var dv = db.DichVu_SanPham.Single(x => x.MaDV_SP.Equals(madv) && x.Status == true);
            return Json(new
            {
                status = true,
                dongia = dv.DonGia,
                diem = dv.Diem
            });
        }

        [HttpPost]
        public JsonResult getDsMay(int idkh,int idyc,string iddv)
        {
            var ycpv = db.YeuCauPhucVus.Find(idyc);
            List<MayLanh> list = new List<MayLanh>();
            List<MayLanh> rs = db.MayLanhs.ToList();
            if (ycpv.Loai == 2 )
            {
                rs = rs.Where(x => x.IDKhachHang == 1 && x.Status == 1 && x.IDDichVu.Equals(iddv)).ToList();
            }
            else
                rs = rs.Where(x => x.IDKhachHang == idkh && x.Status == 1).ToList();
            foreach(MayLanh item in rs)
            {
                MayLanh ml = new MayLanh();
                ml.ID = item.ID;
                ml.Ma = item.Ma;
                ml.ViTri = item.ViTri;
                ml.TenMay = item.TenMay;
                list.Add(ml);
            }
            return Json(new {
                status=true,
                data=list
            },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult refreshDataML(string madv, int idyc)
        {
            var ycpv = db.YeuCauPhucVus.Find(idyc);
            if( ycpv.Loai == 2)
            {
                var ml = db.MayLanhs.Where(x => x.IDDichVu.Equals(madv) && x.Status == 1);
                List<MayLanh> list = new List<MayLanh>();
                foreach(MayLanh item in ml)
                {
                    MayLanh mayLanh = new MayLanh();
                    mayLanh.ID = item.ID;
                    mayLanh.Ma = item.Ma;
                    mayLanh.ViTri = item.ViTri;
                    mayLanh.TenMay = item.TenMay;
                    list.Add(mayLanh);
                }
                return Json(new {
                    status=true,
                    data=list
                },JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { status = false});
        }

        [HttpGet]
        public void DownloadExcel(int idyc)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            Sheet.Cells.Style.Font.Name = "Arial";

            #region -- Header --

            #region ROW 1
            Sheet.Cells["A1:L1"].Merge = true;
            Sheet.Cells["A1"].Style.Font.Bold = true;
            Sheet.Cells["A1"].Style.Font.Size = 14;
            Sheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A1"].Value = "PHIẾU BÁO VÀ TÍNH HIỆU QUẢ ĐƠN HÀNG";
            #endregion

            #region ROW 2
            Sheet.Cells["A2:E2"].Merge = true;
            Sheet.Cells["A2"].Style.Font.Bold = true;
            Sheet.Cells["A2"].Style.Font.Italic = true;
            Sheet.Cells["A2"].Style.Font.Size = 9;
            Sheet.Cells["A2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            Sheet.Cells["A2"].Value = "Số tham chiếu:";

            Sheet.Cells["F2:J2"].Merge = true;
            Sheet.Cells["F2"].Style.Font.Size = 10;
            Sheet.Cells["F2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            Sheet.Cells["K2"].Style.Font.Bold = true;
            Sheet.Cells["K2"].Style.Font.Italic = true;
            Sheet.Cells["K2"].Style.Font.Size = 9;
            Sheet.Cells["K2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            Sheet.Cells["K2"].Value = "Ngày:";

            Sheet.Cells["L2"].Style.Font.Bold = true;
            Sheet.Cells["L2"].Style.Font.Size = 10;
            Sheet.Cells["L2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            #endregion

            #region ROW 3
            Sheet.Cells["A3"].Style.Font.Bold = true;
            Sheet.Cells["A3"].Value = "Tên KH:";

            Sheet.Cells["B3:E3"].Merge = true;
            Sheet.Cells["B3"].Style.Font.Bold = true;
            Sheet.Cells["B3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells["F3"].Style.Font.Bold = true;
            Sheet.Cells["F3"].Value = "ĐT:";

            Sheet.Cells["G3:I3"].Merge = true;
            Sheet.Cells["G3"].Style.Font.Bold = true;
            Sheet.Cells["G3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells["J3:L3"].Merge = true;
            Sheet.Cells["J3"].Style.Font.Bold = true;
            Sheet.Cells["J3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["J3"].Value = "MST:";
            //Border
            Sheet.Cells["A3:E3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["F3:I3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["J3:L3"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            #endregion

            #region ROW 4
            Sheet.Cells["A4"].Style.Font.Bold = true;
            Sheet.Cells["A4"].Value = "Địa chỉ:";

            Sheet.Cells["B4:E4"].Merge = true;
            Sheet.Cells["B4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells["F4"].Style.Font.Bold = true;
            Sheet.Cells["F4"].Value = "HĐ TC:";

            Sheet.Cells["G4:I4"].Merge = true;
            Sheet.Cells["G4"].Style.Font.Bold = true;
            Sheet.Cells["G4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells["J4:L4"].Merge = true;
            Sheet.Cells["J4"].Style.Font.Bold = true;
            Sheet.Cells["J4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["J4"].Value = "P.thức T/T (khoanh tròn): T.Mặt | C.Khoản";
            //Border
            Sheet.Cells["A4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["B4:E4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["F4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["G4:I4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["J4:L4"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            #endregion

            #region ROW 5
            Sheet.Cells["A5:I5"].Merge = true;
            Sheet.Cells["A5"].Style.Font.Bold = true;
            Sheet.Cells["A5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["A5"].Value = "Thông tin về hiệu quả đơn hàng (Do PTKH ghi)";

            Sheet.Cells["J5:L5"].Merge = true;
            Sheet.Cells["J5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["J5"].Value = "Đơn vị tính (khoanh tròn):   VNĐ   |   USD";
            //Border
            Sheet.Cells["A5:I5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["J5:L5"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            #endregion

            #region ROW 6&7
            Sheet.Cells["A6:A7"].Merge = true;
            Sheet.Cells["A6"].Style.Font.Bold = true;
            Sheet.Cells["A6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["A6"].Value = "stt";

            Sheet.Cells["B6:B7"].Merge = true;
            Sheet.Cells["B6"].Style.Font.Bold = true;
            Sheet.Column(2).Width = 25;
            Sheet.Cells["B6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["B6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["B6"].Value = "Tên DỊCH VỤ";

            Sheet.Cells["C6:C7"].Merge = true;
            Sheet.Cells["C6"].Style.Font.Bold = true;
            Sheet.Column(3).Width = 7;
            Sheet.Cells["C6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["C6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["C6"].Value = "đvt";

            Sheet.Cells["D6:D7"].Merge = true;
            Sheet.Cells["D6"].Style.Font.Bold = true;
            Sheet.Column(4).Width = 7;
            Sheet.Cells["D6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["D6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["D6"].Value = "Slg";

            Sheet.Cells["E6:E7"].Merge = true;
            Sheet.Cells["E6"].Style.Font.Bold = true;
            Sheet.Cells["E6"].Style.Font.Italic = true;
            Sheet.Column(5).Width = 15;
            Sheet.Cells["E6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["E6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["E6"].Value = "Giá đầu vào";

            Sheet.Cells["F6:I6"].Merge = true;
            Sheet.Cells["F6"].Style.Font.Bold = true;
            Sheet.Cells["F6"].Style.Font.Italic = true;
            Sheet.Cells["F6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["F6"].Value = "Tổng phí đầu vào";

            Sheet.Cells["F7"].Style.Font.Bold = true;
            Sheet.Cells["F7"].Style.Font.Italic = true;
            Sheet.Cells["F7"].Style.Font.Size = 9;
            Sheet.Column(6).Width = 12;
            Sheet.Cells["F7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["F7"].Value = "trong kho";

            Sheet.Cells["G7"].Style.Font.Bold = true;
            Sheet.Cells["G7"].Style.Font.Italic = true;
            Sheet.Cells["G7"].Style.Font.Size = 9;
            Sheet.Column(7).Width = 12;
            Sheet.Cells["G7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["G7"].Value = "ngoài kho";

            Sheet.Cells["H7"].Style.Font.Bold = true;
            Sheet.Cells["H7"].Style.Font.Italic = true;
            Sheet.Cells["H7"].Style.Font.Size = 9;
            Sheet.Column(8).Width = 12;
            Sheet.Cells["H7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["H7"].Value = "thuế TNCN";

            Sheet.Cells["I7"].Style.Font.Bold = true;
            Sheet.Cells["I7"].Style.Font.Italic = true;
            Sheet.Cells["I7"].Style.Font.Size = 9;
            Sheet.Column(9).Width = 12;
            Sheet.Cells["I7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["I7"].Value = "VAT vào";

            Sheet.Cells["J6:J7"].Merge = true;
            Sheet.Cells["J6"].Style.Font.Bold = true;
            Sheet.Column(10).Width = 15;
            Sheet.Cells["J6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["J6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["J6"].Value = "Đơn giá bán";

            Sheet.Cells["K6:K7"].Merge = true;
            Sheet.Cells["K6"].Style.Font.Bold = true;
            Sheet.Column(11).Width = 15;
            Sheet.Cells["K6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["K6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["K6"].Value = "Doanh thu";

            Sheet.Cells["L6:L7"].Merge = true;
            Sheet.Cells["L6"].Style.Font.Bold = true;
            Sheet.Column(12).Width = 15;
            Sheet.Cells["L6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["L6"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            Sheet.Cells["L6"].Value = "Lãi gộp";
            //Border
            Sheet.Cells["A6:L8"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A6:L8"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A6:L8"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A6:L8"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            #endregion

            #region ROW 8
            Sheet.Cells["A8"].Style.Font.Bold = true;
            Sheet.Cells["A8"].Style.Font.Size = 6;
            Sheet.Cells["A8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A8"].Value = "A";

            Sheet.Cells["B8"].Style.Font.Bold = true;
            Sheet.Cells["B8"].Style.Font.Size = 6;
            Sheet.Cells["B8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["B8"].Value = "B";

            Sheet.Cells["C8"].Style.Font.Bold = true;
            Sheet.Cells["C8"].Style.Font.Size = 6;
            Sheet.Cells["C8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["C8"].Value = "C";

            Sheet.Cells["D8"].Style.Font.Bold = true;
            Sheet.Cells["D8"].Style.Font.Size = 6;
            Sheet.Cells["D8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["D8"].Value = "D";

            Sheet.Cells["E8"].Style.Font.Bold = true;
            Sheet.Cells["E8"].Style.Font.Size = 6;
            Sheet.Cells["E8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["E8"].Value = "E";

            Sheet.Cells["F8"].Style.Font.Bold = true;
            Sheet.Cells["F8"].Style.Font.Size = 6;
            Sheet.Cells["F8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["F8"].Value = "F = D x E";

            Sheet.Cells["G8"].Style.Font.Bold = true;
            Sheet.Cells["G8"].Style.Font.Size = 6;
            Sheet.Cells["G8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["G8"].Value = "G = D x E";

            Sheet.Cells["H8"].Style.Font.Bold = true;
            Sheet.Cells["H8"].Style.Font.Size = 6;
            Sheet.Cells["H8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["H8"].Value = "I= D x E x 10%";

            Sheet.Cells["I8"].Style.Font.Bold = true;
            Sheet.Cells["I8"].Style.Font.Size = 6;
            Sheet.Cells["I8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["I8"].Value = "H = D x E x 10%";

            Sheet.Cells["J8"].Style.Font.Bold = true;
            Sheet.Cells["J8"].Style.Font.Size = 6;
            Sheet.Cells["J8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["J8"].Value = "J";

            Sheet.Cells["K8"].Style.Font.Bold = true;
            Sheet.Cells["K8"].Style.Font.Size = 6;
            Sheet.Cells["K8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["K8"].Value = "K";

            Sheet.Cells["L8"].Style.Font.Bold = true;
            Sheet.Cells["L8"].Style.Font.Size = 6;
            Sheet.Cells["L8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["L8"].Value = "L";
            #endregion

            #endregion

            #region -- Details --

            var query = from T0 in db.BienBanNghiemThus
                        join T1 in db.KTV_BBNT on T0.ID equals T1.IDBBNT
                        join T2 in db.NhanViens on T1.IDUser equals T2.ID
                        where T0.IDYC == idyc
                        select new
                        {
                            T2.TenKTV
                        };

            List<CTBienBanNghiemThu> listctbbnt = db.CTBienBanNghiemThus.Where(x => x.BienBanNghiemThu.IDYC == idyc).ToList();
            List<CTPhieuXuatVatTu_KTV> listctxuatvattu = db.CTPhieuXuatVatTu_KTV.Where(x => x.PhieuXuatVatTu_KTV.YeuCauPhucVu.ID == idyc).ToList();
            List<ChiPhiKhac> listchiphikhac = db.ChiPhiKhacs.Where(x => x.IDYCPV == idyc).ToList();
            var ycpv = db.YeuCauPhucVus.Find(idyc);
            var khachhang = db.KhachHangs.FirstOrDefault(x => x.ID == ycpv.IDKhachHang);
            Sheet.Cells["L2"].Value = DateTime.Now.ToString("dd/MM/yyyy");

            int row = 9;
            int stt = 1;
            Sheet.Cells["B3"].Value = khachhang.HoTen;
            Sheet.Cells["G3"].Value = khachhang.SDT;
            Sheet.Cells["B4"].Value = ycpv.DiaChiPhucVu;
            //List ctbbnt
            foreach (var item in listctbbnt)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = stt;
                Sheet.Cells[string.Format("A{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.DichVu_SanPham.TenDichVu_SanPham;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.SoLuong;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.CPDauVao.HasValue ? item.CPDauVao : 0;
                Sheet.Cells[string.Format("E{0}", row)].Style.Font.Italic = true;
                //Sheet.Cells[string.Format("G{0}", row)].Value = item.SoLuong * item.CPDauVao;
                Sheet.Cells[string.Format("G{0}", row)].Formula = "=" + string.Format("D{0}*", row) + string.Format("E{0}", row);
                Sheet.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("G{0}", row)].Style.Font.Italic = true;
                //Sheet.Cells[string.Format("H{0}", row)].Value = item.SoLuong * item.CPDauVao * 0.1;
                Sheet.Cells[string.Format("H{0}", row)].Formula = "=" + string.Format("G{0}*", row) + "0.1";
                Sheet.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("H{0}", row)].Style.Font.Italic = true;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.DonGia;
                //Sheet.Cells[string.Format("K{0}", row)].Value = item.DonGia * item.SoLuong;
                Sheet.Cells[string.Format("K{0}", row)].Formula = "=" + string.Format("D{0}*", row) + string.Format("J{0}", row);
                Sheet.Cells[string.Format("L{0}", row)].Formula = "=" + string.Format("K{0}-", row) + string.Format("F{0}-", row) + string.Format("G{0}-", row) + string.Format("H{0}-", row) + string.Format("I{0}", row);

                //tongngoaikho = tongngoaikho + (double.Parse(item.SoLuong.ToString()) * double.Parse(item.CPDauVao.HasValue ? item.CPDauVao.ToString() : 0.ToString()));
                //tongthueTNCN = tongthueTNCN + (double.Parse(item.SoLuong.ToString()) * double.Parse(item.CPDauVao.HasValue ? item.CPDauVao.ToString() : 0.ToString()) * 0.1);
                //tongdoanhthu = tongdoanhthu + (double.Parse(item.DonGia.ToString()) * double.Parse(item.SoLuong.ToString()));

                //tonglaigop = tonglaigop + (double.Parse(Sheet.Cells[string.Format("L{0}", row)].Text.ToString()));

                Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("L{0}", row)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Dotted;
                row++;
                stt++;
            }

            //List ct xuất vật tư
            foreach (var item in listctxuatvattu)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = stt;
                Sheet.Cells[string.Format("A{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.VatTu.TenVatTu;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.SLThucTe;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.VatTu.GiaBan.HasValue ? item.VatTu.GiaBan : 0;
                Sheet.Cells[string.Format("E{0}", row)].Style.Font.Italic = true;
                //Sheet.Cells[string.Format("G{0}", row)].Value = item.SoLuong * item.CPDauVao;
                Sheet.Cells[string.Format("F{0}", row)].Formula = "=" + string.Format("D{0}*", row) + string.Format("E{0}", row);
                Sheet.Cells[string.Format("F{0}", row)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("F{0}", row)].Style.Font.Italic = true;
                //Sheet.Cells[string.Format("H{0}", row)].Value = item.SoLuong * item.CPDauVao * 0.1;
                Sheet.Cells[string.Format("H{0}", row)].Formula = "=" + string.Format("G{0}*", row) + "0.1";
                Sheet.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("H{0}", row)].Style.Font.Italic = true;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.DonGia;
                //Sheet.Cells[string.Format("K{0}", row)].Value = item.DonGia * item.SoLuong;
                Sheet.Cells[string.Format("K{0}", row)].Formula = "=" + string.Format("D{0}*", row) + string.Format("J{0}", row);
                Sheet.Cells[string.Format("L{0}", row)].Formula = "=" + string.Format("K{0}-", row) + string.Format("F{0}", row);

                Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("L{0}", row)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Dotted;
                row++;
                stt++;
            }

            //List chi phí khác
            foreach (var item in listchiphikhac)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = stt;
                Sheet.Cells[string.Format("A{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.TenChiPhi;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.SoLuong;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.GiaDauVao.HasValue ? item.GiaDauVao : 0;
                Sheet.Cells[string.Format("E{0}", row)].Style.Font.Italic = true;
                //Sheet.Cells[string.Format("G{0}", row)].Value = item.SoLuong * item.CPDauVao;
                Sheet.Cells[string.Format("G{0}", row)].Formula = "=" + string.Format("D{0}*", row) + string.Format("E{0}", row);
                Sheet.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("G{0}", row)].Style.Font.Italic = true;
                //Sheet.Cells[string.Format("H{0}", row)].Value = item.SoLuong * item.CPDauVao * 0.1;
                Sheet.Cells[string.Format("H{0}", row)].Formula = "=" + string.Format("G{0}*", row) + "0.1";
                Sheet.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("H{0}", row)].Style.Font.Italic = true;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.DonGia;
                //Sheet.Cells[string.Format("K{0}", row)].Value = item.DonGia * item.SoLuong;
                Sheet.Cells[string.Format("K{0}", row)].Formula = "=" + string.Format("D{0}*", row) + string.Format("J{0}", row);
                Sheet.Cells[string.Format("L{0}", row)].Formula = "=" + string.Format("K{0}-", row) + string.Format("F{0}-", row) + string.Format("G{0}-", row) + string.Format("H{0}-", row) + string.Format("I{0}", row);

                Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("L{0}", row)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Dotted;
                row++;
                stt++;
            }

            Sheet.Cells["A9:" + string.Format("L{0}", row - 1)].Style.Font.Size = 9;
            #endregion

            #region -- Footer --

            Sheet.Cells["A9:" + string.Format("A{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["B9:" + string.Format("B{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["C9:" + string.Format("C{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["D9:" + string.Format("D{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["E9:" + string.Format("E{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["F9:" + string.Format("F{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["G9:" + string.Format("G{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["H9:" + string.Format("H{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["I9:" + string.Format("I{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["J9:" + string.Format("J{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["K9:" + string.Format("K{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells["L9:" + string.Format("L{0}", row - 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("E{0}", row)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("E{0}", row)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("E{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("E{0}", row)].Value = "Tổng cộng";
            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("E{0}", row)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            //Border
            Sheet.Cells[string.Format("A{0}", row) + ":" + string.Format("E{0}", row)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("F{0}", row) + ":" + string.Format("L{0}", row)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells[string.Format("F{0}", row) + ":" + string.Format("L{0}", row)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells[string.Format("F{0}", row) + ":" + string.Format("L{0}", row)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells[string.Format("F{0}", row) + ":" + string.Format("L{0}", row)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;


            Sheet.Cells[string.Format("F{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("F{0}", row)].Style.Font.Italic = true;
            //Tổng trong kho
            Sheet.Cells[string.Format("F{0}", row)].Formula = "=SUM(F9:" + string.Format("F{0})", row - 1);

            Sheet.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("G{0}", row)].Style.Font.Italic = true;
            //Tổng ngoài kho
            Sheet.Cells[string.Format("G{0}", row)].Formula = "=SUM(G9:" + string.Format("G{0})", row - 1);

            Sheet.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("H{0}", row)].Style.Font.Italic = true;
            //Tổng thuế TNCN
            Sheet.Cells[string.Format("H{0}", row)].Formula = "=SUM(H9:" + string.Format("H{0})", row - 1);

            Sheet.Cells[string.Format("K{0}", row)].Style.Font.Bold = true;
            //Tổng doanh thu
            Sheet.Cells[string.Format("K{0}", row)].Formula = "=SUM(K9:" + string.Format("K{0})", row - 1);

            Sheet.Cells[string.Format("L{0}", row)].Style.Font.Bold = true;
            //Tổng lãi gộp
            Sheet.Cells[string.Format("L{0}", row)].Formula = "=SUM(L9:" + string.Format("L{0})", row - 1);


            Sheet.Cells[string.Format("A{0}", row + 1) + ":" + string.Format("E{0}", row + 1)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row + 1) + ":" + string.Format("E{0}", row + 1)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row + 1) + ":" + string.Format("E{0}", row + 1)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 1) + ":" + string.Format("E{0}", row + 1)].Value = "VAT 10%";
            Sheet.Cells[string.Format("A{0}", row + 1) + ":" + string.Format("E{0}", row + 1)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            //Border
            Sheet.Cells[string.Format("A{0}", row + 1) + ":" + string.Format("E{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("F{0}", row + 1) + ":" + string.Format("J{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("K{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("L{0}", row + 1)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);


            Sheet.Cells[string.Format("F{0}", row + 1) + ":" + string.Format("J{0}", row + 1)].Merge = true;

            Sheet.Cells[string.Format("K{0}", row + 1)].Style.Font.Bold = true;
            //VAT 10%
            Sheet.Cells[string.Format("K{0}", row + 1)].Formula = "=" + string.Format("K{0}*0.1", row);

            Sheet.Cells[string.Format("A{0}", row + 2) + ":" + string.Format("E{0}", row + 2)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row + 2) + ":" + string.Format("E{0}", row + 2)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row + 2) + ":" + string.Format("E{0}", row + 2)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 2) + ":" + string.Format("E{0}", row + 2)].Value = "Tổng sau thuế";
            Sheet.Cells[string.Format("A{0}", row + 2) + ":" + string.Format("E{0}", row + 2)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            //Border
            Sheet.Cells[string.Format("A{0}", row + 2) + ":" + string.Format("E{0}", row + 2)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("F{0}", row + 2) + ":" + string.Format("J{0}", row + 2)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("K{0}", row + 2)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("L{0}", row + 2)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);


            Sheet.Cells[string.Format("F{0}", row + 2) + ":" + string.Format("J{0}", row + 2)].Merge = true;

            Sheet.Cells[string.Format("K{0}", row + 2)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("K{0}", row + 2)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("K{0}", row + 2)].Style.Fill.BackgroundColor.SetColor(Color.Black);
            Sheet.Cells[string.Format("K{0}", row + 2)].Style.Font.Color.SetColor(Color.White);
            //Sheet.Cells[string.Format("K{0}", row + 2)].Value =tongdoanhthu + (tongdoanhthu * 0.1);
            //Tổng sau thuế
            Sheet.Cells[string.Format("K{0}", row + 2)].Formula = "=" + string.Format("K{0}+", row) + string.Format("K{0}", row + 1);

            Sheet.Cells[string.Format("A{0}", row + 3) + ":" + string.Format("E{0}", row + 3)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row + 3) + ":" + string.Format("E{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 3) + ":" + string.Format("E{0}", row + 3)].Value = "Thông tin về các chi phí đơn hàng (Do PTKH ghi)";

            Sheet.Cells[string.Format("I{0}", row + 3) + ":" + string.Format("J{0}", row + 3)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 3) + ":" + string.Format("J{0}", row + 3)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("I{0}", row + 3) + ":" + string.Format("J{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("I{0}", row + 3) + ":" + string.Format("J{0}", row + 3)].Value = "Thực tế";
            Sheet.Cells[string.Format("I{0}", row + 3) + ":" + string.Format("J{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("K{0}", row + 3)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("K{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("K{0}", row + 3)].Value = "Dự kiến";
            Sheet.Cells[string.Format("K{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("L{0}", row + 3)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("L{0}", row + 3)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("L{0}", row + 3)].Value = "Chênh Lệch";
            Sheet.Cells[string.Format("L{0}", row + 3)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            //Border
            Sheet.Cells[string.Format("A{0}", row + 3) + ":" + string.Format("H{0}", row + 3)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("I{0}", row + 3) + ":" + string.Format("J{0}", row + 3)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("K{0}", row + 3)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            Sheet.Cells[string.Format("L{0}", row + 3)].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            Sheet.Cells[string.Format("A{0}", row + 4)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row + 4)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 4)].Value = "1";
            Sheet.Cells[string.Format("A{0}", row + 4)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("B{0}", row + 4) + ":" + string.Format("H{0}", row + 4)].Merge = true;
            Sheet.Cells[string.Format("B{0}", row + 4) + ":" + string.Format("H{0}", row + 4)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("B{0}", row + 4) + ":" + string.Format("H{0}", row + 4)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 4) + ":" + string.Format("H{0}", row + 4)].Value = "Chi phí tài chính";
            Sheet.Cells[string.Format("B{0}", row + 4) + ":" + string.Format("H{0}", row + 4)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("I{0}", row + 4) + ":" + string.Format("J{0}", row + 4)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 4) + ":" + string.Format("J{0}", row + 4)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("I{0}", row + 4) + ":" + string.Format("J{0}", row + 4)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("A{0}", row + 5)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row + 5)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 5)].Value = "2";
            Sheet.Cells[string.Format("A{0}", row + 5)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("B{0}", row + 5) + ":" + string.Format("H{0}", row + 5)].Merge = true;
            Sheet.Cells[string.Format("B{0}", row + 5) + ":" + string.Format("H{0}", row + 5)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("B{0}", row + 5) + ":" + string.Format("H{0}", row + 5)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 5) + ":" + string.Format("H{0}", row + 5)].Value = "Lãi Gộp";
            Sheet.Cells[string.Format("B{0}", row + 5) + ":" + string.Format("H{0}", row + 5)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("I{0}", row + 5) + ":" + string.Format("J{0}", row + 5)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 5) + ":" + string.Format("J{0}", row + 5)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("I{0}", row + 5) + ":" + string.Format("J{0}", row + 5)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            //Value lãi gộp
            Sheet.Cells[string.Format("I{0}", row + 5)].Formula = "=SUM(L9:" + string.Format("L{0})", row - 1);

            Sheet.Cells[string.Format("A{0}", row + 6)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row + 6)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 6)].Value = "3";
            Sheet.Cells[string.Format("A{0}", row + 6)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("B{0}", row + 6) + ":" + string.Format("H{0}", row + 6)].Merge = true;
            Sheet.Cells[string.Format("B{0}", row + 6) + ":" + string.Format("H{0}", row + 6)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("B{0}", row + 6) + ":" + string.Format("H{0}", row + 6)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 6) + ":" + string.Format("H{0}", row + 6)].Value = "Tiền mặt";
            Sheet.Cells[string.Format("B{0}", row + 6) + ":" + string.Format("H{0}", row + 6)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("I{0}", row + 6) + ":" + string.Format("J{0}", row + 6)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 6) + ":" + string.Format("J{0}", row + 6)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("I{0}", row + 6) + ":" + string.Format("J{0}", row + 6)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            //Value tiền mặt
            Sheet.Cells[string.Format("I{0}", row + 6)].Formula = "=" + string.Format("K{0}+", row) + string.Format("K{0}", row + 1);

            Sheet.Cells[string.Format("A{0}", row + 7)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row + 7)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("A{0}", row + 7)].Value = "4";
            Sheet.Cells[string.Format("A{0}", row + 7)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("B{0}", row + 7) + ":" + string.Format("H{0}", row + 7)].Merge = true;
            Sheet.Cells[string.Format("B{0}", row + 7) + ":" + string.Format("H{0}", row + 7)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("B{0}", row + 7) + ":" + string.Format("H{0}", row + 7)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("B{0}", row + 7) + ":" + string.Format("H{0}", row + 7)].Value = "Tỷ lệ lãi";
            Sheet.Cells[string.Format("B{0}", row + 7) + ":" + string.Format("H{0}", row + 7)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells[string.Format("I{0}", row + 7) + ":" + string.Format("J{0}", row + 7)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 7) + ":" + string.Format("J{0}", row + 7)].Style.Font.Bold = true;
            Sheet.Cells[string.Format("I{0}", row + 7) + ":" + string.Format("J{0}", row + 7)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;



            Sheet.Cells[string.Format("A{0}", row + 8) + ":" + string.Format("E{0}", row + 8)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row + 8) + ":" + string.Format("E{0}", row + 8)].Value = "Đánh giá hiệu quả công việc";
            Sheet.Cells[string.Format("A{0}", row + 8) + ":" + string.Format("E{0}", row + 8)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("F{0}", row + 8) + ":" + string.Format("H{0}", row + 8)].Merge = true;
            Sheet.Cells[string.Format("F{0}", row + 8) + ":" + string.Format("H{0}", row + 8)].Value = "Nhân viên triển khai";
            Sheet.Cells[string.Format("F{0}", row + 8) + ":" + string.Format("H{0}", row + 8)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("I{0}", row + 8) + ":" + string.Format("J{0}", row + 8)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 8) + ":" + string.Format("J{0}", row + 8)].Value = "NV Kế toán";
            Sheet.Cells[string.Format("I{0}", row + 8) + ":" + string.Format("J{0}", row + 8)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("K{0}", row + 8) + ":" + string.Format("L{0}", row + 8)].Merge = true;
            Sheet.Cells[string.Format("K{0}", row + 8) + ":" + string.Format("L{0}", row + 8)].Value = "NV Quản lý";
            Sheet.Cells[string.Format("K{0}", row + 8) + ":" + string.Format("L{0}", row + 8)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells[string.Format("A{0}", row + 9) + ":" + string.Format("E{0}", row + 11)].Merge = true;
            Sheet.Cells[string.Format("F{0}", row + 9) + ":" + string.Format("H{0}", row + 11)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 9) + ":" + string.Format("J{0}", row + 11)].Merge = true;
            Sheet.Cells[string.Format("K{0}", row + 9) + ":" + string.Format("L{0}", row + 11)].Merge = true;
            //Hiển thị tên KTV thực hiện
            string tenktv = "";
            try
            {
                int dem = 0;
                foreach (var item in query)
                {
                    if (dem == 2)
                    {
                        tenktv = tenktv + "\n";
                        dem = 0;
                    }
                    tenktv = tenktv + string.Format(",{0}", item.TenKTV);
                    dem++;
                }
                tenktv = tenktv.Substring(1);
            }
            catch
            {
                tenktv = "";
            }
            
            Sheet.Cells[string.Format("F{0}", row + 9)].Style.WrapText = true;
            Sheet.Cells[string.Format("F{0}", row + 9)].Style.Font.Size = 9;
            Sheet.Cells[string.Format("F{0}", row + 9)].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
            Sheet.Cells[string.Format("F{0}", row + 9)].Value = tenktv;

            Sheet.Cells[string.Format("A{0}", row + 12) + ":" + string.Format("E{0}", row + 12)].Merge = true;
            Sheet.Cells[string.Format("A{0}", row + 12) + ":" + string.Format("E{0}", row + 12)].Style.Font.Size = 10;
            Sheet.Cells[string.Format("A{0}", row + 12) + ":" + string.Format("E{0}", row + 12)].Value = "Ngày:";
            Sheet.Cells[string.Format("A{0}", row + 12) + ":" + string.Format("E{0}", row + 12)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            Sheet.Cells[string.Format("F{0}", row + 12) + ":" + string.Format("H{0}", row + 12)].Merge = true;
            Sheet.Cells[string.Format("I{0}", row + 12) + ":" + string.Format("J{0}", row + 12)].Merge = true;

            Sheet.Cells[string.Format("K{0}", row + 12) + ":" + string.Format("L{0}", row + 12)].Merge = true;
            Sheet.Cells[string.Format("K{0}", row + 12) + ":" + string.Format("L{0}", row + 12)].Value = "Phạm Trần Anh Quân";

            //Border 1->4
            Sheet.Cells[string.Format("A{0}", row + 4) + ":" + string.Format("L{0}", row + 12)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells[string.Format("A{0}", row + 4) + ":" + string.Format("L{0}", row + 12)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells[string.Format("A{0}", row + 4) + ":" + string.Format("L{0}", row + 12)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells[string.Format("A{0}", row + 4) + ":" + string.Format("L{0}", row + 12)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            //Number format
            Sheet.Cells["A9:" + string.Format("L{0}", row + 6)].Style.Numberformat.Format = "#,###";

            #endregion


            //Sheet.Cells["A:AZ"].AutoFitColumns();
            //Sheet.Cells.AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            //Response.AddHeader("Refresh", "1; url=/admin/VatTu/PhieuNhap");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.Flush();
            Response.End();

        }


    }
}