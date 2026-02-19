using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ServiceAxis.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for <see cref="ServiceAxisDbContext"/>.
/// Used exclusively by EF Core CLI tools (migrations add, database update, etc.)
/// so they never trigger the application startup pipeline.
/// </summary>
public sealed class ServiceAxisDbContextFactory : IDesignTimeDbContextFactory<ServiceAxisDbContext>
{
    public ServiceAxisDbContext CreateDbContext(string[] args)
    {
        // Walk up to the solution root and read the API appsettings
        var solutionRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "src", "ServiceAxis.API");

        // Fallback: try current dir first (in case the tool is run from the Infrastructure project)
        var apiPath = Directory.Exists(solutionRoot)
            ? solutionRoot
            : Path.Combine(Directory.GetCurrentDirectory(), "..", "ServiceAxis.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetFullPath(apiPath))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ServiceAxisDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(typeof(ServiceAxisDbContext).Assembly.FullName));

        return new ServiceAxisDbContext(optionsBuilder.Options);
    }
}
