using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MeuCantinhoDeEstudos3.Models
{

    public class MeuCantinhoDeEstudosContext : IdentityDbContext<Usuario, Grupo, int, UsuarioLogin, UsuarioGrupo, UsuarioIdentificacao>
    {
        public MeuCantinhoDeEstudosContext()
            : base("DefaultConnection")
        {
            //Database.SetInitializer<MeuCantinhoDeEstudosContext>(new CreateDatabaseIfNotExists<MeuCantinhoDeEstudosContext>());
            //Configuration.ProxyCreationEnabled = false;
        }

        public static MeuCantinhoDeEstudosContext Create()
        {
            return new MeuCantinhoDeEstudosContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<MateriaAuditoria>().ToTable("MateriaAuditoria");
            //modelBuilder.Entity<TemaAuditoria>().ToTable("TemaAuditoria");

            base.OnModelCreating(modelBuilder);
        }

        public System.Data.Entity.DbSet<MeuCantinhoDeEstudos3.Models.Materia> Materias { get; set; }

        public System.Data.Entity.DbSet<MeuCantinhoDeEstudos3.Models.Tema> Temas { get; set; }

        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public override int SaveChanges()
        {
            try
            {
                var currentTime = DateTime.Now;
                foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity != null && 
                IsAssignableToGenericType(e.Entity.GetType(), typeof(IEntidade<>))))
                {
                    if (entry.State == EntityState.Added)
                    {

                        if (entry.Property("DataCriacao") != null)
                        {
                            entry.Property("DataCriacao").CurrentValue = currentTime;
                        }
                        if (entry.Property("UsuarioCriacao") != null)
                        {
                            entry.Property("UsuarioCriacao").CurrentValue = HttpContext.Current != null ? HttpContext.Current.User.Identity.Name : "Usuario";
                        }
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property("DataCriacao").IsModified = false;
                        entry.Property("UsuarioCriacao").IsModified = false;

                        if (entry.Property("UltimaModificacao") != null)
                        {
                            entry.Property("UltimaModificacao").CurrentValue = currentTime;
                        }
                        if (entry.Property("UsuarioModificacao") != null)
                        {
                            entry.Property("UsuarioModificacao").CurrentValue = HttpContext.Current != null ? HttpContext.Current.User.Identity.Name : "Usuario";
                        }
                    }
                }

                foreach (var entidade in ChangeTracker.Entries())
                {
                    var tipoTabelaAuditoria = entidade.Entity.GetType().GetInterfaces()[0].GenericTypeArguments[0];
                    var registroTabelaAuditoria = Activator.CreateInstance(tipoTabelaAuditoria);

                    // Isto aqui é lento, mas serve como exemplo. 
                    // Depois procure trocar por FastMember ou alguma outra estratégia de cópia.
                    foreach (var propriedade in entidade.Entity.GetType().GetProperties())
                    {
                        registroTabelaAuditoria.GetType()
                                               .GetProperty(propriedade.Name)
                                               .SetValue(registroTabelaAuditoria, entidade.Entity.GetType().GetProperty(propriedade.Name).GetValue(entidade.Entity, null));
                    }

                    this.Set(registroTabelaAuditoria.GetType()).Add(registroTabelaAuditoria);
                }

                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);

                var exceptionsMessage = string.Concat(ex.Message, "Os erros de validações são: ", fullErrorMessage);

                throw new DbEntityValidationException(exceptionsMessage, ex.EntityValidationErrors);
            }
            catch(DbUpdateException ex)
            {
                foreach (var eve in ex.Entries)
                {
                    Console.WriteLine(eve.Entity);
                    foreach (var item in eve.CurrentValues.PropertyNames)
                    {
                        Console.WriteLine(item);
                    }
                }
                throw;
            }
        }

    }
}