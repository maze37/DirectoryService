using DirectoryService.Presentation.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApp(builder.Configuration);

var app = builder.Build();

app.ConfigureExtensions();

app.UseHttpsRedirection();

app.MapControllers();
app.Run();
