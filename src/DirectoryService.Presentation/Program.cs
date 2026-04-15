using System.Globalization;
using DirectoryService.Presentation.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    
    var builder = WebApplication.CreateBuilder(args);

    // DI
    builder.Services.ConfigureApp(builder.Configuration);
    
    // Serilog
    builder.Host.UseSerilog();

    var app = builder.Build();

    // Подключение Middlewares
    app.ConfigureExtensions();
    
    // Логирование HTTP запросов
    app.UseSerilogRequestLogging();

    // Регистрация контроллеров
    app.MapControllers();
    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}