using System.ComponentModel;

namespace MeuCantinhoDeEstudos3.Models
{
    public class VideoAula : Atividade
    {
        [DisplayName("Link do vídeo")]
        public string LinkVideo { get; set; }
    }
}