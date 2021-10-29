using System.Web.Optimization;
using EntityFramework.Audit;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using MeuCantinhoDeEstudos3.Mappers;

namespace MeuCantinhoDeEstudos3
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MiniProfilerEF6.Initialize();
            MiniProfiler.Configure(new MiniProfilerOptions
            {
                RouteBasePath = "~/profiler",
                ColorScheme = ColorScheme.Dark
            });

            var auditConfiguration = AuditConfiguration.Default;
            auditConfiguration.IncludeRelationships = true;
            auditConfiguration.LoadRelationships = true;
            auditConfiguration.DefaultAuditable = true;

            AutoMapperConfig.RegisterMappings();
        }

        protected void Application_BeginRequest()
        {
            if (Request.IsLocal)
            {
                MiniProfiler.StartNew();
            }
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Current?.Stop();
        }
    }
}
