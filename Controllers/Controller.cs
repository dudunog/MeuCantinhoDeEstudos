using MeuCantinhoDeEstudos3.Extensions;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.Controllers
{
    public abstract class Controller : System.Web.Mvc.Controller
    {
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new CustomJsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }
    }
}