using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservasTemporales.Data;
using ReservasTemporales.Models;

namespace ReservasTemporales.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InquilinosController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region GET Actions

        // GET: Inquilinos
        public async Task<IActionResult> Index(string buscar, int pagina = 1)
        {
            int registrosPorPagina = 1;

            var query = _context.Inquilinos.Where(i => i.Activo);

            // búsqueda en el servidor por Nombre, Apellido o DNI
            if (!string.IsNullOrEmpty(buscar))
            {
                buscar = buscar.Trim();
                query = query.Where(i => i.Nombre.Contains(buscar) ||
                                         i.Apellido.Contains(buscar) ||
                                         i.Dni.Contains(buscar));
            }

            // paginado en el servidor
            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            pagina = Math.Max(1, Math.Min(pagina, totalPaginas > 0 ? totalPaginas : 1));

            var listado = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync(); //acá se ejecuta la consulta en MySQL.

            // mapear datos a la Vista
            ViewData["FiltroActual"] = buscar;
            ViewData["PaginaActual"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas;

            return View(listado);
        }

        // GET: Inquilinos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos
                .FirstOrDefaultAsync(i => i.IdInquilino == id);

            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos.FindAsync(id);

            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos
                .FirstOrDefaultAsync(i => i.IdInquilino == id);

            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        #endregion

        #region POST Actions

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inquilino inquilino)
        {
            // para prevenir que se guarden dos personas con el mismo correo
            if (await _context.Inquilinos.AnyAsync(i => i.Email == inquilino.Email))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(inquilino);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inquilino inquilino)
        {
            if (id != inquilino.IdInquilino)
            {
                return NotFound();
            }

            // Validar si otro inquilino distinto ya tiene este email
            if (await _context.Inquilinos.AnyAsync(i => i.Email == inquilino.Email && i.IdInquilino != id))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado por otro inquilino.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inquilino);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InquilinoExists(inquilino.IdInquilino))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inquilino = await _context.Inquilinos.FindAsync(id);

            if (inquilino == null)
            {
                return NotFound();
            }

            // Baja lógica
            inquilino.Activo = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Private Helpers

        private bool InquilinoExists(int id)
        {
            return _context.Inquilinos
                .Any(e => e.IdInquilino == id);
        }

        #endregion
    }
}