using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers;

public class PagosController : Controller
{
    private readonly RepositorioPago _repositorioPago;
    private readonly RepositorioReserva _repositorioReserva;

    public PagosController(RepositorioPago repositorioPago, RepositorioReserva repositorioReserva)
    {
        _repositorioPago = repositorioPago;
        _repositorioReserva = repositorioReserva;
    }

    // GET: Pagos
    public IActionResult Index(string? buscar, int paginaNro = 1)
    {
        int tamPagina = 10;
        var pagos = _repositorioPago.GetPaginado(buscar, paginaNro, tamPagina);
        int totalRegistros = _repositorioPago.ObtenerCantidad(buscar);

        ViewBag.Buscar = buscar;
        ViewBag.PaginaActual = paginaNro;
        ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamPagina);

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
        // Asignación hardcodeada del usuario creador (hasta que implemente la autenticación)
        pago.CreadoPorUserId = 1;

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

        _repositorioPago.AnularLogico(id);
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