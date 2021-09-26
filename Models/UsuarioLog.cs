using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MeuCantinhoDeEstudos3.Models
{
    public class UsuarioLog
    {
		[Key]
		public int UsuarioLogId { get; set; }

		public int UsuarioId { get; set; }

		public string Action { get; set; }

		public string NomeTabela { get; set; }

		public DateTime Date { get; set; }

		public List<UsuarioLogValue> Values { get; set; }

		[ForeignKey(nameof(UsuarioId))]
		public virtual Usuario Usuario { get; set; }
	}
}