using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
    public class ReservasController : Controller
    {
        private readonly RepositorioReserva _repositorioReserva;

        public ReservasController(IConfiguration configuration)
        {
            _repositorioReserva = new RepositorioReserva(configuration);
        }

        // GET: Reservas
        public IActionResult Index()
        {
            var reservas = _repositorioReserva.GetAllActivas();
            return View(reservas);
        }

        // GET: Reservas/Details/5
        public IActionResult Details(int id)
        {
            var reserva = _repositorioReserva.GetById(id);
            if (reserva == null)
            {
                return NotFound();
            }
            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            ViewBag.IdInmueble = new SelectList(_repositorioReserva.GetInmueblesDisponibles(), "Id", "Direccion");
            ViewBag.IdInquilino = new SelectList(_repositorioReserva.GetInquilinosActivos(), "IdInquilino", "NombreCompleto");
            return View();
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            // Asignamos el ID del usuario por defecto hasta que hagamos autenticación
            reserva.CreadoPorUserId = 1;

            ModelState.Remove(nameof(reserva.CreadoPorUserId));

            var errores = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errores)
            {
                Console.WriteLine("ERROR DE MODELO: " + error.ErrorMessage);
            }

            if (_repositorioReserva.ExisteSuperposicion(reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta))
            {
                ModelState.AddModelError("", "Ya existe una reserva activa para este inmueble en el rango de fechas seleccionado.");
            }

            if (reserva.FechaDesde >= reserva.FechaHasta)
            {
                ModelState.AddModelError("FechaHasta", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (ModelState.IsValid)
            {
                _repositorioReserva.Create(reserva);
                TempData["Success"] = "La reserva fue creada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IdInmueble = new SelectList(_repositorioReserva.GetInmueblesDisponibles(), "Id", "Direccion", reserva.IdInmueble);
            ViewBag.IdInquilino = new SelectList(_repositorioReserva.GetInquilinosActivos(), "IdInquilino", "NombreCompleto", reserva.IdInquilino);
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public IActionResult Edit(int id)
        {
            var reserva = _repositorioReserva.GetById(id);
            if (reserva == null)
            {
                return NotFound();
            }

            ViewBag.IdInmueble = new SelectList(_repositorioReserva.GetInmueblesDisponibles(), "Id", "Direccion", reserva.IdInmueble);
            ViewBag.IdInquilino = new SelectList(_repositorioReserva.GetInquilinosActivos(), "IdInquilino", "NombreCompleto", reserva.IdInquilino);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Reserva reserva)
        {
            if (id != reserva.Id)
            {
                return NotFound();
            }

            if (_repositorioReserva.ExisteSuperposicion(reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta, reserva.Id))
            {
                ModelState.AddModelError("", "Ya existe otra reserva activa para este inmueble en el rango de fechas seleccionado.");
            }

            if (reserva.FechaDesde >= reserva.FechaHasta)
            {
                ModelState.AddModelError("FechaHasta", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _repositorioReserva.Update(reserva);
                    TempData["Success"] = "La reserva fue actualizada correctamente.";
                }
                catch (Exception)
                {
                    if (!_repositorioReserva.Exists(reserva.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IdInmueble = new SelectList(_repositorioReserva.GetInmueblesDisponibles(), "Id", "Direccion", reserva.IdInmueble);
            ViewBag.IdInquilino = new SelectList(_repositorioReserva.GetInquilinosActivos(), "IdInquilino", "NombreCompleto", reserva.IdInquilino);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public IActionResult Delete(int id)
        {
            var reserva = _repositorioReserva.GetById(id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioReserva.DeleteLogico(id);
            TempData["Success"] = "La reserva fue cancelada (borrado lógico) correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetPrecioInmueble(int id)
        {
            var precio = _repositorioReserva.GetPrecioInmueble(id);
            if (precio == null)
            {
                return NotFound();
            }
            return Json(new { precio = precio.Value });
        }

        [HttpGet]
        public IActionResult GetFechasReservadas(int idInmueble)
        {
            var fechas = _repositorioReserva.ObtenerFechasReservadasPorInmueble(idInmueble);
            return Json(fechas);
        }
    }
}