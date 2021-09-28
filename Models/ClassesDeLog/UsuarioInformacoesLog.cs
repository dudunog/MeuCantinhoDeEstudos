using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models.ClassesDeLog
{
    public class UsuarioInformacoesLog
    {
		[Key]
		public int UsuarioInformacoesLogId { get; set; }

		public int UsuarioInformacoesId { get; set; }

		public string Action { get; set; }

		public string NomeTabela { get; set; }

		public DateTime Data { get; set; }

		public List<UsuarioInformacoesLogValores> Valores { get; set; }

		[ForeignKey(nameof(UsuarioInformacoesId))]
		public virtual UsuarioInformacoes UsuarioInformacoes { get; set; }
	}
}