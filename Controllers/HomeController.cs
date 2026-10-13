using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PaginaWeb.Context;
using PaginaWeb.Models;
using PaginaWeb.Services;

namespace PaginaWeb.Controllers
{
    public class HomeController : BaseController
    {
        private const int DiscosPorPagina = 12;

        private readonly IProductoServices _productoService;
        private readonly ICategoriaServices _categoriaService;

        public HomeController(
            PaginaDatabaseContext context,
            IProductoServices productoServices,
            ICategoriaServices categoriaServices) : base(context)
        {
            _productoService = productoServices;
            _categoriaService = categoriaServices;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categorias = await _categoriaService.GetCategorias();
            var destacados = await _productoService.GetProductosDestacados(4);
            return View(destacados);
        }

        // Catálogo con filtro por género, búsqueda y paginación
        public async Task<IActionResult> Productos(int? categoriaId, string? busqueda, int pagina = 1)
        {
            var modelo = await _productoService.GetProductosPaginados(categoriaId, busqueda, pagina, DiscosPorPagina);
            ViewBag.Categorias = await _categoriaService.GetCategorias();
            return View(modelo);
        }

        public async Task<IActionResult> DetallesProducto(int id)
        {
            var producto = await _productoService.GetProducto(id);
            if (producto == null)
            {
                return NotFound();
            }

            ViewBag.Relacionados = await _productoService.GetRelacionados(producto, 4);
            return View(producto);
        }

        // Agrega un disco al carrito y vuelve a la página desde donde se agregó
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAlCarrito(int id, int cantidad = 1, string? volverA = null)
        {
            var carrito = await AgregarProductoAlCarrito(id, cantidad);

            TempData["Mensaje"] = carrito != null
                ? "Agregaste el disco al carrito."
                : "No pudimos agregar el disco: no hay stock disponible.";

            if (!string.IsNullOrEmpty(volverA) && Url.IsLocalUrl(volverA))
            {
                return LocalRedirect(volverA);
            }
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
