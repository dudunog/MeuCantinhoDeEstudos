using MeuCantinhoDeEstudos3.Models.Interfaces;

namespace MeuCantinhoDeEstudos3.Models
{
    public interface IEntidadeAuditada<TClasseAuditada> : IEntidade
        where TClasseAuditada : class
    {
    }
}