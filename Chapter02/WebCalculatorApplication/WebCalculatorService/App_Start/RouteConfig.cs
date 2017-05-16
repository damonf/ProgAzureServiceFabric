using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebCalculatorService.App_Start
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "CalculatorApi",
                routeTemplate: "api/{action}",
                defaults: new { controller = "Default" }
            );
        }
    }
}
