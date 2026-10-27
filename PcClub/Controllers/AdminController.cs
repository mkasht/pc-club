using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PcClub.Data;
using PcClub.Models;

namespace PcClub.Controllers;

[Authorize(Roles = "Admin"), Route("admin")]
public class AdminController(ClubDbContext db, IWebHostEnvironment env) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(new CatalogPage(await db.Categories.OrderBy(x => x.SortOrder).ToListAsync(), await db.Services.Include(x => x.Category).OrderBy(x => x.SortOrder).ToListAsync(), null));

    [HttpGet("services/new")]
    public async Task<IActionResult> Create() { await SetCategoriesAsync(); return View("Edit", new ClubService()); }

    [HttpGet("services/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null) return NotFound();
        await SetCategoriesAsync();
        return View(service);
    }

    [HttpPost("services/save/{id:int?}")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Save(int? id, [Bind("Name,Summary,Description,Price,PriceUnit,Specs,SortOrder,ServiceCategoryId")] ClubService form, IFormFile? image)
    {
        var existing = id is null ? new ClubService() : await db.Services.FindAsync(id);
        if (existing is null) return NotFound();
        if (!await db.Categories.AnyAsync(x => x.Id == form.ServiceCategoryId)) ModelState.AddModelError("ServiceCategoryId", "Выберите существующую категорию.");
        string? extension = null;
        if (image is not null)
        {
            if (image.Length is <= 0 or > 4 * 1024 * 1024) ModelState.AddModelError("image", "Выберите изображение размером до 4 МБ.");
            else
            {
                var header = new byte[12];
                await using var input = image.OpenReadStream();
                var read = await input.ReadAsync(header);
                extension = read >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255 ? ".jpg"
                    : read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) ? ".png"
                    : read >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP" ? ".webp" : null;
                if (extension is null) ModelState.AddModelError("image", "Поддерживаются только JPEG, PNG и WebP.");
            }
        }
        if (!ModelState.IsValid)
        {
            form.Id = id ?? 0; form.ImagePath = existing.ImagePath;
            await SetCategoriesAsync(); return View("Edit", form);
        }
        if (image is not null && extension is not null)
        {
            if (db is CloudDbContext cloud)
            {
                await using var memory = new MemoryStream();
                await image.CopyToAsync(memory);
                var stored = new StoredImage { Content = memory.ToArray(), ContentType = extension switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" } };
                cloud.StoredImages.Add(stored);
                existing.ImagePath = "/media/" + stored.Id;
            }
            else
            {
            var filename = Guid.NewGuid().ToString("N") + extension;
            var folder = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);
            await using var output = System.IO.File.Create(Path.Combine(folder, filename));
            await image.CopyToAsync(output);
            existing.ImagePath = "/uploads/" + filename;
            }
        }
        existing.Name = form.Name.Trim(); existing.Summary = form.Summary.Trim(); existing.Description = form.Description.Trim();
        existing.Price = form.Price; existing.PriceUnit = form.PriceUnit.Trim(); existing.Specs = form.Specs.Trim();
        existing.SortOrder = form.SortOrder; existing.ServiceCategoryId = form.ServiceCategoryId;
        if (id is null) db.Services.Add(existing);
        await db.SaveChangesAsync();
        TempData["Notice"] = "Услуга сохранена. Каталог и API обновлены.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("services/{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null) return NotFound();
        db.Services.Remove(service); await db.SaveChangesAsync();
        TempData["Notice"] = "Услуга удалена."; return RedirectToAction(nameof(Index));
    }

    [HttpPost("categories/save/{id:int?}")]
    public async Task<IActionResult> SaveCategory(int? id, [Bind("Name,SortOrder")] ServiceCategory form)
    {
        if (ModelState.IsValid && await db.Categories.AnyAsync(x => x.Name == form.Name.Trim() && x.Id != (id ?? 0))) ModelState.AddModelError("Name", "Название уже используется.");
        if (!ModelState.IsValid) { TempData["Notice"] = "Категория не сохранена. Укажите уникальное название (до 100 символов) и порядок от 0 до 9999."; return RedirectToAction(nameof(Index)); }
        var category = id is null ? new ServiceCategory() : await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        category.Name = form.Name.Trim(); category.SortOrder = form.SortOrder;
        if (id is null) db.Categories.Add(category);
        try { await db.SaveChangesAsync(); TempData["Notice"] = "Категория сохранена."; }
        catch (DbUpdateException) { TempData["Notice"] = "Не удалось сохранить категорию. Проверьте уникальность названия."; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categories/{id:int}/delete")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        if (await db.Services.AnyAsync(x => x.ServiceCategoryId == id)) TempData["Notice"] = "Сначала перенесите или удалите услуги этой категории.";
        else
        {
            db.Categories.Remove(category);
            try { await db.SaveChangesAsync(); TempData["Notice"] = "Категория удалена."; }
            catch (DbUpdateException) { TempData["Notice"] = "Категория используется услугами и не может быть удалена."; }
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task SetCategoriesAsync() => ViewBag.Categories = new SelectList(await db.Categories.OrderBy(x => x.SortOrder).ToListAsync(), "Id", "Name");
}
