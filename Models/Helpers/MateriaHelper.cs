using FileHelpers;

namespace MeuCantinhoDeEstudos3.Models.Helpers
{
    [DelimitedRecord(";")]
    public class MateriaHelper
    {
        [FieldOrder(0)]
        public string Nome { get; set; }

        [FieldOrder(1)]
        public string CorIdentificacao { get; set; }
    }
}