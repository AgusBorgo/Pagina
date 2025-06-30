using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaginaWeb.Context;
using PaginaWeb.Models;

namespace PaginaWeb.Controllers
{
    public class PedidoDetallesController : BaseController
    {
        public PedidoDetallesController(PaginaDatabaseContext context) : base(context) { }

        // GET: PedidoDetalles
        public async Task<IActionResult> Index()
        {
            var pedidoDetalles = _context.PedidoDetalles
                .Include(p => p.Pedido)
                .Include(p => p.Producto);
            return View(await pedidoDetalles.ToListAsync());
        }

        // GET: PedidoDetalles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pedidoDetalle = await _context.PedidoDetalles
                .Include(p => p.Pedido)
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(m => m.DetallePedidoId == id);

            if (pedidoDetalle == null) return NotFound();

            return View(pedidoDetalle);
        }

        // GET: PedidoDetalles/Create
        public IActionResult Create()
        {
            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "PedidoId", "Estado");
            ViewData["ProductoId"] = new SelectList(_context.Producto, "ProductoId", "Nombre");
            return View();
        }

        // POST: PedidoDetalles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DetallePedidoId,PedidoId,ProductoId,Cantidad,PrecioUnitario")] PedidoDetalle pedidoDetalle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pedidoDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "PedidoId", "Estado", pedidoDetalle.PedidoId);
            ViewData["ProductoId"] = new SelectList(_context.Producto, "ProductoId", "Nombre", pedidoDetalle.ProductoId);
            return View(pedidoDetalle);
        }

        // GET: PedidoDetalles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pedidoDetalle = await _context.PedidoDetalles.FindAsync(id);
            if (pedidoDetalle == null) return NotFound();

            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "PedidoId", "Estado", pedidoDetalle.PedidoId);
            ViewData["ProductoId"] = new SelectList(_context.Producto, "ProductoId", "Nombre", pedidoDetalle.ProductoId);
            return View(pedidoDetalle);
        }

        // POST: PedidoDetalles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetallePedidoId,PedidoId,ProductoId,Cantidad,PrecioUnitario")] PedidoDetalle pedidoDetalle)
        {
            if (id != pedidoDetalle.DetallePedidoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedidoDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoDetalleExists(pedidoDetalle.DetallePedidoId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["PedidoId"] = new SelectList(_context.Pedidos, "PedidoId", "Estado", pedidoDetalle.PedidoId);
            ViewData["ProductoId"] = new SelectList(_context.Producto, "ProductoId", "Nombre", pedidoDetalle.ProductoId);
            return View(pedidoDetalle);
        }
        [HttpPost]
        public async Task<IActionResult> AgregarProductoDetalle(int id, int cantidad)
        {
            var carritoViewModel = await AgregarProductoAlCarrito(id, cantidad);
            if (carritoViewModel == null)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Details", new { id });
        }


        // GET: PedidoDetalles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pedidoDetalle = await _context.PedidoDetalles
                .Include(p => p.Pedido)
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(m => m.DetallePedidoId == id);

            if (pedidoDetalle == null) return NotFound();

            return View(pedidoDetalle);
        }

        // POST: PedidoDetalles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pedidoDetalle = await _context.PedidoDetalles.FindAsync(id);
            if (pedidoDetalle != null)
            {
                _context.PedidoDetalles.Remove(pedidoDetalle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PedidoDetalleExists(int id)
        {
            return _context.PedidoDetalles.Any(e => e.DetallePedidoId == id);
        }
    }
}

