using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices.Cleanup;
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
        
        // Убрать стандартный возврат ответа ошибок от AspNetCore.
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        
        services.Configure<DepartmentsCleanupOptions>(
            configuration.GetSection(DepartmentsCleanupOptions.SectionName));
        
        services.Configure<LocationsCleanupOptions>(
            configuration.GetSection(LocationsCleanupOptions.SectionName));
        
        services.Configure<PositionsCleanupOptions>(
            configuration.GetSection(PositionsCleanupOptions.SectionName));

        return services;
    }
}