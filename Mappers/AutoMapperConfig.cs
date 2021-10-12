using AutoMapper;
using MeuCantinhoDeEstudos3.Models;
using MeuCantinhoDeEstudos3.ViewModels;

namespace MeuCantinhoDeEstudos3.Mappers
{
    public class AutoMapperConfig
    {
        public static IMapper Mapper { get; private set; }

        public static void RegisterMappings()
        {
            var configuration = new MapperConfiguration(mapper =>
            {
                mapper.CreateMap<Atividade, AtividadeViewModel>();
                mapper.CreateMap<AtividadeViewModel, Atividade>();
                mapper.CreateMap<VideoAula, VideoAulaViewModel>();
                mapper.CreateMap<VideoAulaViewModel, VideoAula>();
            });

            Mapper = configuration.CreateMapper();
        }
    }
}