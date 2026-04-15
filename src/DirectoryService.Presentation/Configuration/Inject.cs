using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class Inject
{
    public static IServiceCollection ConfigureApp(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddSerilogLogging(configuration)
            .AddInfrastructure(configuration)
            .AddApplication();

        services
            .AddControllers();

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", "LessonService"));

        return services;
    }
}