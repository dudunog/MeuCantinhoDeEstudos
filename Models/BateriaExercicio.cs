using System.ComponentModel;
using MeuCantinhoDeEstudos3.Models.Auditoria;
using MeuCantinhoDeEstudos3.Models.Interfaces;

namespace MeuCantinhoDeEstudos3.Models
{
    public class BateriaExercicio : Atividade, IEntidadeAuditada<BateriaExercicioAuditoria>
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