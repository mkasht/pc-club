using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcClub.Data;
using PcClub.Models;
using PcClub.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, log) => log.ReadFrom.Configuration(context.Configuration).WriteTo.Console());
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
var cloudConnection = CloudConnection.Read(builder.Configuration);
var isCloud = !string.IsNullOrWhiteSpace(cloudConnection);
if (isCloud)
{
    builder.Services.AddDbContext<CloudDbContext>(options => options.UseNpgsql(cloudConnection,
        postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", CloudDbContext.Schema)));
    builder.Services.AddScoped<ClubDbContext>(services => services.GetRequiredService<CloudDbContext>());
    var certificate = builder.Configuration["CloudKeyCertificate"] ?? throw new InvalidOperationException("Cloud key certificate is not configured.");
    builder.Services.AddDataProtection().SetApplicationName("PcClub.Layero").PersistKeysToDbContext<CloudDbContext>()
        .ProtectKeysWithCertificate(X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(certificate),
        null,
        X509KeyStorageFlags.EphemeralKeySet));
    builder.Configuration["AllowedHosts"] = "*";
    if (int.TryParse(builder.Configuration["PORT"], out var cloudPort))
        builder.WebHost.UseUrls($"http://0.0.0.0:{cloudPort}");
}
else
    builder.Services.AddDbContext<ClubDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Club")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    }).AddEntityFrameworkStores<ClubDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
    {
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    if (isCloud)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // Keep auth redirects on the public HTTPS origin behind TLS termination.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect(new Uri(context.RedirectUri).PathAndQuery);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect(new Uri(context.RedirectUri).PathAndQuery);
        return Task.CompletedTask;
    };
    });
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.Configure<ClubOptions>(builder.Configuration.GetSection("Club"));
var culture = CultureInfo.GetCultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/error"); app.UseHsts(); }
app.UseSerilogRequestLogging();
app.Use(async (context, next) =>
    {
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
    });
app.UseStaticFiles();
// HTML and authentication redirects depend on the current account.
// Do this before authorization so a guest/user 302 cannot be cached for an admin.
app.Use(async (context, next) =>
    {
    context.Response.Headers.CacheControl = "private, no-store, no-cache, max-age=0";
    context.Response.Headers.Pragma = "no-cache";
    context.Response.Headers.Expires = "0";
    context.Response.Headers["X-Accel-Expires"] = "0";
    context.Response.Headers.Append("Vary", "Cookie");
    await next();
    });
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
await using (var scope = app.Services.CreateAsyncScope()) { await SeedData.InitializeAsync(scope.ServiceProvider); }
await app.RunAsync();
