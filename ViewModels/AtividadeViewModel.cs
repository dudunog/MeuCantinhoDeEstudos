using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class AtividadeViewModel
    {
        [DisplayName("Matéria")]
        public int MateriaId { get; set; }

        [DisplayName("Tema")]
        public int TemaId { get; set; }

        [Required]
        [DisplayName("Descrição/Observação")]
        public string Descricao { get; set; }
    }
}