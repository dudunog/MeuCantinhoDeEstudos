using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FastMember;
using MeuCantinhoDeEstudos3.Models.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using MeuCantinhoDeEstudos3.Models.ClassesDeLog;
using EntityFramework.Extensions;
using EntityFramework.Audit;
using EntityFramework.Triggers;

namespace MeuCantinhoDeEstudos3.Models
{
    public class MeuCantinhoDeEstudosContext : IdentityDbContext<Usuario, Grupo, int, UsuarioLogin, UsuarioGrupo, UsuarioIdentificacao>
    {
        public MeuCantinhoDeEstudosContext()
            : base("DefaultConnection")
        {
        }

        public static MeuCantinhoDeEstudosContext Create()
        {
            return new MeuCantinhoDeEstudosContext();
        }

        public DbSet<Materia> Materias { get; set; }
        public DbSet<MateriaAuditoria> MateriaAuditoria { get; set; }
        public DbSet<Tema> Temas { get; set; }
        public DbSet<TemaAuditoria> TemaAuditoria { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<BateriaExercicio> BateriasExercicios { get; set; }
        public DbSet<VideoAula> VideoAulas { get; set; }
        public DbSet<UsuarioInformacoes> UsuarioInformacoes { get; set; }
        public DbSet<UsuarioLog> UsuarioLogs { get; set; }
        public DbSet<UsuarioLogValores> UsuarioLogValores { get; set; }
        public DbSet<UsuarioInformacoesLog> UsuarioInformacoesLogs { get; set; }
        public DbSet<UsuarioInformacoesLogValores> UsuarioInformacoesLogValores { get; set; }

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
                foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity != null &&
                    // IsAssignableToGenericType(e.Entity.GetType(), typeof(IEntidade<>))))
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
                {
                    ApplyCreationAndModificationProperts(entry);
                }

                var audit = this.BeginAudit();

                this.SaveChangesWithTriggers(base.SaveChanges);
                
                ApplyAuditInUsuarioEntity(this, audit);
                ApplyAuditInUsuarioInformacoesEntity(this, audit);
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

            foreach (var entidade in ChangeTracker.Entries().Where(e => e.Entity != null &&
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

                this.Set(registroTabelaAuditoria.GetType()).Add(classeAuditoria.Target);
            }

            return this.SaveChangesWithTriggers(base.SaveChanges);
        }

        public override async Task<int> SaveChangesAsync()
        {
            try
            {
                var currentTime = DateTime.Now;
                foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity != null &&
                    //IsAssignableToGenericType(e.Entity.GetType(), typeof(IEntidade))))
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
                {
                    ApplyCreationAndModificationProperts(entry);
                }

                var audit = this.BeginAudit();

                await this.SaveChangesWithTriggersAsync(base.SaveChangesAsync);

                await ApplyAuditInUsuarioEntityAsync(this, audit);

                await ApplyAuditInUsuarioInformacoesEntityAsync(this, audit);
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

            foreach (var entidade in ChangeTracker.Entries().Where(e => e.Entity != null &&
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

                this.Set(registroTabelaAuditoria.GetType()).Add(classeAuditoria.Target);
            }

            return await this.SaveChangesWithTriggersAsync(base.SaveChangesAsync);
        }

        private static void ApplyCreationAndModificationProperts(DbEntityEntry entry)
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

        private static void ApplyAuditInUsuarioEntity(MeuCantinhoDeEstudosContext db, AuditLogger audit)
        {
            List<UsuarioLogValores> auditLogsValores = new List<UsuarioLogValores>();

            foreach (var entry in db.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
            {
                if (entry.Entity.GetType().BaseType.Name == typeof(Usuario).Name ||
                    entry.Entity.GetType().Name == typeof(Usuario).Name)
                {
                    var log = audit.LastLog;

                    foreach (var auditEntry in log.Entities)
                    {
                        UsuarioLog usuarioLog = new UsuarioLog()
                        {
                            UsuarioId = (int)entry.Property("Id").CurrentValue,
                            Action = auditEntry.Action.ToString(),
                            NomeTabela = auditEntry.EntityType.Name.ToString(),
                            Data = log.Date,
                            Valores = new List<UsuarioLogValores>(),
                        };

                        db.UsuarioLogs.Add(usuarioLog);

                        db.SaveChangesWithTriggers(db.SaveChanges);

                        foreach (var propery in auditEntry.Properties)
                        {
                            var usuarioLogValores = new UsuarioLogValores()
                            {
                                UsuarioLogId = usuarioLog.UsuarioLogId,
                                NomePropriedade = propery.Name.ToString(),
                                ValorAntigo = propery.Original != null ? propery.Original.ToString() : null,
                                ValorNovo = propery.Current != null ? propery.Current.ToString() : null,
                            };

                            auditLogsValores.Add(usuarioLogValores);
                        }

                        db.UsuarioLogValores.AddRange(auditLogsValores);
                    }

                    db.SaveChangesWithTriggers(db.SaveChanges);
                }
            }
        }

        private static async Task ApplyAuditInUsuarioEntityAsync(MeuCantinhoDeEstudosContext db, AuditLogger audit)
        {
            //MeuCantinhoDeEstudosContext db = new MeuCantinhoDeEstudosContext();
            List<UsuarioLogValores> auditLogsValores = new List<UsuarioLogValores>();

            foreach (var entry in db.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
            {
                if (entry.Entity.GetType().BaseType.Name == typeof(Usuario).Name ||
                    entry.Entity.GetType().Name == typeof(Usuario).Name)
                {
                    var log = audit.LastLog;

                    foreach (var auditEntry in log.Entities)
                    {
                        UsuarioLog usuarioLog = new UsuarioLog()
                        {
                            UsuarioId = (int)entry.Property("Id").CurrentValue,
                            Action = auditEntry.Action.ToString(),
                            NomeTabela = auditEntry.EntityType.Name.ToString(),
                            Data = log.Date,
                            Valores = new List<UsuarioLogValores>(),
                        };

                        db.UsuarioLogs.Add(usuarioLog);

                        await db.SaveChangesWithTriggersAsync(db.SaveChangesAsync);

                        foreach (var propery in auditEntry.Properties)
                        {
                            var usuarioLogValores = new UsuarioLogValores()
                            {
                                UsuarioLogId = usuarioLog.UsuarioLogId,
                                NomePropriedade = propery.Name.ToString(),
                                ValorAntigo = propery.Original != null ? propery.Original.ToString() : null,
                                ValorNovo = propery.Current != null ? propery.Current.ToString() : null,
                            };

                            auditLogsValores.Add(usuarioLogValores);
                        }

                        db.UsuarioLogValores.AddRange(auditLogsValores);
                    }

                    await db.SaveChangesWithTriggersAsync(db.SaveChangesAsync);
                }
            }
        }

        private static void ApplyAuditInUsuarioInformacoesEntity(MeuCantinhoDeEstudosContext db, AuditLogger audit)
        {
            List<UsuarioInformacoesLogValores> auditLogsValores = new List<UsuarioInformacoesLogValores>();

            foreach (var entry in db.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
            {
                if (entry.Entity.GetType().Name == typeof(UsuarioInformacoes).Name ||
                    entry.Entity.GetType().BaseType.Name == typeof(UsuarioInformacoes).Name)
                {
                    var log = audit.LastLog;

                    foreach (var auditEntry in log.Entities)
                    {
                        UsuarioInformacoesLog usuarioInformacoesLog = new UsuarioInformacoesLog()
                        {
                            UsuarioInformacoesId = (int)entry.Property("UsuarioId").CurrentValue,
                            Action = auditEntry.Action.ToString(),
                            NomeTabela = auditEntry.EntityType.Name.ToString(),
                            Data = log.Date,
                            Valores = new List<UsuarioInformacoesLogValores>(),
                        };

                        db.UsuarioInformacoesLogs.Add(usuarioInformacoesLog);

                        db.SaveChangesWithTriggers(db.SaveChanges);

                        foreach (var property in auditEntry.Properties)
                        {
                            var usuarioInformacoesLogValores = new UsuarioInformacoesLogValores()
                            {
                                UsuarioInformacoesLogId = usuarioInformacoesLog.UsuarioInformacoesLogId,
                                NomePropriedade = property.Name.ToString(),
                                ValorAntigo = property.Original != null ? property.Original.ToString() : null,
                                ValorNovo = property.Current != null ? property.Current.ToString() : null,
                            };

                            auditLogsValores.Add(usuarioInformacoesLogValores);
                        }

                        db.UsuarioInformacoesLogValores.AddRange(auditLogsValores);
                    }

                    db.SaveChangesWithTriggers(db.SaveChanges);
                }
            }
        }

        private static async Task ApplyAuditInUsuarioInformacoesEntityAsync(MeuCantinhoDeEstudosContext db, AuditLogger audit)
        {
            List<UsuarioInformacoesLogValores> auditLogsValores = new List<UsuarioInformacoesLogValores>();

            foreach (var entry in db.ChangeTracker.Entries().Where(e => e.Entity != null &&
                    typeof(IEntidade).IsAssignableFrom(e.Entity.GetType())))
            {
                if (entry.Entity.GetType().Name == typeof(UsuarioInformacoes).Name ||
                    entry.Entity.GetType().BaseType.Name == typeof(UsuarioInformacoes).Name)
                {
                    var log = audit.LastLog;

                    foreach (var auditEntry in log.Entities)
                    {
                        UsuarioInformacoesLog usuarioInformacoesLog = new UsuarioInformacoesLog()
                        {
                            UsuarioInformacoesId = (int)entry.Property("UsuarioId").CurrentValue,
                            Action = auditEntry.Action.ToString(),
                            NomeTabela = auditEntry.EntityType.Name.ToString(),
                            Data = log.Date,
                            Valores = new List<UsuarioInformacoesLogValores>(),
                        };

                        db.UsuarioInformacoesLogs.Add(usuarioInformacoesLog);

                        await db.SaveChangesWithTriggersAsync(db.SaveChangesAsync);

                        foreach (var property in auditEntry.Properties)
                        {
                            var usuarioInformacoesLogValores = new UsuarioInformacoesLogValores()
                            {
                                UsuarioInformacoesLogId = usuarioInformacoesLog.UsuarioInformacoesLogId,
                                NomePropriedade = property.Name.ToString(),
                                ValorAntigo = property.Original != null ? property.Original.ToString() : null,
                                ValorNovo = property.Current != null ? property.Current.ToString() : null,
                            };

                            auditLogsValores.Add(usuarioInformacoesLogValores);
                        }

                        db.UsuarioInformacoesLogValores.AddRange(auditLogsValores);
                    }

                    await db.SaveChangesWithTriggersAsync(db.SaveChangesAsync);
                }
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}