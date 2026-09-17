using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;


namespace ReservasTemporales.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly RepositorioInmueble _repositorioInmueble;
        private readonly RepositorioPropietario _repositorioPropietario;
        private readonly RepositorioTipoInmueble _repositorioTipoInmueble;

        public InmueblesController(
            RepositorioInmueble repositorioInmueble,
            RepositorioPropietario repositorioPropietario,
            RepositorioTipoInmueble repositorioTipoInmueble)
        {
            _repositorioInmueble = repositorioInmueble;
            _repositorioPropietario = repositorioPropietario;
            _repositorioTipoInmueble = repositorioTipoInmueble;
        }

        // Listar (con paginado)
        public IActionResult Index(string? buscar, int pagina = 1)
        {
            int registrosPorPagina = 10;

            var listado = _repositorioInmueble.GetPaginado(buscar, pagina, registrosPorPagina);

            int totalRegistros = _repositorioInmueble.ObtenerCantidad(buscar);
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            ViewData["FiltroActual"] = buscar;
            ViewData["PaginaActual"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

            return View(listado);
        }

        // Detalle de un inmueble
        public IActionResult Details(int id)
        {
            var inmueble = _repositorioInmueble.GetById(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        // Crear
        public IActionResult Create()
        {
            CargarSelects();
            return View();
        }

        // Editar
        public IActionResult Edit(int id)
        {
            var inmueble = _repositorioInmueble.GetById(id);
            if (inmueble == null) return NotFound();

            CargarSelects(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Inmueble inmueble,
            IFormFile? archivoPortada,
            List<IFormFile>? archivosGaleria,
            [FromServices] IWebHostEnvironment environment)
        {
            if (ModelState.IsValid)
            {
                // Foto de portada
                if (archivoPortada != null && archivoPortada.Length > 0)
                {
                    inmueble.Foto_portada = await ImagenesController.GuardarArchivoAsync(archivoPortada, environment, "Inmuebles");
                }

                // Galería de fotos
                if (archivosGaleria != null && archivosGaleria.Any())
                {
                    var listaRutas = new List<string>();
                    foreach (var foto in archivosGaleria)
                    {
                        var ruta = await ImagenesController.GuardarArchivoAsync(foto, environment, "Inmuebles");
                        if (!string.IsNullOrEmpty(ruta)) listaRutas.Add(ruta);
                    }
                    inmueble.Fotos = string.Join("|", listaRutas);
                }

                _repositorioInmueble.Create(inmueble);
                return RedirectToAction(nameof(Index));
            }

            CargarSelects(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Inmueble inmueble,
            IFormFile? archivoPortada,
            List<IFormFile>? archivosGaleria,
            [FromServices] IWebHostEnvironment environment)
        {
            if (ModelState.IsValid)
            {
                var inmuebleExistente = _repositorioInmueble.GetById(inmueble.Id);
                if (inmuebleExistente == null) return NotFound();

                // Mantener o reemplazar portada
                if (archivoPortada != null && archivoPortada.Length > 0)
                {
                    ImagenesController.BorrarArchivoFisico(inmuebleExistente.Foto_portada, environment);
                    inmueble.Foto_portada = await ImagenesController.GuardarArchivoAsync(archivoPortada, environment, "Inmuebles");
                }
                else
                {
                    inmueble.Foto_portada = inmuebleExistente.Foto_portada;
                }

                // Mantener galería actual y concatenar nuevas
                var listaFotos = string.IsNullOrEmpty(inmuebleExistente.Fotos)
                    ? new List<string>()
                    : inmuebleExistente.Fotos.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (archivosGaleria != null && archivosGaleria.Any())
                {
                    foreach (var foto in archivosGaleria)
                    {
                        var ruta = await ImagenesController.GuardarArchivoAsync(foto, environment, "Inmuebles");
                        if (!string.IsNullOrEmpty(ruta)) listaFotos.Add(ruta);
                    }
                }
                inmueble.Fotos = string.Join("|", listaFotos);

                _repositorioInmueble.Update(inmueble);
                return RedirectToAction(nameof(Index));
            }

            CargarSelects(inmueble.IdPropietario, inmueble.IdTipoInmueble);
            return View(inmueble);
        }
        // Eliminar

        // GET: Inmuebles/Delete/5 (Muestra la vista de confirmación)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var inmueble = _repositorioInmueble.GetById(id);
            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioInmueble.DeleteLogico(id);
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para llenar los select
        private void CargarSelects(int? propietarioSel = null, int? tipoSel = null)
        {
            if (propietarioSel.HasValue && propietarioSel.Value > 0)
            {
                var p = _repositorioPropietario.GetById(propietarioSel.Value);
                if (p != null)
                {
                    var propietarioSeleccionado = new[] {
                        new { IdPropietario = p.IdPropietario, NombreCompleto = $"{p.Nombre} {p.Apellido}" }
                    };
                    ViewBag.IdPropietario = new SelectList(propietarioSeleccionado, "IdPropietario", "NombreCompleto", propietarioSel);
                }
            }
            else
            {
                ViewBag.IdPropietario = new SelectList(new List<SelectListItem>());
            }

            var tipos = _repositorioTipoInmueble.GetAll();
            ViewBag.IdTipoInmueble = new SelectList(tipos, "Id", "Nombre", tipoSel);
        }

        [HttpGet]
        public IActionResult Buscar(BusquedaInmuebleViewModel model)
        {
            // Cargar select de tipos
            ViewBag.TiposInmueble = _repositorioTipoInmueble.GetAll();

            if (model.FechaInicio.HasValue && model.FechaFin.HasValue && model.FechaInicio >= model.FechaFin)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la de inicio.");
                return View(model);
            }

            model.Resultados = _repositorioInmueble.BuscarDisponiblesViewModel(
                model.IdTipoInmueble,
                model.Personas,
                model.FechaInicio,
                model.FechaFin
            );

            return View(model);
        }
    }
}