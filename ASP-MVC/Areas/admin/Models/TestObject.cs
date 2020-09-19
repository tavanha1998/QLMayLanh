using ASP_MVC.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ASP_MVC.Areas.admin.Models
{
    public class TestObject
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        public static bool IsValidSDT(string sdt)
        {
            string pattern = @"\d{10,11}";
            if (!long.TryParse(sdt, out long a))
                return false;
            else
                return Regex.IsMatch(sdt, pattern);
        }

        public string TestNhanViens(string username, string tenktv, string sdt, bool create)
        {
            if (username == "" || username == null && create == true)
                return "Vui lòng nhập tên tài khoản";
            else if (db.NhanViens.SingleOrDefault(x => x.Username == username) != null && create == true)
                return "Tên tài khoản đã tồn tại";
            else if (tenktv == "" || tenktv == null)
                return "Vui lòng nhập họ tên";
            else if (!IsValidSDT(sdt))
                return "Vui lòng nhập đúng định dạng cho số điện thoại(Gồm 10 chữ số)";
            //else if (db.NhanViens.FirstOrDefault(x => x.SDT == sdt) != null && create==true)
            //    return "Số điện thoại đã tồn tại";
            return "OK";
        }

        public string TestKhachHangs(int id, string hoten, string sdt, string diachi)
        {
            if (hoten == "" || hoten == null)
                return "Vui lòng nhập họ tên";
            else if (!IsValidSDT(sdt))
                return "Vui lòng nhập đúng định dạng cho số điện thoại (Tối đa 11 chữ số)";
            else if (db.KhachHangs.FirstOrDefault(x => (x.SDT == sdt && x.ID != id)) != null)
                return "Số điện thoại bị trùng (khách đã tồn tại), vui lòng kiểm tra lại!";
            return "OK";
        }

        public string TestMayPhucVus(string tenmay, string vitri)
        {
            if (tenmay == "" || tenmay == null)
                return "Vui lòng nhập tên máy";
            else if (vitri == "" || vitri == null)
                return "Vui lòng nhập vị trí cho máy";
            else
                return "OK";
        }

        public string TestMayChoThues(string tenmay)
        {
            if (tenmay == "" || tenmay == null)
                return "Vui lòng nhập tên máy";
            return "OK";
        }

        public string TestYeuCauPhucVus(string diachi)
        {
            if (diachi == null)
                return "Vui lòng nhập địa chỉ";
            return "OK";
        }

        public string TestDichVu(string ma,string ten,DateTime ngayapdung)
        {
            if (ma == "" || ma == null)
                return "Vui lòng nhập mã dịch vụ";
            else if (db.DichVu_SanPham.SingleOrDefault(x => x.MaDV_SP.Contains(ma) && x.Status == true) != null)
                return "Mã dịch vụ đã tồn tại";
            else if (ten == "" || ten == null)
                return "Vui lòng nhập tên dịch vụ";
            else if (ngayapdung == null)
                return "Vui lòng nhập ngày áp dụng";
            return "OK";
        }

        public string TestKho(string tenvatdung, string ngaymua)
        {
            if (tenvatdung == null || tenvatdung == "")
                return "Vui lòng nhập tên vật dụng";
            return "OK";
        }
    }
}