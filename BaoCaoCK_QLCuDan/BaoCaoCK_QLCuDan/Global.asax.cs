using System.Web.Mvc;
using System.Web.Routing;

namespace BaoCaoCK_QLCuDan
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // --- THÊM DÒNG NÀY ---
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;
            // ---------------------

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}