using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using MediatR;
using Serilog;

namespace InferiorBot.Handlers
{
    public class InteractionCreatedNotification(DiscordSocketClient client, InteractionService handler, SocketInteraction interaction, InferiorBotContext context, IServiceProvider services) : INotification
    {
        public DiscordSocketClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
        public InteractionService Handler { get; } = handler ?? throw new ArgumentNullException(nameof(handler));
        public SocketInteraction Interaction { get; } = interaction ?? throw new ArgumentNullException(nameof(interaction));
        public InferiorBotContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
        public IServiceProvider Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    }

    public class InteractionCreatedHandler : INotificationHandler<InteractionCreatedNotification>
    {
        public async Task Handle(InteractionCreatedNotification notification, CancellationToken cancellationToken)
        {
            var client = notification.Client;
            var handler = notification.Handler;
            var interaction = notification.Interaction;
            var databaseContext = notification.Context;
            var services = notification.Services;

            var interactionContext = new SocketInteractionContext(client, interaction);
            if (interactionContext.Interaction is ISlashCommandInteraction)
            {
                var guildData = await interactionContext.Guild.GetGuildDataAsync(databaseContext);
                var channel = interactionContext.Guild.Channels.FirstOrDefault(channel => guildData.BotChannels != null && guildData.BotChannels.Contains(channel.Id));
                if (channel != null && channel.Id != interactionContext.Channel.Id)
                {
                    await interaction.RespondAsync($"You can only use bot commands in {DiscordFormatter.Mention(channel)}.", ephemeral: true);
                    return;
                }
            }

            var result = await handler.ExecuteCommandAsync(interactionContext, services);
            if (result.IsSuccess) return;

            switch (interaction)
            {
                case ISlashCommandInteraction slashCommand:
                    Log.Error("Exception occurred during {User}{Discriminator}'s invocation of '{Command}'",
                        interaction.User.Username,
                        interaction.User.Discriminator != "0000" ? $"#{interaction.User.Discriminator}" : string.Empty,
                        slashCommand.Data.Name);
                    break;
                case IComponentInteraction componentInteraction:
                    Log.Error("Exception occurred during {User}{Discriminator}'s invocation of '{Command}'",
                        interaction.User.Username,
                        interaction.User.Discriminator != "0000" ? $"#{interaction.User.Discriminator}" : string.Empty,
                        componentInteraction.Data.CustomId);
                    break;
            }

            await interaction.RespondAsync("Something went wrong.", ephemeral: true).ConfigureAwait(false);
        }
    }
}
