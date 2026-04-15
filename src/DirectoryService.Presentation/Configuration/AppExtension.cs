using DirectoryService.Presentation.Middlewares;

namespace DirectoryService.Presentation.Configuration;

public static class AppExtension 
{
    public static WebApplication ConfigureExtensions(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}