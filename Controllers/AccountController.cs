using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
namespace PaginaWeb.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(PaginaDatabaseContext context) : base(context)
        {
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(); // Devuelve la vista vacía para que el usuario llene el formulario
        }

        //EL METODO POST QUE PERIMTE REGISTRARSE Y GUARDARLO
        //registarse se lo permite a todos
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
                        usuario.RolId = clienteRol.RolId; // Asignar el rol de cliente
                    }

                    _context.Clientes.Add(usuario);
                    await _context.SaveChangesAsync(); // para guardar el usuario en la base de datos

                    // Guardar el usuario en la cookie
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Nombre));
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Cliente"));

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true // Mantener la sesión activa
                        }
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

        [AllowAnonymous]

        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Cliente"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string contrasena)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(contrasena))
            {
                ModelState.AddModelError("", "El correo electrónico y la contraseña son obligatorios.");
                return View();
            }
            var usuario = await _context.Clientes
                .FirstOrDefaultAsync(u => u.Email == email && u.Contrasena == contrasena);
            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo electrónico o contraseña incorrectos.");
                return View();
            }
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Nombre));
            identity.AddClaim(new Claim(ClaimTypes.Role, usuario.Rol.Nombre));
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true // Mantener la sesión activa
                }
            );
            if (usuario.Rol.Nombre == "Administrador")
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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
    }
}