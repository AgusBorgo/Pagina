using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Data;

namespace PaginaWeb.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : BaseController
    {
        public DashboardController(PaginaDatabaseContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalDiscos = await _context.Producto.CountAsync();
            ViewBag.SinStock = await _context.Producto.CountAsync(p => p.Activo && p.Stock < 1);
            ViewBag.TotalPedidos = await _context.Pedidos.CountAsync();
            ViewBag.TotalClientes = await _context.Clientes.CountAsync(c => c.Rol != null && c.Rol.Nombre == DatosIniciales.RolCliente);
            ViewBag.Facturado = await _context.Pedidos.SumAsync(p => (decimal?)p.Total) ?? 0m;

            var ultimosPedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaPedido)
                .Take(5)
                .ToListAsync();

            return View(ultimosPedidos);
        }
    }
}
