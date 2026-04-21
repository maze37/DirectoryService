using DirectoryService.Infrastructure;
using DirectoryService.Presentation.Middlewares;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class AppExtension 
{
    public static async Task<WebApplication> ConfigureExtensions(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        
        // Логирование HTTP запросов
        app.UseSerilogRequestLogging();
        
        app.UseSwagger();
        app.UseSwaggerUI();
        
        // Автоматическое применение миграций
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        return app;
    }
}