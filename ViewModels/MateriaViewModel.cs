using MeuCantinhoDeEstudos3.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class MateriaViewModel : Entidade
    {
        [DisplayName("Matéria")]
        public int MateriaId { get; set; }
       
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(200)]
        [DisplayName("Matéria")]
        public string Nome { get; set; }

        [StringLength(7)]
        [DisplayName("Cor de identificacao")]
        public string CorIdentificacao { get; set; }

        public virtual Usuario Usuario { get; set; }

        public virtual ICollection<Tema> Temas { get; set; }
    }
}