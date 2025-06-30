using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaginaWeb.Context;
using PaginaWeb.Models;
using PaginaWeb.Models.ViewModel;

namespace PaginaWeb.Controllers
{
    public class BaseController : Controller
    {
        protected readonly PaginaDatabaseContext _context;

        public BaseController(PaginaDatabaseContext context)
        {
            _context = context;
        }

        public override ViewResult View(string? viewName, object? model)
        {
            ViewBag.NumeroProductos = GetCarritoCount();
            return base.View(viewName, model);
        }

        protected int GetCarritoCount()
        {
            int count = 0;
            string? carritoJson = Request.Cookies["Carrito"];

            if (!string.IsNullOrEmpty(carritoJson))
            {
                var carrito = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);
                if (carrito != null)
                {
                    count = carrito.Count;
                }
            }

            return count;
        }

        public async Task<CarritoViewModel> AgregarProductoAlCarrito(int productoId, int cantidad)
        {
            var producto = await _context.Producto.FindAsync(productoId);

            if (producto != null)
            {
                var carritoViewModel = await GetCarritoViewModelAsync();
                var carritoItem = carritoViewModel.Items.FirstOrDefault(i => i.ProductoId == productoId);

                if (carritoItem != null)
                {
                    carritoItem.Cantidad += cantidad;
                }
                else
                {
                    carritoViewModel.Items.Add(new CarritoItemViewModel
                    {
                        ProductoId = productoId,
                        Cantidad = cantidad,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio
                    });
                }

                carritoViewModel.Total = carritoViewModel.Items.Sum(i => i.Precio * i.Cantidad);
                await UpdateCarritoViewModelAsync(carritoViewModel);
                return carritoViewModel;
            }

            // Producto no encontrado
            return new CarritoViewModel
            {
                Items = new List<CarritoItemViewModel>(),
                Total = 0
            };
        }

        public async Task UpdateCarritoViewModelAsync(CarritoViewModel carritoViewModel)
        {
            var productoIds = carritoViewModel.Items.Select(item => new ProductoIdAndCantidad
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad
            }).ToList();

            var carritoJson = await Task.Run(() => JsonConvert.SerializeObject(productoIds));

            Response.Cookies.Append("Carrito", carritoJson, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7)
            });
        }

        public async Task<CarritoViewModel> GetCarritoViewModelAsync()
        {
            var carritoJson = Request.Cookies["Carrito"];

            if (string.IsNullOrEmpty(carritoJson))
            {
                return new CarritoViewModel();
            }

            var productoIdsAndCantidades = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);
            var carritoViewModel = new CarritoViewModel();

            if (productoIdsAndCantidades != null)
            {
                foreach (var item in productoIdsAndCantidades)
                {
                    var producto = await _context.Producto.FindAsync(item.ProductoId);
                    if (producto != null)
                    {
                        carritoViewModel.Items.Add(new CarritoItemViewModel
                        {
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            Nombre = producto.Nombre,
                            Precio = producto.Precio
                        });
                    }
                }
            }

            carritoViewModel.Total = carritoViewModel.Items.Sum(i => i.Precio * i.Cantidad);
            return carritoViewModel;
        }
    }
}

