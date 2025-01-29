using Discord;
using Discord.WebSocket;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using MediatR;

namespace InferiorBot.Handlers
{
    public class MessageReceivedNotification(SocketMessage message, InferiorBotContext context) : INotification
    {
        public SocketMessage Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
        public InferiorBotContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
    }

    public class MessageReceivedHandler : INotificationHandler<MessageReceivedNotification>
    {
        public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Message is not SocketUserMessage message) return;
            if (message.Author.IsBot) return;
            if (message.Content.IsValidUrl() == false) return;
            if (message.Channel is SocketGuildChannel channel)
            {
                var guildData = await channel.Guild.GetGuildDataAsync(context, cancellationToken);
                if (guildData.ConvertUrls == false) return;
            }
            else if (message.Channel is not SocketDMChannel) return;

            var convertedUrl = Methods.ConvertUrl(message.Content, null, out var website);
            if (string.IsNullOrEmpty(convertedUrl)) return;

            var componentBuilder = new ComponentBuilder();
            switch (website)
            {
                case "instagram":
                    componentBuilder
                        .WithButton("Instagram", "convert_url:instagram", ButtonStyle.Secondary)
                        .WithButton("InstaFix", "convert_url:InstaFix", ButtonStyle.Secondary)
                        .WithButton("EmbedEZ", "convert_url:EmbedEZ", ButtonStyle.Secondary)
                        .WithButton("Delete", "delete_message", ButtonStyle.Danger);
                    break;
                case "twitter":
                    componentBuilder
                        .WithButton("Twitter", "convert_url:twitter", ButtonStyle.Secondary)
                        .WithButton("TwitFix", "convert_url:TwitFix", ButtonStyle.Secondary)
                        .WithButton("EmbedEZ", "convert_url:EmbedEZ", ButtonStyle.Secondary)
                        .WithButton("Delete", "delete_message", ButtonStyle.Danger);
                    break;
                case "tiktok":
                    componentBuilder
                        .WithButton("TikTok", "convert_url:tiktok", ButtonStyle.Secondary)
                        .WithButton("vxTiktok", "convert_url:vxTiktok", ButtonStyle.Secondary)
                        .WithButton("EmbedEZ", "convert_url:EmbedEZ", ButtonStyle.Secondary)
                        .WithButton("Delete", "delete_message", ButtonStyle.Danger);
                    break;
                case "reddit":
                    componentBuilder
                        .WithButton("Reddit", "convert_url:reddit", ButtonStyle.Secondary)
                        .WithButton("FixReddit", "convert_url:FixReddit", ButtonStyle.Secondary)
                        .WithButton("EmbedEZ", "convert_url:EmbedEZ", ButtonStyle.Secondary)
                        .WithButton("Delete", "delete_message", ButtonStyle.Danger);
                    break;
            }
            var components = componentBuilder.Build();

            await message.ReplyAsync($"{DiscordFormatter.Mention(message.Author)}: {convertedUrl}", components: components);
            if (message.Channel is SocketGuildChannel) await message.DeleteAsync();
        }
    }
}
