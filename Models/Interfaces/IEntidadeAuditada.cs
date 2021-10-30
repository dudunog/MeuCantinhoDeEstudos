using EntityFramework.Triggers;
using MeuCantinhoDeEstudos3.Models.Interfaces;
using System;
using System.ComponentModel;

namespace MeuCantinhoDeEstudos3.Models
{
    public class EntidadeAuditada<TClasseAuditada> : IEntidade
        where TClasseAuditada : class
    {
        [DisplayName("Criado em")]
        public DateTime DataCriacao { get; set; }

        [DisplayName("Criado por")]
        public string UsuarioCriacao { get; set; }

        [DisplayName("Modificado em")]
        public DateTime? UltimaModificacao { get; set; }

        public string UsuarioModificacao { get; set; }

        static EntidadeAuditada()
        {
            Triggers<EntidadeAuditada<TClasseAuditada>>.Inserting += entry => entry.Entity.DataCriacao = DateTime.Now;
            Triggers<EntidadeAuditada<TClasseAuditada>>.Updating += entry => entry.Entity.UltimaModificacao = DateTime.Now;
        }
    }
}