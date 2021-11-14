using MeuCantinhoDeEstudos3.Models;
using System.Data.Entity.Migrations;
using System.Linq;

namespace MeuCantinhoDeEstudos3.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MeuCantinhoDeEstudosContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MeuCantinhoDeEstudosContext context)
        {
            //  This method will be called after migrating to the latest version.

            //Usuario usuario = context.Users.FirstOrDefault();

            //Materia materia = new Materia()
            //{
            //    Nome = "Matemática",
            //    CorIdentificacao = "#000",
            //    UsuarioId = usuario.Id
            //};

            //context.Materias.Add(materia);
            //context.SaveChanges();

            //Tema tema = new Tema()
            //{
            //    MateriaId = materia.MateriaId,
            //    Nome = "Probabilidade"
            //};

            //context.Temas.Add(tema);
            //context.SaveChanges();

            //Atividade atividade = new Atividade()
            //{
            //    TemaId = tema.TemaId,
            //    Descricao = "Atividade de leitura do livro de matemática sobre conjuntos"
            //};

            //context.Atividades.Add(atividade);
            //await context.SaveChangesAsync();

            //VideoAula videoAula = new VideoAula()
            //{
            //    TemaId = tema.TemaId,
            //    Descricao = "Vídeoaula do youtube sobre conjuntos",
            //    LinkVideo = "youtube.com/watch?v=dasdasdad"
            //};

            //context.VideoAulas.Add(videoAula);
            //await context.SaveChangesAsync();

            //BateriaExercicio bateriaExercicio = new BateriaExercicio()
            //{
            //    TemaId = tema.TemaId,
            //    Descricao = "Exercícios do livro de matemática sobre conjuntos",
            //    QuantidadeExercicios = 40,
            //    QuantidadeAcertos = 50
            //};

            //bateriaExercicio.CalcularAproveitamento();

            //context.BateriasExercicios.Add(bateriaExercicio);
            //await context.SaveChangesAsync();
        }
    }
}
