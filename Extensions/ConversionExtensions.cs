using MeuCantinhoDeEstudos3.Models;
using MeuCantinhoDeEstudos3.ViewModels;

namespace MeuCantinhoDeEstudos3.Extensions
{
    public static class ConversionExtensions
    {
        public static Atividade ToAtividade(this AtividadeViewModel viewModel)
        {
            return new Atividade()
            {
                TemaId = viewModel.TemaId,
                Descricao = viewModel.Descricao
            };
        }

        public static VideoAula ToVideoAula(this VideoAulaViewModel viewModel)
        {
            return new VideoAula()
            {
                TemaId = viewModel.TemaId,
                Descricao = viewModel.Descricao,
                LinkVideo = viewModel.LinkVideo
            };
        }

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