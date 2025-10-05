using Discord;
using Discord.WebSocket;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Handlers
{
    public class MessageReceivedNotification(SocketMessage message, InferiorBotContext context, IServiceProvider services) : INotification
    {
        public SocketMessage Message { get; } = message ?? throw new ArgumentNullException(nameof(message));
        public InferiorBotContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
        public IServiceProvider Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    }

    public class MessageReceivedHandler : INotificationHandler<MessageReceivedNotification>
    {
        public async Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Message is not SocketUserMessage message) return;
            if (message.Author.IsBot) return;
            if (message.Content.IsValidUrl() == false) return;

            var context = notification.Context;
            var services = notification.Services;
            var user = await message.Author.GetUserDataAsync(context, services, cancellationToken);
            if (user.Banned) return;

            var channel = message.Channel as SocketGuildChannel;
            if (channel != null)
            {
                var guildData = await channel.Guild.GetGuildDataAsync(context, cancellationToken);
                if (guildData.ConvertUrls == false) return;

                var guildId = Convert.ToString(channel.Guild.Id);
                var channelId = Convert.ToString(channel.Id);

                var uri = new Uri(message.Content);
                var subdomain = uri.GetSubdomain();
                var host = uri.Host.StartsWith(subdomain, StringComparison.OrdinalIgnoreCase)
                    ? uri.Host[(subdomain.Length > 0 ? subdomain.Length + 1 : 0)..]
                    : uri.Host;

                if (host is "x.com")
                {
                    host = "twitter.com";
                    uri = new UriBuilder(uri)
                    {
                        Host = $"{subdomain}{(subdomain.Length > 0 ? "." : string.Empty)}{host}", Query = string.Empty,
                        Port = -1
                    }.Uri;
                }

                var url = uri.RemoveQuery().ToLower();
                var previousMessage = await context.ConvertedUrls.FirstOrDefaultAsync(x => x.GuildId == guildId && x.ChannelId == channelId && x.OriginalUrl.ToLower() == url, cancellationToken);
                if (previousMessage != null)
                {
                    await message.Channel.SendMessageAsync(previousMessage.UserId == user.UserId
                        ? $"Buddy... You already posted this {DiscordFormatter.Timestamp(previousMessage.DatePosted)}: https://discord.com/channels/{previousMessage.GuildId}/{previousMessage.ChannelId}/{previousMessage.MessageId}"
                        : $"{DiscordFormatter.Mention(message.Author)}, this was already posted {DiscordFormatter.Timestamp(previousMessage.DatePosted)}: https://discord.com/channels/{previousMessage.GuildId}/{previousMessage.ChannelId}/{previousMessage.MessageId}");
                    await message.DeleteAsync();
                    return;
                }
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

            var replyMessage = await message.Channel.SendMessageAsync($"{DiscordFormatter.Mention(message.Author)}: {convertedUrl}", components: components);
            if (message.Channel is SocketDMChannel) return;

            await message.DeleteAsync();
            if (channel != null)
            {
                var newPost = new ConvertedUrl
                {
                    GuildId = Convert.ToString(channel.Guild.Id),
                    ChannelId = Convert.ToString(channel.Id),
                    MessageId = Convert.ToString(replyMessage.Id),
                    UserId = user.UserId,
                    OriginalUrl = new Uri(message.Content).RemoveQuery(),
                    DatePosted = message.Timestamp.DateTime.ToLocalTime()
                };
                await context.ConvertedUrls.AddAsync(newPost, cancellationToken);
                if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
