using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly RepositorioReserva _repositorioReserva;
        private readonly RepositorioInmueble _repositorioInmueble;
        private readonly RepositorioInquilino _repositorioInquilino;
        private readonly RepositorioPago _repositorioPago;

        public ReservasController(
                RepositorioReserva repositorioReserva,
                RepositorioInmueble repositorioInmueble,
                RepositorioInquilino repositorioInquilino,
                RepositorioPago repositorioPago)
        {
            _repositorioReserva = repositorioReserva;
            _repositorioInmueble = repositorioInmueble;
            _repositorioInquilino = repositorioInquilino;
            _repositorioPago = repositorioPago;
        }

        private int ObtenerUsuarioIdActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("Id")?.Value 
                          ?? User.FindFirst("UserId")?.Value;

            return int.TryParse(idClaim, out int id) ? id : 0;
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
        public IActionResult Create(int? idInmueble)
        {
            CargarCombos(idInmueble);

            var reserva = new Reserva();
            if (idInmueble.HasValue)
            {
                reserva.IdInmueble = idInmueble.Value;
            }

            return View(reserva);
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            // Asignar id del usuario autenticado actual
            reserva.CreadoPorUserId = ObtenerUsuarioIdActual();
            ModelState.Remove(nameof(reserva.CreadoPorUserId));

            var inmueble = _repositorioInmueble.GetById(reserva.IdInmueble);

            if (inmueble != null && reserva.FechaHasta > reserva.FechaDesde)
            {
                // Matemática del monto diario
                reserva.MontoDiario = CalcularMontoDiarioFinal(reserva.FechaDesde, reserva.FechaHasta, inmueble.Precio, inmueble.PorcentajeSena);

                ModelState.Remove(nameof(reserva.MontoDiario));
            }
            else if (inmueble == null)
            {
                ModelState.AddModelError("IdInmueble", "El inmueble seleccionado no existe.");
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

            CargarCombos(reserva.IdInmueble, reserva.IdInquilino);
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

            CargarCombos(reserva.IdInmueble, reserva.IdInquilino);
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

            CargarCombos(reserva.IdInmueble, reserva.IdInquilino);
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
            var inmueble = _repositorioInmueble.GetById(id);
            if (inmueble == null) return Json(null);

            return Json(new
            {
                precio = inmueble.Precio,
                porcentajeSena = inmueble.PorcentajeSena
            });
        }

        [HttpGet]
        public IActionResult GetFechasReservadas(int idInmueble)
        {
            var fechas = _repositorioReserva.ObtenerFechasReservadasPorInmueble(idInmueble);
            return Json(fechas);
        }

        // FINALIZACIÓN TEMPRANA + MULTAS
        private (decimal montoMulta, decimal porcentaje, int diasTranscurridos, int totalDias, decimal costoTotalOriginal)? ObtenerCalculoMulta(int idReserva, DateTime fechaTerminacion)
        {
            var reserva = _repositorioReserva.GetById(idReserva);
            if (reserva == null) return null;

            var inmueble = _repositorioInmueble.GetById(reserva.IdInmueble);
            if (inmueble == null) return null;

            int totalDiasOriginales = (reserva.FechaHasta - reserva.FechaDesde).Days;
            if (totalDiasOriginales <= 0)
            {
                return (0m, 0m, 0, 0, 0m);
            }

            int diasTranscurridos = (fechaTerminacion - reserva.FechaDesde).Days;
            if (diasTranscurridos < 0) diasTranscurridos = 0;

            decimal costoTotalOriginal = totalDiasOriginales * inmueble.Precio;
            bool esMenosDeLaMitad = diasTranscurridos < (totalDiasOriginales / 2.0);
            decimal porcentajeAplicado = esMenosDeLaMitad ? 50m : 25m;
            decimal montoMulta = costoTotalOriginal * (porcentajeAplicado / 100m);

            return (montoMulta, porcentajeAplicado, diasTranscurridos, totalDiasOriginales, costoTotalOriginal);
        }

        // Cálculo preliminar para la vista
        [HttpGet]
        public IActionResult CalcularMultaAnticipada(int idReserva, DateTime fTerminacion)
        {
            var calculo = ObtenerCalculoMulta(idReserva, fTerminacion);
            if (calculo == null) return NotFound();

            var res = calculo.Value;
            return Json(new
            {
                montoMulta = res.montoMulta,
                porcentaje = res.porcentaje,
                diasTranscurridos = res.diasTranscurridos,
                totalDias = res.totalDias,
                costoTotalOriginal = res.costoTotalOriginal
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TerminarAnticipadamente(int idReserva, DateTime fechaTerminacion)
        {
            var calculo = ObtenerCalculoMulta(idReserva, fechaTerminacion);
            if (calculo == null) return NotFound();

            var res = calculo.Value;
            int usuarioId = ObtenerUsuarioIdActual();

            // Registrar en reserva la terminación anticipada enviando el usuarioId logueado
            _repositorioReserva.FinalizarAnticipadamente(idReserva, fechaTerminacion, res.montoMulta, usuarioId);

            // Registrar el pago correspondiente asignando también el usuarioId
            var pagoMulta = new Pago
            {
                IdReserva = idReserva,
                Importe = res.montoMulta,
                FechaPago = DateTime.Now,
                Concepto = $"Multa por terminación anticipada ({res.porcentaje}%)",
                CreadoPorUserId = usuarioId
            };
            _repositorioPago.Create(pagoMulta);

            TempData["Success"] = "La reserva fue terminada anticipadamente y la multa registrada en Pagos.";
            return RedirectToAction(nameof(Index));
        }

        // --- ENDPOINTS AJAX PARA SELECT2 ---

        [HttpGet]
        public IActionResult BuscarInmuebles(string q)
        {
            var inmuebles = _repositorioInmueble.GetParaSelect(q, 20);
            var resultado = inmuebles.Select(i => new
            {
                id = i.Id,
                text = i.Direccion
            });
            return Json(resultado);
        }

        [HttpGet]
        public IActionResult BuscarInquilinos(string q)
        {
            var inquilinos = _repositorioInquilino.GetParaSelect(q, 20);
            var resultado = inquilinos.Select(i => new
            {
                id = i.IdInquilino,
                text = $"{i.Apellido}, {i.Nombre} (DNI: {i.Dni})"
            });
            return Json(resultado);
        }

        // --- MÉTODOS AUXILIARES Y DE CÁLCULO ---

        private decimal CalcularMontoDiarioFinal(DateTime fechaDesde, DateTime fechaHasta, decimal precioInmueble, decimal porcentajeSena)
        {
            int dias = (fechaHasta - fechaDesde).Days;
            if (dias <= 0) return 0m;

            decimal precioTotalOriginal = dias * precioInmueble;
            decimal montoSena = precioTotalOriginal * (porcentajeSena / 100m);

            return (precioTotalOriginal - montoSena) / dias;
        }

        private void CargarCombos(int? idInmuebleSeleccionado = null, int? idInquilinoSeleccionado = null)
        {
            var inmuebles = _repositorioInmueble.GetParaSelect();

            if (idInmuebleSeleccionado.HasValue && !inmuebles.Any(i => i.Id == idInmuebleSeleccionado.Value))
            {
                var seleccionado = _repositorioInmueble.GetById(idInmuebleSeleccionado.Value);
                if (seleccionado != null)
                {
                    inmuebles.Add(seleccionado);
                }
            }

            var inquilinos = _repositorioInquilino.GetParaSelect();
            if (idInquilinoSeleccionado.HasValue && !inquilinos.Any(i => i.IdInquilino == idInquilinoSeleccionado.Value))
            {
                var seleccionadoInquilino = _repositorioInquilino.GetById(idInquilinoSeleccionado.Value);
                if (seleccionadoInquilino != null)
                {
                    inquilinos.Add(seleccionadoInquilino);
                }
            }

            ViewBag.IdInmueble = new SelectList(inmuebles, "Id", "Direccion", idInmuebleSeleccionado);
            ViewBag.IdInquilino = new SelectList(inquilinos, "IdInquilino", "NombreCompleto", idInquilinoSeleccionado);
        }
    }
}