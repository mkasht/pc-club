using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PcClub.Models;

namespace PcClub.Data;

public class ClubDbContext : IdentityDbContext
{
    public ClubDbContext(DbContextOptions<ClubDbContext> options) : base(options) { }
    protected ClubDbContext(DbContextOptions options) : base(options) { }
    public DbSet<ServiceCategory> Categories => Set<ServiceCategory>();
    public DbSet<ClubService> Services => Set<ClubService>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ClubService>().Property(x => x.Price).HasPrecision(12, 2);
        builder.Entity<ClubService>().HasOne(x => x.Category).WithMany(x => x.Services)
            .HasForeignKey(x => x.ServiceCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ServiceCategory>().HasIndex(x => x.Name).IsUnique();
    }
}
