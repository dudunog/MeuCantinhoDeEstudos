using FastMember;
using MeuCantinhoDeEstudos3.Models;
using MeuCantinhoDeEstudos3.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MeuCantinhoDeEstudos3.Extensions
{
    public static class MyDbContextExtensions
    {
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

        public async static Task MyBulkInsertAsync<T>(this DbContext @this, IEnumerable<T> entities) where T : class
        {
            try
            {
                var currentTime = DateTime.Now;
                foreach (var entry in @this.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    //IsAssignableToGenericType(e.Entity.GetType(), typeof(IEntidade))))
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
                {
                    ApplyCreationAndModificationProperts(entry);
                }

                await @this.BulkInsertAsync(entities);
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

            foreach (var entidade in @this.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    IsAssignableToGenericType(e.Entity.GetType(), typeof(IEntidadeAuditada<>))))
                    // typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
            {
                var tipoTabelaAuditoria = entidade.Entity.GetType().GetInterfaces()[0].GenericTypeArguments[0];
                var registroTabelaAuditoria = Activator.CreateInstance(tipoTabelaAuditoria);

                var classePrincipal = ObjectAccessor.Create(entidade.Entity);
                var typer_classePrincipal = TypeAccessor.Create(entidade.Entity.GetType());
                var classeAuditoria = ObjectAccessor.Create(registroTabelaAuditoria);

                foreach (var member in typer_classePrincipal.GetMembers())
                {
                    classeAuditoria[member.Name] = classePrincipal[member.Name];
                }

                @this.Set(registroTabelaAuditoria.GetType()).Add(classeAuditoria.Target);
            }

            await @this.BulkSaveChangesAsync();
        }

        public async static Task MyBulkUpdateAsync<T>(this DbContext @this, IEnumerable<T> entities) where T : class
        {
            try
            {
                var currentTime = DateTime.Now;
                foreach (var entry in @this.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    //IsAssignableToGenericType(e.Entity.GetType(), typeof(IEntidade))))
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
                {
                    ApplyCreationAndModificationProperts(entry);
                }

                await @this.BulkUpdateAsync(entities);
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

            foreach (var entidade in @this.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    IsAssignableToGenericType(e.Entity.GetType(), typeof(IEntidadeAuditada<>))))
                    // typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
            {
                var tipoTabelaAuditoria = entidade.Entity.GetType().GetInterfaces()[0].GenericTypeArguments[0];
                var registroTabelaAuditoria = Activator.CreateInstance(tipoTabelaAuditoria);

                var classePrincipal = ObjectAccessor.Create(entidade.Entity);
                var typer_classePrincipal = TypeAccessor.Create(entidade.Entity.GetType());
                var classeAuditoria = ObjectAccessor.Create(registroTabelaAuditoria);

                foreach (var member in typer_classePrincipal.GetMembers())
                {
                    if (!member.Name.Equals("_entityWrapper"))
                    {
                        classeAuditoria[member.Name] = classePrincipal[member.Name];
                    }
                }

                @this.Set(registroTabelaAuditoria.GetType()).Add(classeAuditoria.Target);
            }

            await @this.BulkSaveChangesAsync();
        }

        public static void ApplyCreationAndModificationProperts(DbEntityEntry entry)
        {
            var currentTime = DateTime.Now;

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
    }
}