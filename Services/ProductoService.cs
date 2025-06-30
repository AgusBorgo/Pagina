using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;
using PaginaWeb.Models.ProductosPaginadosViewModel;

namespace PaginaWeb.Services
{
    public class ProductoService : IProductoServices
    {
        private readonly PaginaDatabaseContext _context;
        public ProductoService(PaginaDatabaseContext context)
        {
            _context = context;
        }
        public Producto GetProducto(int id)
        {
            var producto = _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.ProductoId == id);
            if (producto != null)
                return producto;

            return new Producto();
        }
        public Task<Producto> GetProductosDestacados()
        {
            IQueryable<Producto> query = _context.Producto
                .Include(p => p.Categoria)
                .Where(p => p.Activo == true)
                .Take(4);

            return query.FirstOrDefaultAsync();
        }
        public async Task<ProductosPaginadosViewModel> GetProductosPaginados(int? CategoriaId, string? busqueda, int pagina, int productosPorPagina)
        {
            IQueryable<Producto> query = _context.Producto
                .Include(p => p.Categoria)
                .Where(p => p.Activo == true);

            if (CategoriaId.HasValue && CategoriaId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == CategoriaId.Value);
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(busqueda) || p.Descripcion.Contains(busqueda));
            }

            List<Producto> products = new List<Producto>();

            if (productosPorPagina > 0)
            {
                products = await query
                    .OrderBy(p => p.Nombre)
                    .Skip((pagina - 1) * productosPorPagina)
                    .Take(productosPorPagina)
                    .ToListAsync();
            }
            else
            {
                products = await query.ToListAsync();
            }

            var model = new ProductosPaginadosViewModel
            {
                Productos = products,
                PaginaActual = pagina,
                ProductosPorPagina = productosPorPagina,
                TotalProductos = await query.CountAsync(),
                CategoriaId = CategoriaId,
                Busqueda = busqueda
            };

            return model;
        }

    }
}

