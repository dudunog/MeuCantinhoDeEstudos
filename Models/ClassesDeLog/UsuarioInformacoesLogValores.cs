using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models.ClassesDeLog
{
    public class UsuarioInformacoesLogValores
    {
        [Key]
        public int UsuarioInformacoesLogValoresId { get; set; }

        public int UsuarioInformacoesLogId { get; set; }

        public string NomePropriedade { get; set; }

        public string ValorNovo { get; set; }

        public string ValorAntigo { get; set; }

        [ForeignKey(nameof(UsuarioInformacoesLogId))]
        public virtual UsuarioInformacoesLog UsuarioInformacoesLog { get; set; }
    }
}