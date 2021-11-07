using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models.Auditoria
{
    public class BateriaExercicioAuditoria
    {
        [Key]
        public int Id { get; set; }

        public int AtividadeId { get; set; }

        public int TemaId { get; set; }

        [Required]
        public string Descricao { get; set; }

        public int QuantidadeExercicios { get; set; }

        public int QuantidadeAcertos { get; set; }

        public double Aproveitamento { get; set; }

        [ForeignKey(nameof(TemaId))]
        public virtual Tema Tema { get; set; }

        public DateTime DataCriacao { get; set; }

        public string UsuarioCriacao { get; set; }

        public DateTime? UltimaModificacao { get; set; }

        public string UsuarioModificacao { get; set; }
    }
}