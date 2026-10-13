using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;

namespace PaginaWeb.Controllers
{
    // Consulta de pedidos para el administrador (los pedidos se crean desde el carrito)
    [Authorize(Policy = "AdminOnly")]
    public class PedidoesController : BaseController
    {
        public PedidoesController(PaginaDatabaseContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.PedidoDetalles)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
            return View(pedidos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.PedidoId == id);

            return pedido == null ? NotFound() : View(pedido);
        }
    }
}
