using MeuCantinhoDeEstudos3.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class AtividadeViewModel : Entidade
    {
        public int AtividadeId { get; set; }

        [DisplayName("Matéria")]
        public int MateriaId { get; set; }

        [DisplayName("Tema")]
        public int TemaId { get; set; }

        [Required]
        [DisplayName("Descrição/Observação")]
        public string Descricao { get; set; }

        public virtual Tema Tema { get; set; }

        public SelectList Materias { get; set; }

        public SelectList Temas { get; set; }
    }
}