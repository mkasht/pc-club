using Microsoft.AspNetCore.Mvc;
using PcClub.Models;
using PcClub.Services;

namespace PcClub.Controllers;

public record ServiceDto(int Id,
    string Name,
    int CategoryId,
    string Category,
    string Summary,
    string Description,
    decimal Price,
    string PriceUnit,
    string Specs,
    string ImagePath)
{
    public static ServiceDto From(ClubService x) =>
        new(x.Id,
            x.Name,
            x.ServiceCategoryId,
            x.Category?.Name ?? "",
            x.Summary,
            x.Description,
            x.Price,
            x.PriceUnit,
            x.Specs,
            x.ImagePath);
}

[ApiController, Route("api/services")]
public class CatalogApiController(ICatalogRepository catalog) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> Index(int? categoryId) =>
        Ok((await catalog.ServicesAsync(categoryId)).Select(ServiceDto.From));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceDto>> Details(int id) =>
        await catalog.FindAsync(id) is { } service ? Ok(ServiceDto.From(service)) : NotFound();
}
