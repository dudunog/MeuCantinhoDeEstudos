using EntityFramework.Triggers;
using MeuCantinhoDeEstudos3.Models.Interfaces;
using System;
using System.ComponentModel;

namespace MeuCantinhoDeEstudos3.Models
{
    public abstract class Entidade : IEntidade
    {
        [DisplayName("Criado em")]
        public DateTime DataCriacao { get; set; }
        [DisplayName("Criado por")]
        public string UsuarioCriacao { get; set; }
        [DisplayName("Modificado em")]
        public DateTime? UltimaModificacao { get; set; }
        public string UsuarioModificacao { get; set; }

        static Entidade()
        {
            Triggers<Entidade>.Inserting += entry => entry.Entity.DataCriacao = DateTime.Now;
            Triggers<Entidade>.Updating += entry => entry.Entity.UltimaModificacao = DateTime.Now;
        }
    }
}