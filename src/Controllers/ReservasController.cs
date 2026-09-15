using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
    public class ReservasController : Controller
    {
        private readonly RepositorioReserva _repositorioReserva;
        private readonly RepositorioInmueble _repositorioInmueble;
        private readonly RepositorioInquilino _repositorioInquilino;

        public ReservasController(
                RepositorioReserva repositorioReserva,
                RepositorioInmueble repositorioInmueble,
                RepositorioInquilino repositorioInquilino)
        {
            _repositorioReserva = repositorioReserva;
            _repositorioInmueble = repositorioInmueble;
            _repositorioInquilino = repositorioInquilino;
        }

        // GET: Reservas
        public IActionResult Index(string buscar, int pagina = 1)
        {
            int registrosPorPagina = 10;

            var listado = _repositorioReserva.GetPaginado(buscar, pagina, registrosPorPagina);

            int totalRegistros = _repositorioReserva.ObtenerCantidad(buscar);
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            ViewData["FiltroActual"] = buscar;
            ViewData["PaginaActual"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

            return View(listado);
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
            var inmuebles = _repositorioInmueble.GetParaSelect();
            var inquilinos = _repositorioInquilino.GetParaSelect();

            ViewBag.IdInmueble = new SelectList(inmuebles, "Id", "Direccion");
            ViewBag.IdInquilino = new SelectList(inquilinos, "IdInquilino", "NombreCompleto");
            return View();
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            reserva.CreadoPorUserId = 1;
            ModelState.Remove(nameof(reserva.CreadoPorUserId));

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

            var inmuebles = _repositorioInmueble.GetParaSelect();
            var inquilinos = _repositorioInquilino.GetParaSelect();

            ViewBag.IdInmueble = new SelectList(inmuebles, "Id", "Direccion", reserva.IdInmueble);
            ViewBag.IdInquilino = new SelectList(inquilinos, "IdInquilino", "NombreCompleto", reserva.IdInquilino);

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

            var inmuebles = _repositorioInmueble.GetParaSelect();
            var inquilinos = _repositorioInquilino.GetParaSelect();

            ViewBag.IdInmueble = new SelectList(inmuebles, "Id", "Direccion", reserva.IdInmueble);
            ViewBag.IdInquilino = new SelectList(inquilinos, "IdInquilino", "NombreCompleto", reserva.IdInquilino);
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

            var inmuebles = _repositorioInmueble.GetParaSelect();
            var inquilinos = _repositorioInquilino.GetParaSelect();

            ViewBag.IdInmueble = new SelectList(inmuebles, "Id", "Direccion", reserva.IdInmueble);
            ViewBag.IdInquilino = new SelectList(inquilinos, "IdInquilino", "NombreCompleto", reserva.IdInquilino);
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

        // --- ENDPOINTS AJAX PARA SELECT2 ---

        [HttpGet]
        public IActionResult BuscarInmuebles(string q)
        {
            var inmuebles = _repositorioInmueble.GetParaSelect(q, 20);
            var resultado = inmuebles.Select(i => new
            {
                id = i.Id,
                direccion = i.Direccion
            });
            return Json(resultado);
        }

        [HttpGet]
        public IActionResult BuscarInquilinos(string q)
        {
            var inquilinos = _repositorioInquilino.GetParaSelect(q, 20);
            var resultado = inquilinos.Select(i => new
            {
                idInquilino = i.IdInquilino,
                nombre = i.Nombre,
                apellido = i.Apellido,
                dni = i.Dni
            });
            return Json(resultado);
        }
    }
}