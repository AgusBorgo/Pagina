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
        public readonly PaginaDatabaseContext _context;

        public BaseController(PaginaDatabaseContext context)
        {
            _context = context;
        }

        public override ViewResult View(string? viewName, object? model)
        {
            @ViewBag.NumeroProductos = GetCarritoCount();
            return base.View(viewName, model);
        }

        protected int GetCarritoCount()
        {
            var count = 0;

            string? carritoJson = Request.Cookies["Carrito"];
            if (carritoJson != null)
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
                carritoViewModel.Total += producto.Precio * cantidad;
                await UpdateCarritoViewModelAsync(carritoViewModel);
                return carritoViewModel;
            }

            // Return a default value if the product is not found  
            return new CarritoViewModel
            {
                Items = new List<CarritoItemViewModel>(),
                Total = 0
            };
        }

        public async Task UpdateCarritoViewModelAsync(CarritoViewModel carritoViewModel)
        {
            var productoIds = carritoViewModel.Items.Select(
                item => new ProductoIdAndCantidad
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad
                }
            ).ToList();

            // Fixing CS1501, CS1003, and CS1002 by correcting the syntax for Task.Run and lambda expression  
            var carritoJson = await Task.Run(() => JsonConvert.SerializeObject(productoIds));

            // Assuming the serialized JSON needs to be stored in a cookie  
            Response.Cookies.Append("Carrito", carritoJson, new CookieOptions { 
            Expires = DateTime.Now.AddDays(7)
            });
        }

        private async Task<CarritoViewModel> GetCarritoViewModelAsync()
        {
            var carritoJson = Request.Cookies["Carrito"];
            
            if(string.IsNullOrEmpty(carritoJson))
            
                return new CarritoViewModel();
            var productoIdsAndCantidades = JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson);
            var CarritoViewModel = new CarritoViewModel();

            if(productoIdsAndCantidades != null)
            {
                foreach (var item in productoIdsAndCantidades)
                {
                    var producto = await _context.Producto.FindAsync(item.ProductoId);
                    if (producto != null)
                    {
                        CarritoViewModel.Items.Add(new CarritoItemViewModel
                        {
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            Nombre = producto.Nombre,
                            Precio = producto.Precio
                        });
                    }
                }
            }
            CarritoItemViewModel.Total=CarritoViewModel.Items.Sum(i => i.Precio * i.Cantidad);
            return CarritoViewModel;
        }
    }

}
