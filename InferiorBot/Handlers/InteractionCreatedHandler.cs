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
            var user = await interactionContext.User.GetUserDataAsync(databaseContext, services, cancellationToken);
            if (user.Banned)
            {
                await interactionContext.Interaction.RespondAsync("You have been banned from the bot.", ephemeral: true);
                return;
            }

            if (interactionContext.Interaction is ISlashCommandInteraction)
            {
                var guildData = await interactionContext.Guild.GetGuildDataAsync(databaseContext, cancellationToken);
                var channel = interactionContext.Guild.Channels.FirstOrDefault(channel => channel.Id == interactionContext.Channel.Id && guildData.BotChannels.Contains(Convert.ToString(channel.Id)));
                if (channel == null)
                {
                    var botChannels = interactionContext.Guild.Channels.Where(x => guildData.BotChannels.Contains(Convert.ToString(x.Id))).ToList();
                    if (botChannels.Count > 0)
                    {
                        var channelMentions = string.Empty;
                        for (var i = 0; i < botChannels.Count; i++)
                        {
                            var botChannel = botChannels[i];
                            channelMentions += DiscordFormatter.Mention(botChannel);

                            if (i + 1 == botChannels.Count - 1) channelMentions += ", or ";
                            else if (i + 1 != botChannels.Count) channelMentions += ", ";
                        }

                        await interactionContext.Interaction.RespondAsync($"You can only use bot commands in {channelMentions}.", ephemeral: true);
                        return;
                    }
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

            await interaction.RespondAsync("Something went wrong.", ephemeral: true);
        }
    }
}
