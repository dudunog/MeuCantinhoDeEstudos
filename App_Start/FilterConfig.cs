using StackExchange.Profiling.Mvc;
using System.Web;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ProfilingActionFilter());
        }
    }
}
