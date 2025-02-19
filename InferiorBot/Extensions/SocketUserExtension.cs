using Discord.WebSocket;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InferiorBot.Extensions
{
    public static class SocketUserExtension
    {
        public static async Task<User> GetUserDataAsync(this SocketUser user, InferiorBotContext context, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var userData = await context.Users.Include(x => x.UserStat).Include(x => x.UserCooldown)
                .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
            if (userData == null)
            {
                userData = new User
                {
                    UserId = user.Id,
                    Balance = configuration.GetValue<decimal>("Settings:StartingBalance"),
                    Banned = false
                };
                await context.Users.AddAsync(userData, cancellationToken);
                if (!context.ChangeTracker.HasChanges()) throw new ApplicationException("Failed to add user to database.");
            }
            if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync(cancellationToken);

            userData.UserStat ??=
                await context.UserStats.FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken) ??
                new UserStat();

            userData.UserCooldown ??=
                await context.UserCooldowns.FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken) ??
                new UserCooldown();

            await context.Entry(userData).ReloadAsync(cancellationToken);
            return userData;
        }

        public static string GetUserName(this SocketUser user)
        {
            return $"{user.Username}{(user.Discriminator != "0000" ? $"#{user.Discriminator}" : string.Empty)}";
        }
    }
}
