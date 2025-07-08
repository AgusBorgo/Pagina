using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaginaWeb.Context;
using System.Threading.Tasks;
using System.Linq;
using System;
using PaginaWeb.Models;

namespace PaginaWeb.Controllers
{
    public class CarritoController : BaseController
    {
        public CarritoController(PaginaDatabaseContext context) : base(context) { }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var carritoViewModel = await GetCarritoViewModelAsync();

            foreach (var item in carritoViewModel.Items)
            {
                var producto = await _context.Producto.FindAsync(item.ProductoId);
                if (producto != null)
                {
                    item.Producto = producto;

                    if (producto.Stock == 0)
                    {
                        item.Cantidad = 0;
                    }
                    else
                    {
                        item.Cantidad = Math.Min(item.Cantidad, producto.Stock);
                    }
                }
            }

            var procederConCompraViewModel = new Models.ViewModel.ProcederConCompraViewModel
            {
                Carrito = carritoViewModel,
            };

            return View(procederConCompraViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(int productoId, int cantidad)
        {
            var carritoViewModel = await GetCarritoViewModelAsync();
            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (carritoItem != null)
            {
                var producto = await _context.Producto.FindAsync(productoId);
                if (producto != null)
                {
                    carritoItem.Cantidad = Math.Min(cantidad, producto.Stock);
                }

                await UpdateCarritoViewModelAsync(carritoViewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarProducto(int productoId)
        {
            var carritoViewModel = await GetCarritoViewModelAsync();
            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (carritoItem != null)
            {
                carritoViewModel.Items.Remove(carritoItem);
                await UpdateCarritoViewModelAsync(carritoViewModel);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> VaciarCarrito()
        {
            await RemoveCarritoViewModelAsync();
            return RedirectToAction("Index");
        }

        private Task RemoveCarritoViewModelAsync()
        {
            Response.Cookies.Delete("Carrito");
            return Task.CompletedTask;
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarCompra()
        {
            

            var carritoViewModel = await GetCarritoViewModelAsync();
            System.Diagnostics.Debug.WriteLine("Ítems en carrito: " + carritoViewModel.Items.Count);

            if (carritoViewModel.Items.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("El carrito está vacío.");
                return BadRequest("Tu carrito está vacío.");
            }

            var pedido = new Pedido
            {
               // UsuarioId = ObtenerUsuarioId(),
                FechaPedido = DateTime.Now,
                Estado = "Pendiente",
                Total = 0m,
                PedidoDetalles = new List<PedidoDetalle>()
            };

            foreach (var item in carritoViewModel.Items)
            {
                System.Diagnostics.Debug.WriteLine( "Revisando producto con ID: " + item.ProductoId);

                var producto = await _context.Producto.FindAsync(item.ProductoId);
                if (producto == null)
                {
                    System.Diagnostics.Debug.WriteLine("Producto no encontrado.");
                    return NotFound($"Producto con ID {item.ProductoId} no encontrado.");
                }

                if (producto.Stock < item.Cantidad)
                {
                    System.Diagnostics.Debug.WriteLine(" Stock insuficiente para " + producto.Nombre);
                    return BadRequest("No hay stock suficiente para {producto.Nombre}");
                }

                System.Diagnostics.Debug.WriteLine($"Producto OK: {producto.Nombre}, Cantidad: {item.Cantidad}");

                producto.Stock -= item.Cantidad;
                _context.Producto.Update(producto);

                var detalle = new PedidoDetalle
                {
                    ProductoId = producto.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio
                };

                pedido.PedidoDetalles.Add(detalle);
                pedido.Total += producto.Precio * item.Cantidad;
            }

            System.Diagnostics.Debug.WriteLine("Guardando pedido. Total: " + pedido.Total);

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            await RemoveCarritoViewModelAsync();

            TempData["mensaje"] = "Confirmación de compra completada.";
            System.Diagnostics.Debug.WriteLine("Compra confirmada");

            return RedirectToAction("Index");
        }

        private int ObtenerUsuarioId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (claim != null)
                {
                    return int.Parse(claim.Value);
                }
            }

            return 0; // Usuario no autenticado
        }
    }
}
