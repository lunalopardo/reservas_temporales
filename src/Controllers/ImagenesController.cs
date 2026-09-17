using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
    public class ImagenesController : Controller
    {
        private readonly RepositorioInmueble _repositorioInmueble;
        private readonly RepositorioUsuario _repositorioUsuario;

        public ImagenesController(
            RepositorioInmueble repositorioInmueble,
            RepositorioUsuario repositorioUsuario)
        {
            _repositorioInmueble = repositorioInmueble;
            _repositorioUsuario = repositorioUsuario;
        }

        // Métodos auxiliares
        public static async Task<string> GuardarArchivoAsync(IFormFile archivo, IWebHostEnvironment environment, string subcarpeta = "Inmuebles")
        {
            if (archivo == null || archivo.Length == 0)
                return string.Empty;

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

            return $"/Uploads/{subcarpeta}/{nombreArchivo}";
        }

        public static void BorrarArchivoFisico(string? urlRelativa, IWebHostEnvironment environment)
        {
            if (string.IsNullOrEmpty(urlRelativa)) return;

            string rutaFisica = Path.Combine(environment.WebRootPath, urlRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }

        // endpoints de eliminación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarInmuebleFoto(
            int idInmueble, 
            string tipoFoto, 
            int posicion = 0, 
            [FromServices] IWebHostEnvironment environment = null!)
        {
            var inmueble = _repositorioInmueble.GetById(idInmueble);
            if (inmueble == null) return NotFound();

            if (tipoFoto == "portada")
            {
                BorrarArchivoFisico(inmueble.Foto_portada, environment);
                inmueble.Foto_portada = null;
            }
            else if (tipoFoto == "galeria")
            {
                var fotos = string.IsNullOrEmpty(inmueble.Fotos)
                    ? new List<string>()
                    : inmueble.Fotos.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (posicion >= 0 && posicion < fotos.Count)
                {
                    BorrarArchivoFisico(fotos[posicion], environment);
                    fotos.RemoveAt(posicion);
                    inmueble.Fotos = string.Join("|", fotos);
                }
            }

            _repositorioInmueble.Update(inmueble);
            return RedirectToAction("Edit", "Inmuebles", new { id = idInmueble });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarAvatarUsuario(
            int idUsuario, 
            [FromServices] IWebHostEnvironment environment = null!)
        {
            var usuario = _repositorioUsuario.GetById(idUsuario);
            if (usuario == null) return NotFound();

            if (!string.IsNullOrEmpty(usuario.Avatar))
            {
                BorrarArchivoFisico(usuario.Avatar, environment);
                usuario.Avatar = null;
                _repositorioUsuario.UpdateCompleto(usuario);
            }

            return RedirectToAction("Edit", "Usuarios", new { id = idUsuario });
        }
    }
}