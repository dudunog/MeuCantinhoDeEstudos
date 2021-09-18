using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MeuCantinhoDeEstudos3.Models
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

        public virtual Materia Materia { get; set; }
        public DateTime DataCriacao { get; set; }
        public string UsuarioCriacao { get; set; }
        public DateTime? UltimaModificacao { get; set; }
        public String UsuarioModificacao { get; set; }
    }
}