namespace PaginaWeb.Models
{
    // Lo que se guarda en la cookie del carrito: solo el id y la cantidad
    public class ProductoIdAndCantidad
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}
