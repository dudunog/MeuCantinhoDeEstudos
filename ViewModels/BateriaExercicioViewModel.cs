using System.ComponentModel;

namespace MeuCantinhoDeEstudos3.ViewModels
{
    public class BateriaExercicioViewModel : AtividadeViewModel
    {
        [DisplayName("Quantidade de exercícios")]
        public int QuantidadeExercicios { get; set; }

        [DisplayName("Quantidade de acertos")]
        public int QuantidadeAcertos { get; set; }
    }
}