using Discord.WebSocket;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Extensions
{
    public static class SocketUserExtension
    {
        public static async Task<User> GetUserDataAsync(this SocketUser user, InferiorBotContext context, CancellationToken cancellationToken = default)
        {
            var userData = await context.Users.FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
            if (userData == null)
            {
                userData = new User
                {
                    UserId = user.Id,
                    Balance = 100,
                    Banned = false
                };
                await context.Users.AddAsync(userData, cancellationToken);
                if (!context.ChangeTracker.HasChanges()) throw new ApplicationException("Failed to add user to database.");
            }
            if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync(cancellationToken);

            await context.Entry(userData).ReloadAsync(cancellationToken);
            return userData;
        }
    }
}
