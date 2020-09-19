using ASP_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ASP_MVC.EF;

namespace ASP_MVC.Controllers
{
    public class HomeController : Controller
    {
        private QLMayLanhEntities db = new QLMayLanhEntities();
        //[Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginModel model)
        {
            string tk = Request.Form.Get("txtUsername");
            string mk = Request.Form.Get("txtPassword");
            //var rs = model.Login(tk, mk);
            var nhanvien = db.NhanViens.Where(x => x.Username.Equals(tk) && x.Status != 2).SingleOrDefault();
            if (nhanvien != null && nhanvien.Status == 1)
            {
                if ((nhanvien.Loai == 0 || nhanvien.Loai == 2) && Membership.ValidateUser(tk, model.CreateMD5(model.Base64Encode(mk))) && ModelState.IsValid)
                {
                    //Session.Add("Account", tk);
                    FormsAuthentication.SetAuthCookie(tk, false);
                    return RedirectToAction("Index", "Admin");
                }
                else if (nhanvien.Loai == 1 && Membership.ValidateUser(tk, model.CreateMD5(model.Base64Encode(mk))) && ModelState.IsValid)
                {
                    FormsAuthentication.SetAuthCookie(tk, false);
                    return RedirectToAction("Kho", "Admin");
                }
                else if(nhanvien.Loai == 3 && Membership.ValidateUser(tk, model.CreateMD5(model.Base64Encode(mk))) && ModelState.IsValid)
                {
                    FormsAuthentication.SetAuthCookie(tk, false);
                    return Redirect("~/KTV/Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                }
            }
            else
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            }
            return View();
        }
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home",new {area="" });
        }
    }
}