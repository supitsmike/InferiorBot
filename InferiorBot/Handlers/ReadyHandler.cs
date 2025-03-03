using Discord.Interactions;
using MediatR;

namespace InferiorBot.Handlers
{
    public class ReadyNotification(InteractionService handler, IServiceProvider services) : INotification
    {
        public InteractionService Handler { get; } = handler ?? throw new ArgumentNullException(nameof(handler));
        public IServiceProvider Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    }

    public class ReadyHandler : INotificationHandler<ReadyNotification>
    {
        public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
        {
            var handler = notification.Handler;
            var services = notification.Services;

            await handler.AddModulesAsync(typeof(DiscordBot).Assembly, services);

            await handler.RegisterCommandsGloballyAsync();
        }
    }
}
