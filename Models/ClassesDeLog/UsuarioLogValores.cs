using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models.ClassesDeLog
{
    public class UsuarioLogValores
    {
        [Key]
        public int UsuarioLogValoresId { get; set; }

        public int UsuarioLogId { get; set; }

        public string NomePropriedade { get; set; }

        public string ValorNovo { get; set; }

        public string ValorAntigo { get; set; }

        [ForeignKey(nameof(UsuarioLogId))]
        public UsuarioLog UsuarioLog { get; set; }
    }
}