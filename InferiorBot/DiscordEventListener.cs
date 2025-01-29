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
    public class DiscordEventListener(DiscordSocketClient client, InteractionService handler, InferiorBotContext context, IMediator mediator, IServiceProvider services)
    {
        private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;

        public async Task StartAsync()
        {
            client.Log += OnLogAsync;
            client.Ready += OnReadyAsync;
            client.MessageReceived += OnMessageReceivedAsync;
            client.InteractionCreated += OnInteractionCreatedAsync;

            var configuration = services.GetRequiredService<IConfiguration>();
            await client.LoginAsync(TokenType.Bot, configuration.GetValue<string>("DiscordToken"));
            await client.StartAsync();

            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            await client.SetActivityAsync(new Game("you sleep", ActivityType.Watching));
        }

        private Task OnLogAsync(LogMessage message)
        {
            return mediator.Publish(new LogNotification(message), _cancellationToken);
        }

        private Task OnReadyAsync()
        {
            return mediator.Publish(new ReadyNotification(handler, services), _cancellationToken);
        }

        private Task OnMessageReceivedAsync(SocketMessage message)
        {
            return mediator.Publish(new MessageReceivedNotification(message, context), _cancellationToken);
        }

        private Task OnInteractionCreatedAsync(SocketInteraction interaction)
        {
            return mediator.Publish(new InteractionCreatedNotification(client, handler, interaction, context, services), _cancellationToken);
        }
    }
}
