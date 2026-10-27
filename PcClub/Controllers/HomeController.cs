using Microsoft.AspNetCore.Mvc;
using PcClub.Models;
using PcClub.Services;

namespace PcClub.Controllers;

public class HomeController(ICatalogRepository catalog) : Controller
{
    public async Task<IActionResult> Index() => View((await catalog.ServicesAsync()).Take(3).ToList());
    [HttpGet("/contacts")]
    public IActionResult Contacts() => View();
    [HttpGet("/error")]
    public IActionResult Error() => View("Message", new MessagePage("Что-то пошло не так", "Попробуйте обновить страницу немного позже."));
}
public record MessagePage(string Title, string Text);
