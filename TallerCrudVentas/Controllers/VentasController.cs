using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TallerCrudVentas.Data;
using TallerCrudVentas.Models;

namespace TallerCrudVentas.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Venta.Include(v => v.Cliente);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta
                .Include(v => v.Cliente)
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.Producto)
                .FirstOrDefaultAsync(m => m.VentaId == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public IActionResult Create()
        {
            ViewBag.Clientes = _context.Clientes.ToList();
            ViewBag.Productos = _context.Productos.ToList();
            return View();
        }

        // POST: Ventas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentasViewModel model)
        {
            if (ModelState.IsValid)
            {
                decimal total = model.Total;
                total = decimal.Parse(total.ToString("0.00", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                total = Math.Round(total, 2);

                foreach (var detalle in model.Detalles)
                {
                    detalle.Precio = Math.Round(detalle.Precio, 2); // Redondea el precio a 2 decimales
                    detalle.Subtotal = Math.Round(detalle.Subtotal, 2); // Calcula el subtotal del detalle

                    var producto = await _context.Productos.FindAsync(detalle.ProductoId); // Busca el producto

                    if (producto != null)
                    {
                        producto.Stock -= detalle.Cantidad; // Actualiza el stock del producto
                    }
                }

                var venta = new Venta
                {
                    ClienteId = model.ClienteId,
                    Fecha = DateTime.Now,
                    Total = total,
                    Estado = "Realizada",
                    DetalleVenta = model.Detalles.Select(d => new DetalleVenta
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        Precio = d.Precio,
                        SubTotal = d.Subtotal
                    }).ToList()
                };

                Console.WriteLine($"Total a guardar: {total}"); // Imprime el total en la consola
                _context.Venta.Add(venta); // Agrega la venta al contexto
                await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos
                return RedirectToAction(nameof(Index)); // Redirige a la lista de ventas

            }
            ViewBag.Clientes = await _context.Clientes.ToListAsync(); // Recarga la lista de clientes
            ViewBag.Productos = await _context.Productos.ToListAsync(); // Recarga la lista de productos
            return View(model);
        }

        //Clase para recibir el estado desde la solicitud AJAX
        public class EstadoUpdateRequest
        {
            public string? Estado { get; set; }//Propiedad para almacenar el estado
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody]
        EstadoUpdateRequest request) // Método para actualizar el estado de una venta
        {
            if (request == null || string.IsNullOrEmpty(request.Estado)) // Verifica si el estado es válido
            {
                return BadRequest("Estado no válido."); // Devuelve un error si no es válido
            }

            var venta = await _context.Venta.Include(v => v.DetalleVenta).FirstOrDefaultAsync(v => v.VentaId == id); // Busca la venta por ID
            if (venta == null) // Si no se encuentra la venta
            {
                return NotFound(); // Devuelve un error 
            }

            // Si el estado es "Anulada", devolver los productos al stock
            if (request.Estado == "Anulada")
            {
                foreach (var detalle in venta.DetalleVenta)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock += detalle.Cantidad; // Aumenta el stock
                    }
                }
            }
            else if (request.Estado == "Realizada")
            {
                // Si el estado es "Realizada", descontar del stock
                foreach (var detalle in venta.DetalleVenta)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock -= detalle.Cantidad; // Disminuye el stock
                    }
                }
            }

            // Actualiza el estado
            venta.Estado = request.Estado; // Asigna el nuevo estado
            await _context.SaveChangesAsync(); // Guarda los cambios

            return Ok(); // Devuelve un resultado exitoso
        }




        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "ClienteId", venta.ClienteId);
            return View(venta);
        }

        // POST: Ventas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VentaId,Fecha,Total,ClienteId")] Venta venta)
        {
            if (id != venta.VentaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VentaExists(venta.VentaId))
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
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "ClienteId", venta.ClienteId);
            return View(venta);
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(m => m.VentaId == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venta = await _context.Venta.FindAsync(id);
            if (venta == null)
            {
                TempData["Error"] = "No se puede eliminar la venta, no se encontró";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Venta.Remove(venta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "No se puede eliminar la venta, ocurrió un error";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VentaExists(int id)
        {
            return _context.Venta.Any(e => e.VentaId == id);
        }
    }
}
