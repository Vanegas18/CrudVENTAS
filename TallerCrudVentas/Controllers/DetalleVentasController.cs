using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TallerCrudVentas.Data;
using TallerCrudVentas.Models;

namespace TallerCrudVentas.Controllers
{
    public class DetalleVentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetalleVentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DetalleVentas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DetalleVentas.Include(d => d.Producto).Include(d => d.Venta);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DetalleVentas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetalleVentas
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(m => m.DetalleVentaId == id);
            if (detalleVenta == null)
            {
                return NotFound();
            }

            return View(detalleVenta);
        }

        // GET: DetalleVentas/Create
        public IActionResult Create()
        {
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre");
            ViewData["VentaId"] = new SelectList(_context.Venta, "VentaId", "VentaId");
            return View();
        }

        // POST: DetalleVentas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DetalleVentaId,VentaId,ProductoId,Cantidad,Precio,SubTotal")] DetalleVenta detalleVenta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalleVenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "VentaId", "VentaId", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // GET: DetalleVentas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetalleVentas.FindAsync(id);
            if (detalleVenta == null)
            {
                return NotFound();
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "VentaId", "VentaId", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // POST: DetalleVentas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetalleVentaId,VentaId,ProductoId,Cantidad,Precio,SubTotal")] DetalleVenta detalleVenta)
        {
            if (id != detalleVenta.DetalleVentaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleVenta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleVentaExists(detalleVenta.DetalleVentaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductoId"] = new SelectList(_context.Productos, "ProductoId", "Nombre", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "VentaId", "VentaId", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // GET: DetalleVentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetalleVentas
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(m => m.DetalleVentaId == id);
            if (detalleVenta == null)
            {
                return NotFound();
            }

            return View(detalleVenta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalleVenta = await _context.DetalleVentas.FindAsync(id);
            if (detalleVenta == null)
            {
                TempData["mensaje"] = "No se puede eliminar el detalle de la venta";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.DetalleVentas.Remove(detalleVenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["mensaje"] = "No se puede eliminar el detalle de la venta";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleVentaExists(int id)
        {
            return _context.DetalleVentas.Any(e => e.DetalleVentaId == id);
        }
    }
}
