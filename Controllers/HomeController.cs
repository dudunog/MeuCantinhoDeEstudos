using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "O lugar ideal para você ter um estudo de alto rendimento!";

            return View();
        }
    }
}