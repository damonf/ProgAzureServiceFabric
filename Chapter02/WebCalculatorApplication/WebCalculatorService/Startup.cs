using System.Web.Http;
using Owin;
using WebCalculatorService.App_Start;

namespace WebCalculatorService
{
    public class Startup : IOwinAppBuilder
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            FormatterConfig.ConfigureFormatters(config.Formatters);
            RouteConfig.RegisterRoutes(config.Routes);
            appBuilder.UseWebApi(config);
        }
    }
}
