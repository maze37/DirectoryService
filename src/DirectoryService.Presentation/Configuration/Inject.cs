using DirectoryService.Application;
using DirectoryService.Infrastructure;

namespace DirectoryService.Presentation.Configuration;

public static class Inject
{
    public static IServiceCollection ConfigureApp(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddInfrastructure(configuration)
            .AddApplication();

        services
            .AddControllers();

        return services;
    }
}