using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaginaWeb.Context;

namespace PaginaWeb.Controllers
{
    public class CarritoController : BaseController
    {
        public CarritoController(PaginaDatabaseContext context) : base(context)
        {
        }

        [AllowAnonymous] // permite acceso sin autenticación  
        public async Task<IActionResult> Index()
        {
            var carritoViewModel = await GetCarritoViewModelAsync();

            foreach (var item in carritoViewModel.Items)
            {
                var producto = await _context.Producto.FindAsync(item.ProductoId);
                if (producto != null)
                {
                    item.Producto = producto;

                    item.Cantidad = Math.Min(item.Cantidad, producto.Stock);

                    if (item.Cantidad == 0)
                    {
                        item.Cantidad = 1; // Asegura que la cantidad sea al menos 1  
                    }
                    else
                    {
                        item.Cantidad = 0; // Si el stock es 0, establece la cantidad a 0  
                    }

                    var procederConCompraViewModel = new Models.ViewModel.ProcederConCompraViewModel
                    {
                        CarritoController = carritoViewModel,
                    };
                    return View(procederConCompraViewModel);
                }
            }

            // Agregar un retorno predeterminado para evitar el error CS0161  
            return View(carritoViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarContenido(int productoId, int cantidad)
        {
            var carritoViewModel = await GetCarritoViewModelAsync();
            var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (carritoItem != null)
            {
                var producto = await _context.Producto.FindAsync(productoId);
                if (producto != null)
                    carritoItem.Cantidad = Math.Min(cantidad, producto.Stock);
                await UpdateCarritoViewModelAsync(carritoViewModel);
            }
            return RedirectToAction("Index", "Carrito");
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
