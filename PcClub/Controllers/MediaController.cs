using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcClub.Data;

namespace PcClub.Controllers;

public class MediaController(ClubDbContext db) : Controller
{
    [HttpGet("/media/{id:guid}")]
    public async Task<IActionResult> Image(Guid id)
    {
        if (db is not CloudDbContext cloud) return NotFound();
        var image = await cloud.StoredImages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (image is null) return NotFound();
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        return File(image.Content, image.ContentType);
    }
}
