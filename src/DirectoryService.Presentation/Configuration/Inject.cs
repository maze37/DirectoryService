using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Configuration;

public static class Inject
{
    public static IServiceCollection ConfigureApp(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddInfrastructure(configuration)
            .AddApplication()
            .AddSwaggerGen()
            .AddEndpointsApiExplorer()
            .AddControllers();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }
}