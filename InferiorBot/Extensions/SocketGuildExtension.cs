using Discord.WebSocket;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Extensions
{
    public static class SocketGuildExtension
    {
        public static async Task<Guild> GetGuildDataAsync(this SocketGuild guild, InferiorBotContext context, CancellationToken cancellationToken = default)
        {
            var guildId = Convert.ToString(guild.Id);
            var guildData = await context.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);
            if (guildData == null)
            {
                guildData = new Guild
                {
                    GuildId = guildId,
                    BotChannels = [],
                    DjRoles = [],
                    ConvertUrls = false
                };
                await context.Guilds.AddAsync(guildData, cancellationToken);
                if (!context.ChangeTracker.HasChanges()) throw new ApplicationException("Failed to add guild to database.");
            }
            if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync(cancellationToken);

            await context.Entry(guildData).ReloadAsync(cancellationToken);
            return guildData;
        }
    }
}
