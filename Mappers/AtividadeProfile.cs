using AutoMapper;
using MeuCantinhoDeEstudos3.Models;
using MeuCantinhoDeEstudos3.ViewModels;

namespace MeuCantinhoDeEstudos3.Mappers
{
    public class AtividadeProfile : Profile
    {
        public AtividadeProfile()
        {
            CreateMap<Atividade, AtividadeViewModel>();
            CreateMap<AtividadeViewModel, Atividade>();
        }
    }
}