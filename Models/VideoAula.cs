using System.ComponentModel;
using MeuCantinhoDeEstudos3.Models.Auditoria;
using MeuCantinhoDeEstudos3.Models.Interfaces;

namespace MeuCantinhoDeEstudos3.Models
{
    public class VideoAula : Atividade, IEntidadeAuditada<VideoAulaAuditoria>
    {
        [DisplayName("Link do vídeo")]
        public string LinkVideo { get; set; }
    }
}