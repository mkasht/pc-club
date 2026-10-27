using Npgsql;

namespace PcClub.Data;

public static class CloudConnection
{
    public static string? Read(IConfiguration configuration)
    {
        var value = configuration.GetConnectionString("Cloud") ?? configuration["LAYERO_DATABASE_URL"];
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (!value.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
            return value;
        var uri = new Uri(value);
        var credentials = uri.UserInfo.Split(':', 2);
        return new NpgsqlConnectionStringBuilder
        {

            Host = uri.Host,

            Port = uri.Port > 0 ? uri.Port : 5432,

            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),

            Username = Uri.UnescapeDataString(credentials[0]),

            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : "",

            SslMode = SslMode.VerifyFull,

            RootCertificate = Path.Combine(AppContext.BaseDirectory, "certs", "isrgrootx1.pem"),

            SearchPath = CloudDbContext.Schema,

            MaxPoolSize = 4,

            Timeout = 15,

            CommandTimeout = 30

        }.ConnectionString;
    }
}
