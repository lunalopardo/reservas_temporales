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

    public PagosController(RepositorioPago repositorioPago, RepositorioReserva repositorioReserva)
    {
        _repositorioPago = repositorioPago;
        _repositorioReserva = repositorioReserva;
    }

    private int ObtenerUsuarioIdActual()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("Id")?.Value
                      ?? User.FindFirst("UserId")?.Value;

        return int.TryParse(idClaim, out int id) ? id : 0;
    }

    // GET: Pagos
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        int tamPagina = 10;
        var pagos = _repositorioPago.GetPaginado(buscar, pagina, tamPagina);
        int totalRegistros = _repositorioPago.ObtenerCantidad(buscar);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = (int)Math.Ceiling((double)totalRegistros / tamPagina);

        return View(pagos);
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

        // Si tu método AnularLogico acepta el id del usuario que anula:
        // _repositorioPago.AnularLogico(id, usuarioId);
        // De lo contrario, si solo recibe id, actualizamos directamente o llamamos al overload:
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
}