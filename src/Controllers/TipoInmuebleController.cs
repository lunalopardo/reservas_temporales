using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers;

public class TipoInmuebleController : Controller
{
    private readonly RepositorioTipoInmueble _repoTipoInmueble;

    public TipoInmuebleController(RepositorioTipoInmueble repoTipoInmueble)
    {
        _repoTipoInmueble = repoTipoInmueble;
    }

    // GET: TipoInmueble
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        int registrosPorPagina = 10;
        var listado = _repoTipoInmueble.GetPaginado(buscar, pagina, registrosPorPagina);

        int totalRegistros = _repoTipoInmueble.ObtenerCantidad(buscar);
        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

        return View(listado);
    }

    // GET: TipoInmueble/Details/5
    public IActionResult Details(int id)
    {
        var tipo = _repoTipoInmueble.GetById(id);
        if (tipo == null)
        {
            TempData["Error"] = "El tipo de inmueble solicitado no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(tipo);
    }

    // GET: TipoInmueble/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TipoInmueble/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TipoInmueble tipoInmueble)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _repoTipoInmueble.Create(tipoInmueble);
                TempData["Mensaje"] = "Tipo de inmueble creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar el tipo de inmueble: " + ex.Message;
            }
        }

        return View(tipoInmueble);
    }

    // GET: TipoInmueble/Edit/5
    public IActionResult Edit(int id)
    {
        var tipo = _repoTipoInmueble.GetById(id);
        if (tipo == null)
        {
            TempData["Error"] = "El tipo de inmueble a editar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(tipo);
    }

    // POST: TipoInmueble/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TipoInmueble tipoInmueble)
    {
        if (id != tipoInmueble.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _repoTipoInmueble.Update(tipoInmueble);
                TempData["Mensaje"] = "Tipo de inmueble actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el tipo de inmueble: " + ex.Message;
            }
        }

        return View(tipoInmueble);
    }

    // GET: TipoInmueble/Delete/5
    public IActionResult Delete(int id)
    {
        var tipo = _repoTipoInmueble.GetById(id);
        if (tipo == null)
        {
            TempData["Error"] = "El tipo de inmueble a eliminar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(tipo);
    }

    // POST: TipoInmueble/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            _repoTipoInmueble.DeleteLogico(id);
            TempData["Mensaje"] = "Tipo de inmueble eliminado exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo eliminar el tipo de inmueble: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}