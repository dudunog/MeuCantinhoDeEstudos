using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models.Auditoria
{
    public class MateriaAuditoria
    {
        [Key]
        public int Id { get; set; }

        public int MateriaId { get; set; }

        public int UsuarioId { get; set; }

        [Required]
        [StringLength(200)]
        [DisplayName("Matéria")]
        public string Nome { get; set; }

        [StringLength(6)]
        [DisplayName("Cor de identificacao")]
        public string CorIdentificacao { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; }

        public virtual ICollection<Tema> Temas { get; set; }

        public DateTime DataCriacao { get; set; }

        public string UsuarioCriacao { get; set; }

        public DateTime? UltimaModificacao { get; set; }

        public String UsuarioModificacao { get; set; }
    }
}