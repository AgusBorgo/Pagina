using PaginaWeb.Models;

namespace PaginaWeb.Services
{
    public interface ICategoriaServices
    {
        Task<List<Categoria>> GetCategorias();
    }
}
