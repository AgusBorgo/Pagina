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


    }
}