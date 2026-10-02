using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaginaWeb.Context;

namespace PaginaWeb.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class Dashboard : BaseController
    {
        public Dashboard(PaginaDatabaseContext context) : base(context)
        {
        }

        public IActionResult Index()
        {
            // Aquí puedes agregar la lógica para obtener datos del dashboard
            // Por ejemplo, contar el número de productos, pedidos, etc.
            var totalProductos = _context.Producto.Count();
            var totalPedidos = _context.Pedidos.Count();
            var totalClientes = _context.Clientes.Count();
            ViewBag.TotalProductos = totalProductos;
            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.TotalClientes = totalClientes;
            return View();
        }
    }
}
