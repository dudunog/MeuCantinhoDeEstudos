using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeuCantinhoDeEstudos3.Models.Interfaces
{
    public interface IEntidade
    {
        DateTime DataCriacao { get; set; }
        String UsuarioCriacao { get; set; }
        DateTime? UltimaModificacao { get; set; }
        String UsuarioModificacao { get; set; }
    }
}