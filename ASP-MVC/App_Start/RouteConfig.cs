using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ASP_MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ASP_MVC.Controllers" } 
            );

            //routes.MapRoute(
            //    name: "admin",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
            //    namespaces: new[] { "ASP_MVC.Areas.admin.Controllers" }
            //);

            //routes.MapRoute(
            //    name: "KTV",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
            //    namespaces: new[] { "ASP_MVC.Areas.KTV.Controllers" }
            //);
        }
    }
}
