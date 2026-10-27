using Microsoft.AspNetCore.Mvc;
using PcClub.Models;
using PcClub.Services;

namespace PcClub.Controllers;

[Route("services")]
public class ServicesController(ICatalogRepository catalog) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int? categoryId) => View(new CatalogPage(await catalog.CategoriesAsync(), await catalog.ServicesAsync(categoryId), categoryId));
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var service = await catalog.FindAsync(id);
        if (service is null) { Response.StatusCode = 404; return View("~/Views/Home/Message.cshtml", new MessagePage("Зона не найдена", "Возможно, предложение больше не доступно. Посмотрите другие зоны в каталоге.")); }
        return View(service);
    }
}
