using Discord;
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
            var messageId = notification.Message.Id;
            var channelId = notification.MessageChannel.Id;

            var previousMessage = await context.ConvertedUrls.FirstOrDefaultAsync(x => x.ChannelId == channelId && x.MessageId == messageId, cancellationToken);
            if (previousMessage != null)
            {
                context.ConvertedUrls.Remove(previousMessage);
                if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}