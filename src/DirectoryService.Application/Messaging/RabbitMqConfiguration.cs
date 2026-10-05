using IntegrationEvents.Files;
using Wolverine;
using Wolverine.RabbitMQ;

namespace DirectoryService.Application.Messaging;

public static class RabbitMqConfiguration
{
    private const string DIRECTORY_ASSET_EVENTS_QUEUE = "directory.asset-events";
    
    public static void ConfigureRabbitMq(this WolverineOptions opts, string connectionString)
    {
        opts.UseRabbitMq(new Uri(connectionString))
            .AutoProvision()
            .EnableWolverineControlQueues()
            .UseQuorumQueues()
            .DeclareExchange(FileEventsRouting.EXCHANGE, exchange =>
            {
                exchange.ExchangeType = ExchangeType.Topic;
                exchange.IsDurable = true;
            });

        opts.ConfigureFileEventsListeners();
    }
    private static void ConfigureFileEventsListeners(this WolverineOptions opts)
    {
        opts.ListenToRabbitQueue(DIRECTORY_ASSET_EVENTS_QUEUE, queue =>
        {
            queue.BindExchange(
                FileEventsRouting.EXCHANGE, 
                FileEventsRouting.RoutingKeys.ALL_ASSET_EVENTS);
        });
    }
}