using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models.Auditoria
{
    public class TemaAuditoria
    {
        [Key]
        public int Id { get; set; }

        public int TemaId { get; set; }

        public int MateriaId { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tema")]
        public string Nome { get; set; }

        [ForeignKey(nameof(MateriaId))]
        public virtual Materia Materia { get; set; }

        public virtual ICollection<Atividade> Atividades { get; set; }

        public DateTime DataCriacao { get; set; }

        public string UsuarioCriacao { get; set; }

        public DateTime? UltimaModificacao { get; set; }

        public String UsuarioModificacao { get; set; }
    }
}