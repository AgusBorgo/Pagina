using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaginaWeb.Context;
using PaginaWeb.Models;
using PaginaWeb.Models.ViewModel;
using PaginaWeb.Services;

namespace PaginaWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductoServices _productoService;
        private readonly ICategoriaServices _categoriaService;

        public HomeController(
            ILogger<HomeController> logger,
            PaginaDatabaseContext context,
            IProductoServices productoServices,
            ICategoriaServices categoriaServices
        ) : base(context)
        {
            _logger = logger;
            _productoService = productoServices;
            _categoriaService = categoriaServices;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.Categorias = await _categoriaService.GetCategorias();
                var productosDestacados = await _categoriaService.GetProductosDestacados();
                return View(productosDestacados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el método Index");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public IActionResult DetallesProducto(int id)
        {
            var producto = _productoService.GetProducto(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        public async Task<IActionResult> Productos(int? categoriaId, string? busqueda, int pagina = 1)
        {
            try
            {
                int productosPorPagina = 9;
                var model = await _productoService.GetProductosPaginados(categoriaId, busqueda, pagina, productosPorPagina);
                ViewBag.Categorias = await _categoriaService.GetCategorias();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ProductosPartial", model);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el método Productos");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public async Task<IActionResult> AgregarProducto(int id, int cantidad, int? categoriaId, string? busqueda, int pagina = 1)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel != null)
            {
                return RedirectToAction("Productos", new { categoriaId, busqueda, pagina });
            }
            return NotFound();
        }

        public async Task<IActionResult> AgregarProductoIndex(int id, int cantidad)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel != null)
            {
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public async Task<IActionResult> AgregarProductoDetalle(int id, int cantidad)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel != null)
            {
                return RedirectToAction("DetallesProducto", new { id });
            }
            return NotFound();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        
        private async Task<CarritoViewModel> AgregarProductoAlCarrito(int productoId, int nuevaCantidad)
        {
            var carrito = await GetCarritoViewModelAsync();

            var itemExistente = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);

            var producto = await _context.Producto.FindAsync(productoId);
            if (producto == null || nuevaCantidad <= 0)
                return null;

            if (itemExistente != null)
            { 
                itemExistente.Cantidad = Math.Min(nuevaCantidad, producto.Stock);
            }
            else
            {
                carrito.Items.Add(new CarritoItemViewModel
                {
                    ProductoId = productoId,
                    Cantidad = Math.Min(nuevaCantidad, producto.Stock),
                    Precio = producto.Precio,
                    Producto = producto
                });
            }

            await UpdateCarritoViewModelAsync(carrito);
            return carrito;
        }
    }
}

