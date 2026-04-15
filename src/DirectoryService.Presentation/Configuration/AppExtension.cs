using DirectoryService.Presentation.Middlewares;

namespace DirectoryService.Presentation.Configuration;

public static class AppExtension 
{
    public static WebApplication ConfigureExtensions(this WebApplication app)
    {
        app.UseExceptionMiddleware();

        app.UseSwaggerUI();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Directory Service");
        });

        return app;
    }
}