namespace PaginaWeb.Models
{
    public class ProductoIdAndCantidad 
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string Nombre { get; internal set; }
        public decimal Precio { get; internal set; }
    }
}
