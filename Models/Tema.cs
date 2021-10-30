using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models
{
    public class Tema : EntidadeAuditada<TemaAuditoria>
    {
        [Key]
        public int TemaId { get; set; }

        [DisplayName("Matéria")]
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
    }
}