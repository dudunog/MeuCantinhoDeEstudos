using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models
{
    public class Tema : IEntidadeAuditada<TemaAuditoria>
    {
        [Key]
        public int TemaId { get; set; }

        public int MateriaId { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tema")]
        public string Nome { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(MateriaId))]
        public virtual Materia Materia { get; set; }

        [JsonIgnore]
        public virtual ICollection<Atividade> Atividades { get; set; }

        [DisplayName("Criado em")]
        public DateTime DataCriacao { get; set; }

        [DisplayName("Criado por")]
        public string UsuarioCriacao { get; set; }

        [DisplayName("Modificado em")]
        public DateTime? UltimaModificacao { get; set; }

        public string UsuarioModificacao { get; set; }
    }
}