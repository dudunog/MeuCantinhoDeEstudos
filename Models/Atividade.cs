using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models
{
    public class Atividade : Entidade
    {
        [Key]
        public int AtividadeId { get; set; }

        public int TemaId { get; set; }

        [Required]
        [DisplayName("Descrição/Observação")]
        public string Descricao { get; set; }

        public virtual Materia Materia { get; set; }

        [ForeignKey(nameof(TemaId))]
        public virtual Tema Tema { get; set; }
    }
}