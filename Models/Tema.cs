using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MeuCantinhoDeEstudos3.Models
{
    public class Tema : IEntidade<TemaAuditoria>
    {
        [Key]
        public int TemaId { get; set; }
        public int MateriaId { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tema")]
        public string Nome { get; set; }

        public virtual Materia Materia { get; set; }
        public DateTime DataCriacao { get; set; }
        public string UsuarioCriacao { get; set; }
        public DateTime? UltimaModificacao { get; set; }
        public string UsuarioModificacao { get; set; }
    }
}