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

        // Solo los discos visibles en la tienda
        private IQueryable<Producto> Activos() =>
            _context.Producto.Include(p => p.Categoria).Where(p => p.Activo);

        public async Task<Producto?> GetProducto(int id)
        {
            return await Activos().FirstOrDefaultAsync(p => p.ProductoId == id);
        }

        // Destacados: primero los que tienen stock, los más nuevos arriba
        public async Task<List<Producto>> GetProductosDestacados(int cantidad)
        {
            return await Activos()
                .OrderByDescending(p => p.Stock > 0)
                .ThenByDescending(p => p.ProductoId)
                .Take(cantidad)
                .ToListAsync();
        }

        // Otros discos del mismo artista o, si no hay, del mismo género
        public async Task<List<Producto>> GetRelacionados(Producto producto, int cantidad)
        {
            return await Activos()
                .Where(p => p.ProductoId != producto.ProductoId &&
                            (p.Artista == producto.Artista || p.CategoriaId == producto.CategoriaId))
                .OrderByDescending(p => p.Artista == producto.Artista)
                .ThenBy(p => p.Anio)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<ProductosPaginadosViewModel> GetProductosPaginados(int? categoriaId, string? busqueda, int pagina, int productosPorPagina)
        {
            IQueryable<Producto> query = Activos();

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                query = query.Where(p => p.Nombre.Contains(texto) ||
                                         p.Artista.Contains(texto) ||
                                         p.Descripcion.Contains(texto));
            }

            int total = await query.CountAsync();
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)total / productosPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var productos = await query
                .OrderBy(p => p.Artista)
                .ThenBy(p => p.Anio)
                .ThenBy(p => p.Nombre)
                .Skip((pagina - 1) * productosPorPagina)
                .Take(productosPorPagina)
                .ToListAsync();

            return new ProductosPaginadosViewModel
            {
                Productos = productos,
                TotalProductos = total,
                PaginaActual = pagina,
                ProductosPorPagina = productosPorPagina,
                CategoriaId = categoriaId,
                Busqueda = busqueda
            };
        }
    }
}
