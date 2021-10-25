using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class UploadViewModel
    {
        [Required]
        [DisplayName("Arquivo")]
        public HttpPostedFileBase File { get; set; }
    }
}