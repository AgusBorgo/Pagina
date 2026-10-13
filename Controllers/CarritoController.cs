using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaginaWeb.Context;
using PaginaWeb.Models;
using PaginaWeb.Models.ViewModel;

namespace PaginaWeb.Controllers
{
    public class CarritoController : BaseController
    {
        public CarritoController(PaginaDatabaseContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var carrito = await GetCarritoViewModelAsync();

            // Ajustamos las cantidades al stock actual (puede haber cambiado desde que se agregó)
            foreach (var item in carrito.Items)
            {
                if (item.Producto != null)
                {
                    item.Cantidad = Math.Min(item.Cantidad, item.Producto.Stock);
                }
            }
            carrito.Total = carrito.Items.Sum(i => i.Subtotal);

            return View(new ProcederConCompraViewModel { Carrito = carrito });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCantidad(int productoId, int cantidad)
        {
            var carrito = await GetCarritoViewModelAsync();
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item?.Producto != null)
            {
                item.Cantidad = Math.Clamp(cantidad, 1, Math.Max(1, item.Producto.Stock));
                await UpdateCarritoViewModelAsync(carrito);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProducto(int productoId)
        {
            var carrito = await GetCarritoViewModelAsync();
            carrito.Items.RemoveAll(i => i.ProductoId == productoId);
            await UpdateCarritoViewModelAsync(carrito);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VaciarCarrito()
        {
            VaciarCookieCarrito();
            return RedirectToAction(nameof(Index));
        }

        // Para comprar hay que tener sesión: si no, [Authorize] manda al login y después vuelve acá
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCompra()
        {
            int usuarioId = ObtenerUsuarioId();
            var carrito = await GetCarritoViewModelAsync();

            if (carrito.Items.Count == 0)
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                FechaPedido = DateTime.Now,
                Estado = "Finalizado",
                FechaFinalizacion = DateTime.Now
            };

            foreach (var item in carrito.Items)
            {
                var producto = item.Producto;
                if (producto == null || !producto.Activo)
                {
                    TempData["Error"] = $"El disco \"{item.Nombre}\" ya no está disponible. Quitalo del carrito para continuar.";
                    return RedirectToAction(nameof(Index));
                }

                if (producto.Stock < item.Cantidad)
                {
                    TempData["Error"] = $"No hay stock suficiente de \"{producto.Nombre}\" (quedan {producto.Stock}).";
                    return RedirectToAction(nameof(Index));
                }

                producto.Stock -= item.Cantidad;

                pedido.PedidoDetalles.Add(new PedidoDetalle
                {
                    ProductoId = producto.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio
                });
                pedido.Total += producto.Precio * item.Cantidad;
            }

            // El pedido y la baja de stock se guardan juntos en un solo SaveChanges
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            VaciarCookieCarrito();
            TempData["Mensaje"] = $"¡Listo! Tu pedido #{pedido.PedidoId} quedó confirmado.";
            return RedirectToAction("DetallePedido", "Account", new { id = pedido.PedidoId });
        }
    }
}
