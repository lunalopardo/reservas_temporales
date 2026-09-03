using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservasTemporales.Data;
using ReservasTemporales.Models;

namespace ReservasTemporales.Controllers
{
    public class ReservasController : Controller
    {
        // ============================================================
        // CONEXION CON LA BASE DE DATOS
        // ============================================================

        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // GET: Reservas
        // Muestra el listado de reservas activas
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Inquilino)
                .Include(r => r.CreadoPorUsuario)
                .Include(r => r.TerminadoPorUsuario)
                .Where(r => r.Activo)
                .ToListAsync();

            return View(reservas);
        }


        // ============================================================
        // GET: Reservas/Details/5
        // Muestra los detalles de una reserva
        // ============================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Inquilino)
                .Include(r => r.CreadoPorUsuario)
                .Include(r => r.TerminadoPorUsuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }


        // ============================================================
        // GET: Reservas/Create
        // ============================================================

        public async Task<IActionResult> Create()
        {
            await CargarInmueblesAsync();
            await CargarInquilinosAsync();

            return View();
        }


        // ============================================================
        // POST: Reservas/Create
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reserva reserva)
        {
            // ========================================================
            // 1. VALIDAR FECHAS
            // ========================================================

            if (reserva.FechaDesde.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FechaDesde",
                    "La fecha de inicio no puede ser anterior a la fecha actual."
                );
            }

            if (reserva.FechaDesde.Date >= reserva.FechaHasta.Date)
            {
                ModelState.AddModelError(
                    "FechaHasta",
                    "La fecha de finalizacion debe ser posterior a la fecha de inicio."
                );
            }


            // ========================================================
            // 2. BUSCAR INMUEBLE
            // ========================================================

            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == reserva.IdInmueble);

            if (inmueble == null)
            {
                ModelState.AddModelError(
                    "IdInmueble",
                    "El inmueble seleccionado no existe."
                );
            }
            else
            {
                // ====================================================
                // 3. VERIFICAR DISPONIBILIDAD
                // ====================================================

                if (!inmueble.EstaDisponibleHoy || !inmueble.Activo)
                {
                    ModelState.AddModelError(
                        "IdInmueble",
                        "El inmueble no esta disponible para reservar."
                    );
                }


                // ====================================================
                // 4. VERIFICAR SUPERPOSICION
                // ====================================================

                bool existeSuperposicion = await _context.Reservas
                    .AnyAsync(r =>
                        r.IdInmueble == reserva.IdInmueble &&
                        r.Activo &&
                        r.FechaDesde < reserva.FechaHasta &&
                        r.FechaHasta > reserva.FechaDesde
                    );

                if (existeSuperposicion)
                {
                    ModelState.AddModelError(
                        "FechaDesde",
                        "El inmueble no esta disponible para las fechas seleccionadas."
                    );
                }
            }


            // ========================================================
            // 5. GUARDAR RESERVA
            // ========================================================

            if (ModelState.IsValid)
            {
                // Por ahora usamos el usuario administrador con Id = 1.
                reserva.CreadoPorUserId = 1;

                // La reserva comienza activa.
                reserva.Activo = true;

                _context.Reservas.Add(reserva);

                await _context.SaveChangesAsync();


                // ====================================================
                // MENSAJE DE EXITO
                // ====================================================

                TempData["Mensaje"] =
                    "Reserva agregada correctamente.";

                TempData["TipoMensaje"] =
                    "exito";


                return RedirectToAction(nameof(Index));
            }


            // ========================================================
            // 6. SI HAY ERRORES, RECARGAR LOS SELECT
            // ========================================================

            await CargarInmueblesAsync(reserva.IdInmueble);
            await CargarInquilinosAsync(reserva.IdInquilino);

            return View(reserva);
        }


        // ============================================================
        // GET: Reservas/Edit/5
        // ============================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            await CargarInmueblesAsync(reserva.IdInmueble);
            await CargarInquilinosAsync(reserva.IdInquilino);

            return View(reserva);
        }


        // ============================================================
        // POST: Reservas/Edit/5
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reserva reserva)
        {
            // ========================================================
            // 1. VERIFICAR ID
            // ========================================================

            if (id != reserva.Id)
            {
                return NotFound();
            }


            // ========================================================
            // 2. OBTENER RESERVA ORIGINAL
            // ========================================================

            var reservaOriginal = await _context.Reservas
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservaOriginal == null)
            {
                return NotFound();
            }


            // ========================================================
            // 3. VALIDAR FECHAS
            // ========================================================

            if (reserva.FechaDesde.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FechaDesde",
                    "La fecha de inicio no puede ser anterior a la fecha actual."
                );
            }

            if (reserva.FechaDesde.Date >= reserva.FechaHasta.Date)
            {
                ModelState.AddModelError(
                    "FechaHasta",
                    "La fecha de finalizacion debe ser posterior a la fecha de inicio."
                );
            }


            // ========================================================
            // 4. VERIFICAR SUPERPOSICION
            // ========================================================

            bool existeSuperposicion = await _context.Reservas
                .AnyAsync(r =>
                    r.Id != reserva.Id &&
                    r.IdInmueble == reserva.IdInmueble &&
                    r.Activo &&
                    r.FechaDesde < reserva.FechaHasta &&
                    r.FechaHasta > reserva.FechaDesde
                );

            if (existeSuperposicion)
            {
                ModelState.AddModelError(
                    "FechaDesde",
                    "El inmueble no esta disponible para las fechas seleccionadas."
                );
            }


            // ========================================================
            // 5. GUARDAR CAMBIOS
            // ========================================================

            if (ModelState.IsValid)
            {
                // Conservamos el usuario que creo originalmente
                // la reserva.

                reserva.CreadoPorUserId =
                    reservaOriginal.CreadoPorUserId;

                // Conservamos el estado de la reserva.

                reserva.Activo =
                    reservaOriginal.Activo;

                try
                {
                    _context.Reservas.Update(reserva);

                    await _context.SaveChangesAsync();


                    // ====================================================
                    // MENSAJE DE EXITO
                    // ====================================================

                    TempData["Mensaje"] =
                        "Reserva modificada correctamente.";

                    TempData["TipoMensaje"] =
                        "exito";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }


            // ========================================================
            // 6. SI HAY ERRORES, RECARGAR LOS SELECT
            // ========================================================

            await CargarInmueblesAsync(reserva.IdInmueble);
            await CargarInquilinosAsync(reserva.IdInquilino);

            return View(reserva);
        }


        // ============================================================
        // GET: Reservas/Delete/5
        // ============================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Inquilino)
                .Include(r => r.CreadoPorUsuario)
                .Include(r => r.TerminadoPorUsuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }


        // ============================================================
        // POST: Reservas/Delete
        // Baja logica
        // ============================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Baja logica
            reserva.Activo = false;

            // Por ahora usamos el usuario administrador.
            reserva.TerminadoPorUserId = 1;

            await _context.SaveChangesAsync();


            // ========================================================
            // MENSAJE DE EXITO
            // ========================================================

            TempData["Mensaje"] =
                "Reserva eliminada correctamente.";

            TempData["TipoMensaje"] =
                "exito";


            return RedirectToAction(nameof(Index));
        }


      
