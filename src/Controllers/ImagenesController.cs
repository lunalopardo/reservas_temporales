using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ReservasTemporales.Controllers
{
    public class ImagenesController : Controller
    {
        private readonly RepositorioInmueble _repositorioInmueble;

        public ImagenesController(RepositorioInmueble repositorioInmueble)
        {
            _repositorioInmueble = repositorioInmueble;
        }

        public static async Task<string> ProcessBase64Async(IFormFile archivo, int maxAncho = 1200)
        {
            if (archivo == null || archivo.Length == 0)
                return string.Empty;

            using var inputStream = archivo.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            if (image.Width > maxAncho)
            {
                int nuevoAlto = (int)Math.Round((double)(image.Height * maxAncho) / image.Width);
                image.Mutate(x => x.Resize(maxAncho, nuevoAlto));
            }

            var encoder = new JpegEncoder { Quality = 75 };
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, encoder);

            return $"data:image/jpeg;base64,{Convert.ToBase64String(outputStream.ToArray())}";
        }

        // Accion para borrar foto de portada o foto individual de la galeria.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int idInmueble, string tipoFoto, int posicion = 0)
        {
            var inmueble = _repositorioInmueble.GetById(idInmueble);
            if (inmueble == null) return NotFound();

            if (tipoFoto == "portada")
            {
                inmueble.Foto_portada = null;
            }
            else if (tipoFoto == "galeria")
            {
                var fotos = string.IsNullOrEmpty(inmueble.Fotos)
                    ? new List<string>()
                    : inmueble.Fotos.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (posicion >= 0 && posicion < fotos.Count)
                {
                    fotos.RemoveAt(posicion);
                    inmueble.Fotos = string.Join("|", fotos);
                }
            }

            _repositorioInmueble.Update(inmueble);
            return RedirectToAction("Edit", "Inmuebles", new { id = idInmueble });
        }
    }
}