using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
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
                    inmueble.Foto_portada = await GuardarArchivoAsync(archivoPortada, environment);
                }

                // Galería de fotos
                if (archivosGaleria != null && archivosGaleria.Any())
                {
                    var listaRutas = new List<string>();
                    foreach (var foto in archivosGaleria)
                    {
                        var ruta = await GuardarArchivoAsync(foto, environment);
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
                    BorrarArchivoFisico(inmuebleExistente.Foto_portada, environment);
                    inmueble.Foto_portada = await GuardarArchivoAsync(archivoPortada, environment);
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
                        var ruta = await GuardarArchivoAsync(foto, environment);
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

        public static async Task<string> GuardarArchivoAsync(IFormFile archivo, IWebHostEnvironment environment, string subcarpeta = "Inmuebles")
        {
            if (archivo == null || archivo.Length == 0)
                return string.Empty;

            // Ruta de la carpeta wwwroot/Uploads/Inmuebles
            string uploadsFolder = Path.Combine(environment.WebRootPath, "Uploads", subcarpeta);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            string rutaCompleta = Path.Combine(uploadsFolder, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // devuelve la ruta relativa para guardar en BD
            return $"/Uploads/{subcarpeta}/{nombreArchivo}";
        }

        public static void BorrarArchivoFisico(string urlRelativa, IWebHostEnvironment environment)
        {
            if (string.IsNullOrEmpty(urlRelativa)) return;

            // Convierte la URL relativa a ruta física en disco
            string rutaFisica = Path.Combine(environment.WebRootPath, urlRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }

        // Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
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
    }
}