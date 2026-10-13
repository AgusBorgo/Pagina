namespace PaginaWeb.Models.ViewModel
{
    public class ProcederConCompraViewModel
    {
        public CarritoViewModel Carrito { get; set; } = new CarritoViewModel();
        public string? DireccionEnvio { get; set; }
    }
}
