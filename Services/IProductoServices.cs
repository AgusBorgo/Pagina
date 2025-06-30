using PaginaWeb.Models;
using PaginaWeb.Models.ProductosPaginadosViewModel;

namespace PaginaWeb.Services
{
    public interface IProductoServices
    {
        Producto GetProducto(int id);

        Task<Producto> GetProductosDestacados();

        Task<ProductosPaginadosViewModel> GetProductosPaginados(int? CategoriaId, string? busqueda, int pagina, int
            productosPorPagina);
    }
}
