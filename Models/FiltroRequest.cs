namespace MeuCantinhoDeEstudos3.Models
{
    public class FiltroRequest
    {
        public string OrdemClassificacao { get; set; }
        public string Search { get; set; }
        public int? NumeroPagina { get; set; }
    }
}
