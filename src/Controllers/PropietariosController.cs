using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasTemporales.Data;
using ReservasTemporales.Models;

namespace ReservasTemporales.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropietariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Propietarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Propietarios
                .Where(p => p.Activo)
                .ToListAsync());
        }

        // GET: Propietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.Propietarios
                .FirstOrDefaultAsync(p => p.IdPropietario == id);

            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Propietario propietario)
        {
            // para prevenir que se guarden dos personas con el mismo correo
            if (await _context.Propietarios.AnyAsync(p => p.Email == propietario.Email))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(propietario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(propietario);
        }

        // GET: Propietarios/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.Propietarios.FindAsync(id);

            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // POST: Propietarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Propietario propietario)
        {
            if (id != propietario.IdPropietario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(propietario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropietarioExists(propietario.IdPropietario))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(propietario);
        }

        // GET: Propietarios/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.Propietarios
                .FirstOrDefaultAsync(p => p.IdPropietario == id);

            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // POST: Propietarios/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propietario = await _context.Propietarios
                .FindAsync(id);

            if (propietario == null)
            {
                return NotFound();
            }

            // Baja lógica
            propietario.Activo = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PropietarioExists(int id)
        {
            return _context.Propietarios
                .Any(e => e.IdPropietario == id);
        }
    }
}