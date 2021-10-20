using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace MeuCantinhoDeEstudos3.Models
{
    public class PaginatedList<T> : List<T>
    {
        public int IndicePagina { get; private set; }

        public int TotalPaginas { get; private set; }

        public int QuantidadeElementos { get; private set; }

        public IEnumerable<T> Items { get; private set; }

        public PaginatedList(List<T> items, int quantidade, int indicePagina, int tamanhoPagina)
        {
            IndicePagina = indicePagina;
            TotalPaginas = (int)Math.Ceiling(quantidade / (double)tamanhoPagina);
            QuantidadeElementos = quantidade;
            //Items = items;
            this.AddRange(items);
        }

        public bool TemPaginaAnterior
        {
            get
            {
                return (IndicePagina > 1);
            }
        }

        public bool TemProximaPagina
        {
            get
            {
                return (IndicePagina < TotalPaginas);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int indicePagina, int tamanhoPagina)
        {
            var quantidade = await source.CountAsync();
            var items = await source.Skip((indicePagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();

            return new PaginatedList<T>(items, quantidade, indicePagina, tamanhoPagina);
        }
    }
}