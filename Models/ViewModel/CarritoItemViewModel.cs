namespace PaginaWeb.Models.ViewModel
{
    public class CarritoItemViewModel
    {
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal Subtotal => Precio * Cantidad;
    }
}
