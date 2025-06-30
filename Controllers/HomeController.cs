using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PaginaWeb.Context;
using PaginaWeb.Models;
using PaginaWeb.Services;

namespace PaginaWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductoServices _productoService;
        private readonly ICategoriaServices _categoriaService;
        private readonly string? productosDestacados;

        public HomeController(ILogger<HomeController> logger, PaginaDatabaseContext context, IProductoServices productoServices, ICategoriaServices categoriaServices) : base(context)
        {
            _logger = logger;
            _productoService = productoServices;
            _categoriaService = categoriaServices;
        }

        public async Task<IActionResult> Index()
        {   // Para los productos destacados
            ViewBag.Categorias = await _categoriaService.GetCategorias();
            try
            {
                List<Producto> productosDestacados = await _categoriaService.GetProductosDestacados();
                return View(productosDestacados);
               

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el método Index");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // obtener los detalles de un producto seleccionado

        public IActionResult DetallesProducto(int id)
        {
            var producto = _productoService.GetProducto(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        //mostrar productos y filtrar productos
        public async Task<IActionResult> Productos(
            int? categoriaId,
            string? busqueda,
            int pagina = 1
         )
        {
            try
            {
                int productosPorPagina = 9;
                var model = await _productoService.GetProductosPaginados(categoriaId, busqueda, pagina, productosPorPagina);
                // ayuda a mostrar las categorias a mi vista
                ViewBag.Categorias = await _categoriaService.GetCategorias();
                //para evitar recargar la pagina todo el tiempo
                // no sobre cargo vistas
                if (Request.Headers["X-Request-With"] == "XMLHttpRequest")
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
        // para agregar productos al carrito desde la vusta productos

        public async Task<IActionResult>AgregarProducto(int id, int cantidad,
            int? categoriaId, string? busqueda, int pagina=1)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel == null)
            {
                return RedirectToAction(
                    "Productos",
                    new { categoriaId, busqueda, pagina }
                    );
            }
            else
                return NotFound();
        }

        // para agregar productos al carrito desde la vusta Index
        public async Task<IActionResult> AgregarProductoIndex(int id, int cantidad)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel == null)
            {
                return RedirectToAction("Index");
            }
            else
                return NotFound();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        // para agregar productos al carrito desde la vusta Detalle
        public async Task<IActionResult> AgregarProductoDetalle(int id, int cantidad)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel == null)
            {
                return RedirectToAction("ProductoDetalle", new {id
                });
            }
            else
                return NotFound();
        }
  
    }

}
