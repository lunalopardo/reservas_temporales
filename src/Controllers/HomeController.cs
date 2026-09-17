using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using src.Models;
using ReservasTemporales.Models;
using ReservasTemporales.Repositories;

namespace src.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly RepositorioTipoInmueble _repositorioTipoInmueble;

    public HomeController(
        ILogger<HomeController> logger,
        RepositorioTipoInmueble repositorioTipoInmueble)
    {
        _logger = logger;
        _repositorioTipoInmueble = repositorioTipoInmueble;
    }

    public IActionResult Index()
    {
        var tipos = _repositorioTipoInmueble.GetAll();
        ViewBag.TiposInmueble = new SelectList(tipos, "Id", "Nombre");

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return View();
        }

        return View(new LoginViewModel());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}