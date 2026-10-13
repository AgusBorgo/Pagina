namespace PaginaWeb.Models.ProductosPaginadosViewModel
{
    public class ProductosPaginadosViewModel
    {
        public List<Producto> Productos { get; set; } = new List<Producto>();
        public int TotalProductos { get; set; }
        public int PaginaActual { get; set; }
        public int ProductosPorPagina { get; set; }
        public int TotalPaginas => ProductosPorPagina > 0
            ? (int)Math.Ceiling((double)TotalProductos / ProductosPorPagina)
            : 1;
        public int? CategoriaId { get; set; }
        public string? Busqueda { get; set; }
        public bool TieneProductos => Productos.Any();
    }
}
