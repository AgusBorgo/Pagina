using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaginaWeb.Context;
using System.Threading.Tasks;
using System.Linq;
using System;

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

        private async Task RemoveCarritoViewModelAsync()
        {
            await Task.Run(() => Response.Cookies.Delete("Carrito"));
        }
    }
}
