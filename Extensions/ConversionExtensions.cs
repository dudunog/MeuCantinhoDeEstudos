using MeuCantinhoDeEstudos3.Models;
using MeuCantinhoDeEstudos3.ViewModels;

namespace MeuCantinhoDeEstudos3.Extensions
{
    public static class ConversionExtensions
    {
        public static BateriaExercicio ToBateriaExercicio(this BateriaExercicioViewModel viewModel)
        {
            return new BateriaExercicio()
            {
                TemaId = viewModel.TemaId,
                Descricao = viewModel.Descricao,
                QuantidadeExercicios = viewModel.QuantidadeExercicios,
                QuantidadeAcertos = viewModel.QuantidadeAcertos
            };
        }
    }
}