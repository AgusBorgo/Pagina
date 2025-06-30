namespace PaginaWeb.Models.ProductosPaginadosViewModel
{
    public class ProductosPaginadosViewModel
    {
        public List<Producto> Productos { get; set; } = null;
        public int TotalProductos { get; set; }
        public int PaginaActual { get; set; }
        public int ProductosPorPagina { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalProductos / ProductosPorPagina);
        public int? CategoriaId { get; set; }
        public string? Busqueda { get; set; }
        public bool TieneProductos => Productos.Any();
    }
}
