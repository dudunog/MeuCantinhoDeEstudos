using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models
{
    public class Materia : IEntidadeAuditada<MateriaAuditoria>
    {
        [Key]
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

        [DisplayName("Criado em")]
        public DateTime DataCriacao { get; set; }

        [DisplayName("Criado por")]
        public string UsuarioCriacao { get; set; }

        [DisplayName("Modificado em")]
        public DateTime? UltimaModificacao { get; set; }

        public string UsuarioModificacao { get; set; }
    }
}