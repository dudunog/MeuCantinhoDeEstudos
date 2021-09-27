using EntityFramework.Audit;
using MeuCantinhoDeEstudos3.Models;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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

            //auditConfiguration.IsAuditable<Usuario>();

            //auditConfiguration.IsAuditable<Usuario>()
            //    .DisplayMember(t => t.UsuarioCriacao);
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
