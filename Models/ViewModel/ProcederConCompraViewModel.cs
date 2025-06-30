namespace PaginaWeb.Models.ViewModel
{
    public class ProcederConCompraViewModel
    {
        public CarritoViewModel Carrito { get; set; }
        public string? DireccionEnvio { get; set; }
        public CarritoViewModel CarritoController { get; internal set; }
    }
}
