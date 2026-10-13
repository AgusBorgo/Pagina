using PaginaWeb.Models;
using PaginaWeb.Models.ProductosPaginadosViewModel;

namespace PaginaWeb.Services
{
    public interface IProductoServices
    {
        Task<Producto?> GetProducto(int id);

        Task<List<Producto>> GetProductosDestacados(int cantidad);

        Task<List<Producto>> GetRelacionados(Producto producto, int cantidad);

        Task<ProductosPaginadosViewModel> GetProductosPaginados(int? categoriaId, string? busqueda, int pagina, int productosPorPagina);
    }
}
