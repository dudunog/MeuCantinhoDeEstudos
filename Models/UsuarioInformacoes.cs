using System;
using EntityFramework.Triggers;
using MeuCantinhoDeEstudos3.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models
{
    public class UsuarioInformacoes : IEntidade
    {
        [Key, ForeignKey(nameof(Usuario))]
        public int UsuarioId { get; set; }

        public string Pais { get; set; }

        public string Estado { get; set; }

        public string Cidade { get; set; }

        public string CEP { get; set; }

        public string Logradouro { get; set; }

        public string Numero { get; set; }

        public virtual Usuario Usuario { get; set; }

        public DateTime DataCriacao { get; set; }

        public string UsuarioCriacao { get; set; }

        public DateTime? UltimaModificacao { get; set; }

        public string UsuarioModificacao { get; set; }

        static UsuarioInformacoes()
        {
            Triggers<UsuarioInformacoes>.Inserting += entry => entry.Entity.DataCriacao = DateTime.Now;
            Triggers<UsuarioInformacoes>.Updating += entry => entry.Entity.UltimaModificacao = DateTime.Now;
        }
    }
}