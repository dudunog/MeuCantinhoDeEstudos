using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models
{
    public class UsuarioLogValue
    {
        [Key]
        public int UsuarioLogValueId { get; set; }

        public int UsuarioLogId { get; set; }

        public string NomeColuna { get; set; }

        public string ValorNovo { get; set; }

        public string ValorAntigo { get; set; }

        [ForeignKey(nameof(UsuarioLogId))]
        public UsuarioLog UsuarioLog { get; set; }
    }
}