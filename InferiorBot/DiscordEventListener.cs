using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InferiorBot.Handlers;
using Infrastructure.InferiorBot;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InferiorBot
{
    public class DiscordEventListener(DiscordSocketClient client, InteractionService handler, InferiorBotContext context, IMediator mediator, IServiceProvider services) : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public async Task StartAsync()
        {
            client.Log += OnLogAsync;
            client.Ready += OnReadyAsync;
            client.MessageDeleted += OnMessageDeletedAsync;
            client.MessageReceived += OnMessageReceivedAsync;
            client.InteractionCreated += OnInteractionCreatedAsync;

            var configuration = services.GetRequiredService<IConfiguration>();
            await client.LoginAsync(TokenType.Bot, configuration.GetValue<string>("DiscordToken"));
            await client.StartAsync();

            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            await client.SetActivityAsync(new Discord.Game("you sleep", ActivityType.Watching));
        }

        private Task OnLogAsync(LogMessage message)
        {
            return mediator.Publish(new LogNotification(message), CancellationToken);
        }

        private Task OnReadyAsync()
        {
            return mediator.Publish(new ReadyNotification(handler, services), CancellationToken);
        }

        private Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
        {
            return mediator.Publish(new MessageDeletedNotification(message, messageChannel, context), CancellationToken);
        }

        private Task OnMessageReceivedAsync(SocketMessage message)
        {
            return mediator.Publish(new MessageReceivedNotification(message, context, services), CancellationToken);
        }

        private Task OnInteractionCreatedAsync(SocketInteraction interaction)
        {
            return mediator.Publish(new InteractionCreatedNotification(client, handler, interaction, context, services), CancellationToken);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
