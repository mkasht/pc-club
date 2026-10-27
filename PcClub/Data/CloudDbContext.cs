using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PcClub.Data;

public class CloudDbContext(DbContextOptions<CloudDbContext> options) : ClubDbContext(options), IDataProtectionKeyContext
{
    public const string Schema = "pcclub";
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    public DbSet<StoredImage> StoredImages => Set<StoredImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);
        builder.Entity<StoredImage>().Property(x => x.ContentType).HasMaxLength(40);
    }
}

public class StoredImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = "image/jpeg";
}

// Design-time only: migration generation does not connect to this placeholder.
public class CloudDbContextFactory : IDesignTimeDbContextFactory<CloudDbContext>
{
    public CloudDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<CloudDbContext>()
        .UseNpgsql("Host=localhost;Database=pcclub_design;Username=design", options => options.MigrationsHistoryTable("__EFMigrationsHistory", CloudDbContext.Schema)).Options);
}
