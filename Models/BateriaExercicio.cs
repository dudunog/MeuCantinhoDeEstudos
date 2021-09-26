using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MeuCantinhoDeEstudos3.Models
{
    public class BateriaExercicio : Atividade
    {
        [DisplayName("Quantidade de exercícios")]
        public int QuantidadeExercicios { get; set; }

        [DisplayName("Quantidade de acertos")]

        public int QuantidadeAcertos { get; set; }

        public double Aproveitamento { get; set; }

        public void CalcularAproveitamento()
        {
            this.Aproveitamento = ((double)this.QuantidadeAcertos/this.QuantidadeExercicios * 100);
        }
    }
}