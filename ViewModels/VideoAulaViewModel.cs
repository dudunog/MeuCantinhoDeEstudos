using MeuCantinhoDeEstudos3.Models;
using System.ComponentModel;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class VideoAulaViewModel : AtividadeViewModel
    {
        [DisplayName("Link do vídeo")]
        public string LinkVideo { get; set; }
    }
}