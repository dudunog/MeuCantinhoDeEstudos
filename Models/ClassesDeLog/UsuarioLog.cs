using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeuCantinhoDeEstudos3.Models.ClassesDeLog
{
    public class UsuarioLog
	{
		[Key]
		public int UsuarioLogId { get; set; }

		public int UsuarioId { get; set; }

		public string Action { get; set; }

		public string NomeTabela { get; set; }

		public DateTime Data { get; set; }

		public List<UsuarioLogValores> Valores { get; set; }

		[ForeignKey(nameof(UsuarioId))]
		public virtual Usuario Usuario { get; set; }
    }
}