using Root.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeuCantinhoDeEstudos3.Models.PDF
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ColunaRelatorioAttribute : Attribute
    {
        public Int16 Largura { get; set; }
        public Int16 Posicao { get; set; }
        public string Titulo { get; set; }
        public TlmTextMode TextMode { get; set; } = TlmTextMode.MultiLine;
    }
}