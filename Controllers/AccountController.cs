using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Data;
using PaginaWeb.Models;
using PaginaWeb.Models.ViewModel;

namespace PaginaWeb.Controllers
{
    // Todo lo de la cuenta exige sesión, salvo lo marcado con [AllowAnonymous]
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly PasswordHasher<Cliente> _hasher = new();

        public AccountController(PaginaDatabaseContext context) : base(context) { }

        // ---------- Registro ----------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegistroViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistroViewModel modelo)
        {
            // 1. ¿Cumple las reglas del ViewModel?
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            // 2. ¿El email ya está registrado?
            var email = modelo.Email.Trim().ToLowerInvariant();
            if (await _context.Clientes.AnyAsync(c => c.Email == email))
            {
                ModelState.AddModelError(nameof(modelo.Email), "Ya existe una cuenta con este email.");
                return View(modelo);
            }

            // 3. Armamos el cliente. El rol siempre es Cliente: nadie puede registrarse como administrador.
            var rolCliente = await _context.Roles.FirstAsync(r => r.Nombre == DatosIniciales.RolCliente);

            var usuario = new Cliente
            {
                Nombre = modelo.Nombre.Trim(),
                Apellido = modelo.Apellido.Trim(),
                Email = email,
                Telefono = modelo.Telefono.Trim(),
                Direccion = modelo.Direccion?.Trim() ?? string.Empty,
                RolId = rolCliente.RolId
            };
            usuario.Contrasena = _hasher.HashPassword(usuario, modelo.Contrasena);

            _context.Clientes.Add(usuario);
            await _context.SaveChangesAsync();

            // 4. Iniciamos sesión automáticamente
            await IniciarSesion(usuario, rolCliente.Nombre);
            TempData["Mensaje"] = $"¡Bienvenida/o, {usuario.Nombre}! Tu cuenta ya está creada.";
            return RedirectToAction("Index", "Home");
        }

        // ---------- Login y logout ----------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var email = modelo.Email.Trim().ToLowerInvariant();
            var usuario = await _context.Clientes
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Mismo mensaje si no existe el email o si la contraseña es incorrecta,
            // así nadie puede averiguar qué emails están registrados.
            if (usuario == null ||
                _hasher.VerifyHashedPassword(usuario, usuario.Contrasena, modelo.Contrasena) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return View(modelo);
            }

            var rol = usuario.Rol?.Nombre ?? DatosIniciales.RolCliente;
            await IniciarSesion(usuario, rol);

            // Volver a donde estaba (por ejemplo, el carrito). Url.IsLocalUrl evita redirecciones a otros sitios.
            if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
            {
                return LocalRedirect(modelo.ReturnUrl);
            }

            return rol == DatosIniciales.RolAdministrador
                ? RedirectToAction("Index", "Dashboard")
                : RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Guarda en la cookie quién es el usuario y qué rol tiene
        private async Task IniciarSesion(Cliente usuario, string rol)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim("UsuarioId", usuario.UsuarioId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Nombre));
            identity.AddClaim(new Claim(ClaimTypes.Email, usuario.Email));
            identity.AddClaim(new Claim(ClaimTypes.Role, rol));

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true });
        }

        // ---------- Perfil ----------

        [HttpGet]
        public async Task<IActionResult> MisDatos()
        {
            var usuario = await _context.Clientes
                .Include(c => c.Pedidos)
                .FirstOrDefaultAsync(c => c.UsuarioId == ObtenerUsuarioId());

            if (usuario == null)
            {
                return RedirectToAction(nameof(Logout));
            }

            return View(new PerfilViewModel
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                CantidadPedidos = usuario.Pedidos.Count
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MisDatos(PerfilViewModel modelo)
        {
            var usuario = await _context.Clientes
                .Include(c => c.Rol)
                .Include(c => c.Pedidos)
                .FirstOrDefaultAsync(c => c.UsuarioId == ObtenerUsuarioId());

            if (usuario == null)
            {
                return RedirectToAction(nameof(Logout));
            }

            // Estos dos datos no vienen del formulario: los completamos desde la base
            modelo.Email = usuario.Email;
            modelo.CantidadPedidos = usuario.Pedidos.Count;

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            usuario.Nombre = modelo.Nombre.Trim();
            usuario.Apellido = modelo.Apellido.Trim();
            usuario.Telefono = modelo.Telefono.Trim();
            usuario.Direccion = modelo.Direccion?.Trim() ?? string.Empty;
            await _context.SaveChangesAsync();

            // Renovamos la cookie para que el header muestre el nombre nuevo
            await IniciarSesion(usuario, usuario.Rol?.Nombre ?? DatosIniciales.RolCliente);

            TempData["Mensaje"] = "Tus datos se guardaron.";
            return RedirectToAction(nameof(MisDatos));
        }

        // ---------- Pedidos ----------

        public async Task<IActionResult> Historial()
        {
            int usuarioId = ObtenerUsuarioId();

            var pedidos = await _context.Pedidos
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }

        public async Task<IActionResult> DetallePedido(int id)
        {
            int usuarioId = ObtenerUsuarioId();

            // Filtrar por usuario evita que alguien vea pedidos ajenos cambiando el id en la URL
            var pedido = await _context.Pedidos
                .Where(p => p.PedidoId == id && p.UsuarioId == usuarioId)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync();

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }
    }
}
