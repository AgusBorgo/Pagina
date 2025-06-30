namespace PaginaWeb.Models.ViewModel
{
    public class CarritoItemViewModel
    {
        public static decimal Total { get; internal set; }
        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; } = decimal.Zero;
        public int Cantidad { get; set; } = 1;
        public decimal Subtotal => Precio * Cantidad;
    }
}
