using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservasTemporales.Data;
using ReservasTemporales.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ReservasTemporales.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InmueblesController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region GET Actions

        // GET: Inmuebles
        public async Task<IActionResult> Index(string buscar, int pagina = 1)
        {
            int registrosPorPagina = 5;

            var query = _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.Reservas)
                .Where(i => i.Activo);

            if (!string.IsNullOrEmpty(buscar))
            {
                buscar = buscar.Trim();
                var terminos = buscar.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var termino in terminos)
                {
                    var t = termino;
                    query = query.Where(i => i.Direccion.Contains(t) ||
                                            i.Tipo.ToString().Contains(t) ||
                                            i.Propietario!.Nombre.Contains(t) ||
                                            i.Propietario!.Apellido.Contains(t) ||
                                            (i.Propietario!.Nombre + " " + i.Propietario!.Apellido).Contains(t));
                }
            }

            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            pagina = Math.Max(1, Math.Min(pagina, totalPaginas > 0 ? totalPaginas : 1));

            var listado = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            ViewData["FiltroActual"] = buscar;
            ViewData["PaginaActual"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas;

            return View(listado);
        }

        // GET: Inmuebles/id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.Reservas)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public async Task<IActionResult> Create()
        {
            await CargarPropietariosSelectAsync();
            return View();
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _context.Inmuebles.FindAsync(id);

            if (inmueble == null) return NotFound();

            await CargarPropietariosSelectAsync(inmueble.IdPropietario);
            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var inmueble = await _context.Inmuebles
                .Include(i => i.Propietario)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        #endregion

        #region POST Actions

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble inmueble, IFormFile? archivoPortada, List<IFormFile>? archivosGaleria)
        {
            if (ModelState.IsValid)
            {
                // Procesar Portada
                if (archivoPortada != null && archivoPortada.Length > 0)
                {
                    inmueble.Foto_portada = await ImagenesHelper.ProcesarImagenToBase64Async(archivoPortada);
                }

                // Procesar Galería (concatenando con '|')
                if (archivosGaleria != null && archivosGaleria.Any())
                {
                    var listaBase64 = new List<string>();
                    foreach (var foto in archivosGaleria)
                    {
                        if (foto.Length > 0)
                        {
                            string base64 = await ImagenesHelper.ProcesarImagenToBase64Async(foto);
                            if (!string.IsNullOrEmpty(base64))
                                listaBase64.Add(base64);
                        }
                    }
                    inmueble.Fotos = string.Join("|", listaBase64);
                }

                _context.Add(inmueble);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarPropietariosSelectAsync(inmueble.IdPropietario);
            return View(inmueble);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble inmueble, IFormFile? archivoPortada, List<IFormFile>? archivosGaleria)
        {
            if (id != inmueble.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener la entidad existente de la BD para no perder las imágenes si no se suben nuevas
                    var inmuebleExistente = await _context.Inmuebles.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (inmuebleExistente == null) return NotFound();

                    // Actualizar Portada solo si se adjuntó un nuevo archivo
                    if (archivoPortada != null && archivoPortada.Length > 0)
                    {
                        inmueble.Foto_portada = await ImagenesHelper.ProcesarImagenToBase64Async(archivoPortada);
                    }
                    else
                    {
                        inmueble.Foto_portada = inmuebleExistente.Foto_portada;
                    }

                    // Actualizar Galería solo si se subieron nuevas fotos
                    if (archivosGaleria != null && archivosGaleria.Any(f => f.Length > 0))
                    {
                        var listaBase64 = new List<string>();
                        foreach (var foto in archivosGaleria)
                        {
                            if (foto.Length > 0)
                            {
                                string base64 = await ImagenesHelper.ProcesarImagenToBase64Async(foto);
                                if (!string.IsNullOrEmpty(base64))
                                    listaBase64.Add(base64);
                            }
                        }
                        inmueble.Fotos = string.Join("|", listaBase64);
                    }
                    else
                    {
                        inmueble.Fotos = inmuebleExistente.Fotos;
                    }

                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InmuebleExists(inmueble.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await CargarPropietariosSelectAsync(inmueble.IdPropietario);
            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);

            if (inmueble == null) return NotFound();

            inmueble.Activo = false; // Baja lógica
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Private Helpers

        private bool InmuebleExists(int id)
        {
            return _context.Inmuebles.Any(e => e.Id == id);
        }

        private async Task CargarPropietariosSelectAsync(object? seleccionado = null)
        {
            var propietarios = await _context.Propietarios
                .Where(p => p.Activo)
                .Select(p => new
                {
                    IdPropietario = p.IdPropietario,
                    NombreCompleto = $"{p.Nombre} {p.Apellido}"
                })
                .ToListAsync();

            ViewBag.IdPropietario = new SelectList(propietarios, "IdPropietario", "NombreCompleto", seleccionado);
        }

        #endregion

        #region Helper de Imágenes

        public static class ImagenesHelper
        {
            private const int MAX_ANCHO = 1200;

            public static async Task<string> ProcesarImagenToBase64Async(IFormFile archivo)
            {
                if (archivo == null || archivo.Length == 0)
                    return string.Empty;

                using var inputStream = archivo.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                if (image.Width > MAX_ANCHO)
                {
                    int nuevoAlto = (int)Math.Round((double)(image.Height * MAX_ANCHO) / image.Width);
                    image.Mutate(x => x.Resize(MAX_ANCHO, nuevoAlto));
                }

                var encoder = new JpegEncoder
                {
                    Quality = 80
                };

                using var outputStream = new MemoryStream();
                await image.SaveAsync(outputStream, encoder);

                byte[] bytesProcesados = outputStream.ToArray();
                string base64String = Convert.ToBase64String(bytesProcesados);

                return $"data:image/jpeg;base64,{base64String}";
            }
        }

        #endregion
    }
}