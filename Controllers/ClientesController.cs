using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models.ViewModel;

namespace PaginaWeb.Controllers
{
    // Administración de usuarios. Los clientes se registran solos desde la tienda,
    // así que acá solo se consultan, se cambia su rol o se eliminan. Nunca se ven contraseñas.
    [Authorize(Policy = "AdminOnly")]
    public class ClientesController : BaseController
    {
        public ClientesController(PaginaDatabaseContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Clientes
                .Include(c => c.Rol)
                .Include(c => c.Pedidos)
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
            return View(usuarios);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Clientes.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            await CargarRoles(usuario.RolId);
            return View(new UsuarioAdminViewModel
            {
                UsuarioId = usuario.UsuarioId,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                RolId = usuario.RolId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int rolId)
        {
            var usuario = await _context.Clientes.FindAsync(id);
            if (usuario == null || !await _context.Roles.AnyAsync(r => r.RolId == rolId))
            {
                return NotFound();
            }

            // Evita que el admin se quite su propio rol y se quede afuera del panel
            if (usuario.UsuarioId == ObtenerUsuarioId() && usuario.RolId != rolId)
            {
                TempData["Error"] = "No podés cambiar tu propio rol.";
                return RedirectToAction(nameof(Index));
            }

            usuario.RolId = rolId;
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = $"Se actualizó el rol de {usuario.Nombre}. El cambio se aplica la próxima vez que inicie sesión.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Clientes
                .Include(c => c.Rol)
                .Include(c => c.Pedidos)
                .FirstOrDefaultAsync(c => c.UsuarioId == id);
            return usuario == null ? NotFound() : View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == ObtenerUsuarioId())
            {
                TempData["Error"] = "No podés eliminar tu propio usuario.";
                return RedirectToAction(nameof(Index));
            }

            var usuario = await _context.Clientes.FindAsync(id);
            if (usuario != null)
            {
                _context.Clientes.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Se eliminó a {usuario.Nombre} {usuario.Apellido}.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRoles(int seleccionado)
        {
            var roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
            ViewBag.Roles = new SelectList(roles, "RolId", "Nombre", seleccionado);
        }
    }
}
