using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;

namespace PaginaWeb.Controllers
{
    // Administración del catálogo de discos
    [Authorize(Policy = "AdminOnly")]
    public class ProductoesController : BaseController
    {
        public ProductoesController(PaginaDatabaseContext context) : base(context) { }

        public async Task<IActionResult> Index(string? busqueda)
        {
            var query = _context.Producto.Include(p => p.Categoria).AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(busqueda) || p.Artista.Contains(busqueda));
            }

            ViewBag.Busqueda = busqueda;
            return View(await query.OrderBy(p => p.Artista).ThenBy(p => p.Anio).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            await CargarGeneros();
            return View(new Producto { Stock = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Artista,Anio,Descripcion,Precio,ImagenUrl,Stock,CategoriaId,Activo")] Producto producto)
        {
            if (!ModelState.IsValid)
            {
                await CargarGeneros(producto.CategoriaId);
                return View(producto);
            }

            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = $"Se agregó \"{producto.Nombre}\" al catálogo.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            await CargarGeneros(producto.CategoriaId);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProductoId,Nombre,Artista,Anio,Descripcion,Precio,ImagenUrl,Stock,CategoriaId,Activo")] Producto producto)
        {
            if (id != producto.ProductoId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await CargarGeneros(producto.CategoriaId);
                return View(producto);
            }

            var existente = await _context.Producto.FindAsync(id);
            if (existente == null)
            {
                return NotFound();
            }

            // Copiamos solo los campos editables sobre el disco que ya está en la base
            _context.Entry(existente).CurrentValues.SetValues(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Se guardaron los cambios de \"{producto.Nombre}\".";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.ProductoId == id);

            if (producto == null)
            {
                return NotFound();
            }

            ViewBag.TieneVentas = await _context.PedidoDetalles.AnyAsync(d => d.ProductoId == id);
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Si el disco ya se vendió, no lo borramos (romperíamos el historial de pedidos):
            // lo ocultamos de la tienda.
            if (await _context.PedidoDetalles.AnyAsync(d => d.ProductoId == id))
            {
                producto.Activo = false;
                TempData["Mensaje"] = $"\"{producto.Nombre}\" tiene ventas, así que se ocultó de la tienda en lugar de borrarse.";
            }
            else
            {
                _context.Producto.Remove(producto);
                TempData["Mensaje"] = $"Se eliminó \"{producto.Nombre}\".";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarGeneros(int? seleccionado = null)
        {
            var generos = await _context.Categoria.OrderBy(c => c.Nombre).ToListAsync();
            ViewBag.Generos = new SelectList(generos, "CategoriaId", "Nombre", seleccionado);
        }
    }
}
