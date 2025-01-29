using Discord;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Handlers
{
    public class MessageDeletedNotification(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel, InferiorBotContext context) : INotification
    {
        public Cacheable<IMessage, ulong> Message { get; } = message;
        public Cacheable<IMessageChannel, ulong> MessageChannel { get; } = messageChannel;
        public InferiorBotContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
    }


    public class MessageDeletedHandler : INotificationHandler<MessageDeletedNotification>
    {
        public async Task Handle(MessageDeletedNotification notification, CancellationToken cancellationToken)
        {
            var context = notification.Context;
            var message = await notification.Message.GetOrDownloadAsync();
            if (message == null) return;

            var convertSplit = message.Content.Split(": ");
            if (convertSplit.Length != 2) return;
            var url = convertSplit[1];
            if (!url.IsValidUrl()) return;

            var previousPost = await context.ConvertedUrls.FirstOrDefaultAsync(x => x.ChannelId == message.Channel.Id && x.MessageId == message.Id, cancellationToken);
            if (previousPost != null)
            {
                context.ConvertedUrls.Remove(previousPost);
                if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}