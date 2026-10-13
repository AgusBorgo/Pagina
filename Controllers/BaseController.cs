using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PaginaWeb.Context;
using PaginaWeb.Models;
using PaginaWeb.Models.ViewModel;

namespace PaginaWeb.Controllers
{
    // Controlador base: todos los controladores de la tienda heredan de acá
    // para compartir el acceso a la base y la lógica del carrito.
    public class BaseController : Controller
    {
        protected readonly PaginaDatabaseContext _context;
        private const string CookieCarrito = "Carrito";

        public BaseController(PaginaDatabaseContext context)
        {
            _context = context;
        }

        // Cada vez que se arma una vista, cargamos la cantidad de discos del carrito para el header
        public override ViewResult View(string? viewName, object? model)
        {
            ViewBag.NumeroProductos = GetCarritoCount();
            return base.View(viewName, model);
        }

        protected int GetCarritoCount()
        {
            return LeerCookieCarrito().Sum(i => i.Cantidad);
        }

        private List<ProductoIdAndCantidad> LeerCookieCarrito()
        {
            string? carritoJson = Request.Cookies[CookieCarrito];
            if (string.IsNullOrEmpty(carritoJson))
            {
                return new List<ProductoIdAndCantidad>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<ProductoIdAndCantidad>>(carritoJson)
                       ?? new List<ProductoIdAndCantidad>();
            }
            catch (JsonException)
            {
                // Si la cookie está corrupta, empezamos con un carrito vacío
                return new List<ProductoIdAndCantidad>();
            }
        }

        // Suma un disco al carrito sin superar el stock disponible.
        // Devuelve null si el disco no existe o no tiene stock.
        public async Task<CarritoViewModel?> AgregarProductoAlCarrito(int productoId, int cantidad)
        {
            var producto = await _context.Producto.FindAsync(productoId);
            if (producto == null || !producto.Activo || producto.Stock < 1 || cantidad < 1)
            {
                return null;
            }

            var carrito = await GetCarritoViewModelAsync();
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                item.Cantidad = Math.Min(item.Cantidad + cantidad, producto.Stock);
            }
            else
            {
                carrito.Items.Add(new CarritoItemViewModel
                {
                    ProductoId = productoId,
                    Producto = producto,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = Math.Min(cantidad, producto.Stock)
                });
            }

            carrito.Total = carrito.Items.Sum(i => i.Subtotal);
            await UpdateCarritoViewModelAsync(carrito);
            return carrito;
        }

        public Task UpdateCarritoViewModelAsync(CarritoViewModel carritoViewModel)
        {
            var items = carritoViewModel.Items
                .Where(i => i.Cantidad > 0)
                .Select(i => new ProductoIdAndCantidad { ProductoId = i.ProductoId, Cantidad = i.Cantidad })
                .ToList();

            Response.Cookies.Append(CookieCarrito, JsonConvert.SerializeObject(items), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

            return Task.CompletedTask;
        }

        protected void VaciarCookieCarrito()
        {
            Response.Cookies.Delete(CookieCarrito);
        }

        // Arma el carrito completo a partir de la cookie, con datos actualizados de la base
        public async Task<CarritoViewModel> GetCarritoViewModelAsync()
        {
            var carrito = new CarritoViewModel();

            foreach (var item in LeerCookieCarrito())
            {
                var producto = await _context.Producto.FindAsync(item.ProductoId);
                if (producto != null)
                {
                    carrito.Items.Add(new CarritoItemViewModel
                    {
                        ProductoId = item.ProductoId,
                        Producto = producto,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        Cantidad = item.Cantidad
                    });
                }
            }

            carrito.Total = carrito.Items.Sum(i => i.Subtotal);
            return carrito;
        }

        // Devuelve el id del usuario logueado (0 si no hay sesión)
        protected int ObtenerUsuarioId()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return 0;
            }

            var claim = User.FindFirst("UsuarioId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(claim?.Value, out int usuarioId) ? usuarioId : 0;
        }
    }
}
