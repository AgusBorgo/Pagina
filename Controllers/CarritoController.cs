using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaginaWeb.Context;
using System.Threading.Tasks;
using System.Linq;
using System;
using PaginaWeb.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmarCompra()
        {

            int usuarioId = ObtenerUsuarioId();

            // También intentamos obtener el ID del claim, si existe
            var userIdClaim = User.FindFirst("UsuarioId")?.Value;


            
            if (usuarioId <= 0)
            {
                return BadRequest("Error: Usuario no encontrado en la base de datos.");
            }

            var carritoViewModel = await GetCarritoViewModelAsync();

            if (carritoViewModel.Items.Count == 0)
            {
                return BadRequest("Tu carrito está vacío.");
            }

            var pedido = new Pedido
            {
                UsuarioId = usuarioId, 
                FechaPedido = DateTime.Now,
                Estado = "Pendiente",
                Total = 0m,
                PedidoDetalles = new List<PedidoDetalle>()
            };

            

            foreach (var item in carritoViewModel.Items)
            {

                var producto = await _context.Producto.FindAsync(item.ProductoId);
                if (producto == null) { 
                    return NotFound($"Producto con ID {item.ProductoId} no encontrado.");
                }

                if (producto.Stock < item.Cantidad)
                {
                    return BadRequest($"No hay stock suficiente para {producto.Nombre}");
                }

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


            try
            {
                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                var pedidoGuardado = await _context.Pedidos.FindAsync(pedido.PedidoId);
               
                bool finalizadoOk = await FinalizarPedido(pedido.PedidoId);

                await RemoveCarritoViewModelAsync();

                TempData["mensaje"] = "Confirmación de compra completada.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar la compra: {ex.Message}");
            }
        }

        private async Task<bool> FinalizarPedido(int pedidoId)
        {
            try
            {
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Pedido {pedidoId} no encontrado para finalizar");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"🔍 Finalizando pedido {pedidoId}, Estado actual: {pedido.Estado}");

                pedido.Estado = "Finalizado";
                pedido.FechaFinalizacion = DateTime.Now;

                _context.Pedidos.Update(pedido);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Pedido {pedidoId} finalizado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al finalizar pedido: {ex.Message}");
                return false;
            }
        }
    }
}