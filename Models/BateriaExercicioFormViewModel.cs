namespace MeuCantinhoDeEstudos3.Models
{
    public class BateriaExercicioFormViewModel
    {
        public string ordemClassificacao { get; set; }

        public string filtroAtual { get; set; }

        public string search { get; set; }

        public int? numeroPagina { get; set; }

        public int? NumeroPagina
        {
            get
            {
                if (search != null)
                {
                    numeroPagina = 1;
                }

                return numeroPagina;
            }
            set
            {
                numeroPagina = value;
            }
        }

        public string FiltroAtual
        {
            get
            {
                return !string.IsNullOrEmpty(search) ? search : filtroAtual;
            }
        }
    }
}