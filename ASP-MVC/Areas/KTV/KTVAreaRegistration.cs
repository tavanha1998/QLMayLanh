using System.Web.Mvc;

namespace ASP_MVC.Areas.KTV
{
    public class KTVAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "KTV";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "KTV_default",
                "KTV/{controller}/{action}/{id}",
                new { action = "Index",controller="Home", id = UrlParameter.Optional }
            );
        }
    }
}