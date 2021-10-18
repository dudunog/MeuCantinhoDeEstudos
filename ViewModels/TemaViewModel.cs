using MeuCantinhoDeEstudos3.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class TemaViewModel : Entidade
    {
        [DisplayName("Tema")]
        public int TemaId { get; set; }

        [DisplayName("Matéria")]
        public int MateriaId { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tema")]
        public string Nome { get; set; }

        public virtual Materia Materia { get; set; }

        public virtual ICollection<Atividade> Atividades { get; set; }

        public SelectList Materias { get; set; }
    }
}