// ============================================================
// GET: Reservas/FechasOcupadas
// Devuelve las reservas de otros
// y la reserva actual si estamos editando
// ============================================================

[HttpGet]
public async Task<IActionResult> FechasOcupadas(
    int idInmueble,
    int? idReserva = null)
{
    // ========================================================
    // RESERVAS DE OTRAS RESERVAS
    // ========================================================

    var reservasOtras = await _context.Reservas
        .Where(r =>
            r.IdInmueble == idInmueble &&
            r.Activo &&
            (!idReserva.HasValue ||
             r.Id != idReserva.Value)
        )
        .Select(r => new
        {
            fechaDesde =
                r.FechaDesde.ToString("yyyy-MM-dd"),

            fechaHasta =
                r.FechaHasta.ToString("yyyy-MM-dd")
        })
        .ToListAsync();


    // ========================================================
    // RESERVA ACTUAL
    // ========================================================

    object? reservaActual = null;

    if (idReserva.HasValue)
    {
        reservaActual = await _context.Reservas
            .Where(r =>
                r.Id == idReserva.Value &&
                r.IdInmueble == idInmueble &&
                r.Activo
            )
            .Select(r => new
            {
                fechaDesde =
                    r.FechaDesde.ToString("yyyy-MM-dd"),

                fechaHasta =
                    r.FechaHasta.ToString("yyyy-MM-dd")
            })
            .FirstOrDefaultAsync();
    }


    // ========================================================
    // DEVOLVER DATOS A JAVASCRIPT
    // ========================================================

    return Json(new
    {
        reservasOtras,
        reservaActual
    });
}



        

        // ============================================================
        // METODOS AUXILIARES
        // ============================================================

        private bool ReservaExists(int id)
        {
            return _context.Reservas
                .Any(r => r.Id == id);
        }


        // ============================================================
        // CARGAR INMUEBLES
        // ============================================================

        private async Task CargarInmueblesAsync(
            object? seleccionado = null)
        {
            var inmuebles = await _context.Inmuebles
                .Where(i => i.Activo && i.EstaDisponibleHoy)
                .OrderBy(i => i.Direccion)
                .ToListAsync();

            ViewBag.IdInmueble = new SelectList(
                inmuebles,
                "Id",
                "Direccion",
                seleccionado
            );
        }


        // ============================================================
        // CARGAR INQUILINOS
        // ============================================================

        private async Task CargarInquilinosAsync(
            object? seleccionado = null)
        {
            var inquilinos = await _context.Inquilinos
                .Where(i => i.Activo)
                .OrderBy(i => i.Apellido)
                .ThenBy(i => i.Nombre)
                .ToListAsync();

            ViewBag.IdInquilino = new SelectList(
                inquilinos,
                "IdInquilino",
                "Nombre",
                seleccionado
            );
        }
    }
}

