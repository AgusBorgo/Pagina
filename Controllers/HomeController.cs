using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, PaginaDatabaseContext context, IProductoServices productoServices, ICategoriaServices categoriaServices) : base(context) 
        {
            _logger = logger;
            _productoService = productoServices;
            _categoriaService = categoriaServices;
        }

        public IActionResult Index()
        {   // Para los productos destacados
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
