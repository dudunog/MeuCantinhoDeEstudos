using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MeuCantinhoDeEstudos3.Models
{
    public class UsuarioInformacoesLog
    {
		[Key]
		public int UsuarioInformacoesLogId { get; set; }

		public int UsuarioId { get; set; }

		public string Action { get; set; }

		public string NomeTabela { get; set; }

		public DateTime Data { get; set; }

		public List<UsuarioInformacoesLogValores> Valores { get; set; }

		[ForeignKey(nameof(UsuarioId))]
		public virtual Usuario Usuario { get; set; }
	}
}