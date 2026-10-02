using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;


namespace PaginaWeb.Controllers
{
    public class AccountController : BaseController
    {
        private readonly PasswordHasher<Cliente> _hasher = new();
        public AccountController(PaginaDatabaseContext context) : base(context) { }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(Cliente usuario)
        {
            try
            {
                if (usuario != null)
                {
                    if (await _context.Clientes.AnyAsync(c => c.Email == usuario.Email))
                    {
                        return BadRequest("El correo electrónico ya está en uso.");
                    }

                    var clienteRol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");
                    if (clienteRol != null)
                    {
                        usuario.RolId = clienteRol.RolId;
                    }
                    usuario.Contrasena = _hasher.HashPassword(usuario, usuario.Contrasena);
                    _context.Clientes.Add(usuario);
                    await _context.SaveChangesAsync();

                   
                    var usuarioGuardado = await _context.Clientes
                        .Include(u => u.Rol)
                        .FirstAsync(u => u.Email == usuario.Email);

                    

                    await HttpContext.SignOutAsync();

                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identity.AddClaim(new Claim("UsuarioId", usuarioGuardado.UsuarioId.ToString()));
                    identity.AddClaim(new Claim(ClaimTypes.Name, usuarioGuardado.Nombre));
                    identity.AddClaim(new Claim(ClaimTypes.Role, usuarioGuardado.Rol?.Nombre ?? "Cliente"));

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        new AuthenticationProperties { IsPersistent = true }
                    );


                    return RedirectToAction("Index", "Home");
                }

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al registrar el usuario: {ex.Message}");
            }
        }

      
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Cliente"))
                    return RedirectToAction("Index", "Home");
                else if (User.IsInRole("Administrador"))
                    return RedirectToAction("Index", "Admin");
            }

            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> Login(string email, string contrasena)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(contrasena))
            {
                ModelState.AddModelError("", "El correo electrónico y la contraseña son obligatorios.");
                return View();
            }

            var usuario = await _context.Clientes
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null || _hasher.VerifyHashedPassword(usuario, usuario.Contrasena, contrasena) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Correo electrónico o contraseña incorrectos.");
                return View();
            }

            await HttpContext.SignOutAsync(); 

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim("UsuarioId", usuario.UsuarioId.ToString())); 
            identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Nombre));
            identity.AddClaim(new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Cliente"));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true }
            );


            if (usuario.Rol?.Nombre == "Administrador")
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Index", "Home");
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
        public async Task<IActionResult> HistorialCompleto()
        {
            int usuarioId = ObtenerUsuarioId();
            System.Diagnostics.Debug.WriteLine("UsuarioId para historial completo: " + usuarioId);

            var todosPedidos = await _context.Pedidos
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Total de pedidos encontrados: {todosPedidos.Count}");

            return View("Historial", todosPedidos); 
        }

        public async Task<IActionResult> DetallePedido(int id)
        {
            int usuarioId = ObtenerUsuarioId();

            var pedido = await _context.Pedidos
                .Where(p => p.PedidoId == id && p.UsuarioId == usuarioId)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync();

            if (pedido == null)
            {
                return NotFound("Pedido no encontrado");
            }

            return View(pedido);
        }

        public async Task<IActionResult> DebugPedidos()
        {
            int usuarioId = ObtenerUsuarioId();

            var todosPedidos = await _context.Pedidos
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();


            foreach (var pedido in todosPedidos)
            {
                System.Diagnostics.Debug.WriteLine($"Pedido ID: {pedido.PedidoId}, Estado: {pedido.Estado}, Fecha: {pedido.FechaPedido}");
            }

            return View("Historial", todosPedidos); 
        }

    }


}