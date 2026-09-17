using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
    public class ImagenesController : Controller
    {
        private readonly RepositorioInmueble _repositorioInmueble;

        public ImagenesController(RepositorioInmueble repositorioInmueble)
        {
            _repositorioInmueble = repositorioInmueble;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(
            int idInmueble, 
            string tipoFoto, 
            int posicion = 0, 
            [FromServices] IWebHostEnvironment environment = null!)
        {
            var inmueble = _repositorioInmueble.GetById(idInmueble);
            if (inmueble == null) return NotFound();

            if (tipoFoto == "portada")
            {
                InmueblesController.BorrarArchivoFisico(inmueble.Foto_portada, environment);
                inmueble.Foto_portada = null;
            }
                else if (tipoFoto == "galeria")
            {
                var fotos = string.IsNullOrEmpty(inmueble.Fotos)
                    ? new List<string>()
                    : inmueble.Fotos.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

                if (posicion >= 0 && posicion < fotos.Count)
                {
                    // Eliminar el archivo físico de la subcarpeta Uploads
                    InmueblesController.BorrarArchivoFisico(fotos[posicion], environment);

                    // Quitar de la lista de la BD
                    fotos.RemoveAt(posicion);
                    inmueble.Fotos = string.Join("|", fotos);
                }
            }

            _repositorioInmueble.Update(inmueble);
            return RedirectToAction("Edit", "Inmuebles", new { id = idInmueble });
        }
    }
}