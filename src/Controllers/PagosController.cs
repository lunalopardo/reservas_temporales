using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers;

[Authorize]
public class PagosController : Controller
{
    private readonly RepositorioPago _repositorioPago;
    private readonly RepositorioReserva _repositorioReserva;

    private readonly RepositorioInmueble _repositorioInmueble;

    public PagosController(RepositorioPago repositorioPago, RepositorioReserva repositorioReserva, RepositorioInmueble repositorioInmueble)
    {
        _repositorioPago = repositorioPago;
        _repositorioReserva = repositorioReserva;
        _repositorioInmueble = repositorioInmueble;
    }

    private int ObtenerUsuarioIdActual()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("Id")?.Value
                      ?? User.FindFirst("UserId")?.Value;

        return int.TryParse(idClaim, out int id) ? id : 0;
    }

    // GET: Pagos
    public IActionResult Index(string buscar, int pagina = 1, int? idReserva = null)
    {
        int cantidadPorPagina = 10;
        int totalRegistros = 0;
        IEnumerable<Pago> pagos;

        if (idReserva.HasValue)
        {
            var reserva = _repositorioReserva.GetById(idReserva.Value);
            if (reserva == null)
            {
                return NotFound();
            }

            pagos = _repositorioPago.GetPorReserva(idReserva.Value);
            totalRegistros = pagos.Count();
            ViewBag.ReservaFiltro = reserva;
            ViewBag.PagoNuevo = new Pago
            {
                IdReserva = idReserva.Value,
                FechaPago = DateTime.Now
            };
        }
        else
        {
            totalRegistros = _repositorioPago.ObtenerCantidad(buscar);
            pagos = _repositorioPago.GetPaginado(buscar, pagina, cantidadPorPagina);
        }

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = (int)Math.Ceiling((double)totalRegistros / cantidadPorPagina);

        return View(pagos);
    }

    // POST: Pagos/CrearPagoRapido
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CrearPagoRapido(Pago pago)
    {
        pago.CreadoPorUserId = ObtenerUsuarioIdActual();
        ModelState.Remove(nameof(pago.CreadoPorUserId));

        if (ModelState.IsValid)
        {
            _repositorioPago.Create(pago);
            TempData["Mensaje"] = "Pago registrado exitosamente.";
            return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
        }

        TempData["Error"] = "Error al registrar el pago. Verifique los datos ingresados.";
        return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
    }

    // GET: Pagos/Details/5
    public IActionResult Details(int id)
    {
        var pago = _repositorioPago.GetById(id);
        if (pago == null)
        {
            return NotFound();
        }
        return View(pago);
    }

    // GET: Pagos/Create
    public IActionResult Create(int? idReserva)
    {
        var pago = new Pago
        {
            FechaPago = DateTime.Now,
            IdReserva = idReserva ?? 0
        };
        return View(pago);
    }

    // POST: Pagos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Pago pago)
    {
        // Se asigna dinámicamente el usuario logueado
        pago.CreadoPorUserId = ObtenerUsuarioIdActual();
        ModelState.Remove(nameof(pago.CreadoPorUserId));

        if (ModelState.IsValid)
        {
            _repositorioPago.Create(pago);
            TempData["Mensaje"] = "El pago ha sido registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(pago);
    }

    // GET: Pagos/BuscarReservas?q=...
    [HttpGet]
    public IActionResult BuscarReservas(string q)
    {
        var datos = _repositorioReserva.BuscarPorInquilinoODireccion(q ?? "");
        return Json(datos);
    }

    // GET: Pagos/Edit/5
    public IActionResult Edit(int id)
    {
        var pago = _repositorioPago.GetById(id);
        if (pago == null)
        {
            return NotFound();
        }
        return View(pago);
    }

    // POST: Pagos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Pago pago)
    {
        if (id != pago.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _repositorioPago.Update(pago);
            TempData["Mensaje"] = "El pago ha sido actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(pago);
    }

    // POST: Pagos/Delete/5 (Anulación Lógica)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var pago = _repositorioPago.GetById(id);
        if (pago == null)
        {
            return NotFound();
        }

        int usuarioId = ObtenerUsuarioIdActual();

        _repositorioPago.AnularLogico(id, usuarioId);

        TempData["Mensaje"] = "El pago ha sido anulado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // POST: Pagos/Reactivar/5 (Reactivación Lógica)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reactivar(int id)
    {
        var pago = _repositorioPago.GetById(id);
        if (pago == null)
        {
            return NotFound();
        }

        _repositorioPago.ReactivarLogico(id);
        TempData["Mensaje"] = "El pago ha sido reactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // GET: Pagos/PorReserva/5
    public IActionResult PorReserva(int id)
    {
        var reserva = _repositorioReserva.GetById(id);
        if (reserva == null)
        {
            return NotFound();
        }

        var pagos = _repositorioPago.GetPorReserva(id);

        ViewBag.Reserva = reserva;
        ViewBag.PagoNuevo = new Pago
        {
            IdReserva = id,
            FechaPago = DateTime.Now
        };

        return View(pagos);
    }

    // POST: Pagos/PorReserva
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PorReserva(Pago pago)
    {
        pago.CreadoPorUserId = ObtenerUsuarioIdActual();
        ModelState.Remove(nameof(pago.CreadoPorUserId));

        if (ModelState.IsValid)
        {
            _repositorioPago.Create(pago);
            TempData["Mensaje"] = "Pago registrado exitosamente.";
            return RedirectToAction(nameof(PorReserva), new { id = pago.IdReserva });
        }

        var reserva = _repositorioReserva.GetById(pago.IdReserva);
        if (reserva == null)
        {
            return NotFound();
        }

        var pagos = _repositorioPago.GetPorReserva(pago.IdReserva);
        ViewBag.Reserva = reserva;
        ViewBag.PagoNuevo = pago;

        TempData["Error"] = "Por favor, complete correctamente los campos del pago.";
        return View(pagos);
    }

    // Obtenemos montos sugeridos para autocompletar los input de nuevos pagos; para mantener una coherencia
    [HttpGet]
    public IActionResult ObtenerMontosSugeridos(int idReserva)
    {
        var reserva = _repositorioReserva.GetById(idReserva);
        if (reserva == null) return NotFound();

        var inmueble = _repositorioInmueble.GetById(reserva.IdInmueble);

        int dias = (reserva.FechaHasta - reserva.FechaDesde).Days;
        decimal totalEstadia = reserva.MontoDiario * dias;
        decimal porcentajeSena = inmueble?.PorcentajeSena ?? 0;
        decimal montoSena = totalEstadia * (porcentajeSena / 100m);

        return Json(new
        {
            montoDiario = reserva.MontoDiario,
            montoSena = Math.Round(montoSena, 2),
            montoTotal = totalEstadia
        });
    }
}