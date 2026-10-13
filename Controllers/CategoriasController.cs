using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;

namespace PaginaWeb.Controllers
{
    // Administración de géneros (categorías)
    [Authorize(Policy = "AdminOnly")]
    public class CategoriasController : BaseController
    {
        public CategoriasController(PaginaDatabaseContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var generos = await _context.Categoria
                .Include(c => c.Productos)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            return View(generos);
        }

        public IActionResult Create()
        {
            return View(new Categoria());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            _context.Categoria.Add(categoria);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = $"Se creó el género \"{categoria.Nombre}\".";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            return categoria == null ? NotFound() : View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Nombre,Descripcion")] Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var existente = await _context.Categoria.FindAsync(id);
            if (existente == null)
            {
                return NotFound();
            }

            existente.Nombre = categoria.Nombre;
            existente.Descripcion = categoria.Descripcion;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Se guardaron los cambios del género.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categoria
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.CategoriaId == id);
            return categoria == null ? NotFound() : View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categoria
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            if (categoria == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Borrar un género con discos borraría también esos discos: no lo permitimos
            if (categoria.Productos.Any())
            {
                TempData["Error"] = $"No se puede borrar \"{categoria.Nombre}\" porque tiene discos. Pasalos a otro género primero.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categoria.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = $"Se eliminó el género \"{categoria.Nombre}\".";
            return RedirectToAction(nameof(Index));
        }
    }
}
