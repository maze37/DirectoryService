using System.Reflection;
using Core.Abstractions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Services;
using FileService.Contracts.HttpCommunication;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class Inject
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(
                    typeof(ICommandHandler<,>),
                    typeof(ICommandHandler<>)
                ))
            .AsSelfWithInterfaces()
            .WithTransientLifetime());

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        services.AddValidatorsFromAssembly(typeof(Inject).Assembly);
        
        services.AddFileServiceHttpCommunication(configuration);
        
        services.AddScoped<ILocationMediaEnrichmentService, LocationMediaEnrichmentService>();
        
        return services;
    }
}