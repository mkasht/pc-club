using Microsoft.EntityFrameworkCore;
using PcClub.Data;
using PcClub.Models;

namespace PcClub.Services;

public interface ICatalogRepository
{
    Task<IReadOnlyList<ServiceCategory>> CategoriesAsync();
    Task<IReadOnlyList<ClubService>> ServicesAsync(int? categoryId = null);
    Task<ClubService?> FindAsync(int id);
}

public class CatalogRepository(ClubDbContext db) : ICatalogRepository
{
    public async Task<IReadOnlyList<ServiceCategory>> CategoriesAsync() =>
        await db.Categories.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
    public async Task<IReadOnlyList<ClubService>> ServicesAsync(int? categoryId = null) =>
        await db.Services.AsNoTracking().Include(x => x.Category)
            .Where(x => categoryId == null || x.ServiceCategoryId == categoryId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
    public Task<ClubService?> FindAsync(int id) => db.Services.AsNoTracking().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
}
