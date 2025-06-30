using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;

namespace PaginaWeb.Services
{
    public class CategoriaService : ICategoriaServices
    {
        private readonly PaginaDatabaseContext _context;
       public CategoriaService(PaginaDatabaseContext context)
        {
            _context = context;
        }
        public async Task<List<Categoria>> GetCategorias()
        {
            return await _context.Categoria.ToListAsync();

        }

        public Task<dynamic> GetProductosDestacados()
        {
            throw new NotImplementedException();
        }
    }
}
