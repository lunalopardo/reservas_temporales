using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
    [Authorize]
    public class PropietariosController(RepositorioPropietario repo) : Controller
    {
        // GET: Propietarios
        public IActionResult Index(string buscar, int pagina = 1)
        {
            int registrosPorPagina = 5;

            var listado = repo.GetPaginado(buscar, pagina, registrosPorPagina);

            int totalRegistros = repo.ObtenerCantidad(buscar);
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            ViewData["FiltroActual"] = buscar;
            ViewData["PaginaActual"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas;

            return View(listado);
        }

        // GET: Propietarios/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var propietario = repo.GetById(id.Value);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create() => View();

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Propietario propietario)
        {
            if (repo.ExistsEmail(propietario.Email))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
            }

            if (repo.ExistsDNI(propietario.Dni))
            {
                ModelState.AddModelError("Dni", "Este DNI ya se encuentra registrado.");
            }


            if (ModelState.IsValid)
            {
                repo.Create(propietario);
                return RedirectToAction(nameof(Index));
            }

            return View(propietario);
        }

        // GET: Propietarios/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var propietario = repo.GetById(id.Value);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Propietario propietario)
        {
            if (id != propietario.IdPropietario) return NotFound();

            if (repo.ExistsEmail(propietario.Email, id))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado por otro propietario.");
            }

            if (repo.ExistsDNI(propietario.Dni))
            {
                ModelState.AddModelError("Dni", "Este DNI ya se encuentra registrado.");
            }

            if (ModelState.IsValid)
            {
                repo.Update(propietario);
                return RedirectToAction(nameof(Index));
            }

            return View(propietario);
        }

        // GET: Propietarios/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var propietario = repo.GetById(id.Value);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.DeleteLogico(id);
            return RedirectToAction(nameof(Index));
        }


        // GET: Propietarios/Buscar/texto
        [HttpGet("Propietarios/Buscar/{q?}")]
        public IActionResult Buscar(string q = "")
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new { datos = new List<object>() });
            }

            var propietarios = repo.GetPaginado(q, 1, 20);

            var resultados = propietarios.Select(p => new
            {
                idPropietario = p.IdPropietario,
                texto = p.ToString()
            });

            return Json(new { datos = resultados });
        }
    }
}