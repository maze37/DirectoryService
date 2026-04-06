using DirectoryService.Application.Abstractions;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure;

public static class Inject
{
    public static IServiceCollection AddInfrastructure
    (
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DirectoryServiceDb");
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(connectionString);
            options.UseLoggerFactory(loggerFactory);
        });
        
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Репозитории
        services.AddScoped<ILocationRepository, LocationRepository>();
        
        return services;
    }
}