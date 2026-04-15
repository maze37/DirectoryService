using DirectoryService.Presentation.Middlewares;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class AppExtension 
{
    public static WebApplication ConfigureExtensions(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        
        // Логирование HTTP запросов
        app.UseSerilogRequestLogging();
        
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}