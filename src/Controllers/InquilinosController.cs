using Microsoft.AspNetCore.Mvc;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace ReservasTemporales.Controllers
{
    public class InquilinosController(RepositorioInquilino repo) : Controller
    {
        // GET: Inquilinos (con paginado)
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

        // GET: Inquilinos/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = repo.GetById(id.Value);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create() => View();

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inquilino inquilino)
        {
            if (repo.ExistsEmail(inquilino.Email))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
            }

            if (repo.ExistsDNI(inquilino.Dni))
            {
                ModelState.AddModelError("Dni", "Este DNI ya se encuentra registrado.");
            }

            if (ModelState.IsValid)
            {
                repo.Create(inquilino);
                return RedirectToAction(nameof(Index));
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = repo.GetById(id.Value);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inquilino inquilino)
        {
            if (id != inquilino.IdInquilino) return NotFound();

            if (repo.ExistsEmail(inquilino.Email, id))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado por otro inquilino.");
            }

            if (repo.ExistsDNI(inquilino.Dni))
            {
                ModelState.AddModelError("Dni", "Este DNI ya se encuentra registrado.");
            }

            if (ModelState.IsValid)
            {
                repo.Update(inquilino);
                return RedirectToAction(nameof(Index));
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var inquilino = repo.GetById(id.Value);
            if (inquilino == null) return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.DeleteLogico(id);
            return RedirectToAction(nameof(Index));
        }
    }
}