using System.ComponentModel.DataAnnotations;
using System.Web;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class UploadViewModel
    {
        [Required]
        public HttpPostedFileBase File { get; set; }
    }
}