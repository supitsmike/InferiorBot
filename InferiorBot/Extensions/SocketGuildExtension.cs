﻿using Discord.WebSocket;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Extensions
{
    public static class SocketGuildExtension
    {
        public static async Task<Guild> GetGuildDataAsync(this SocketGuild guild, InferiorBotContext context)
        {
            var guildData = await context.Guilds.FirstOrDefaultAsync(x => x.GuildId == guild.Id);
            if (guildData == null)
            {
                guildData = new Guild
                {
                    GuildId = guild.Id
                };
                await context.Guilds.AddAsync(guildData);
                if (!context.ChangeTracker.HasChanges()) throw new ApplicationException("Failed to add guild to database.");
            }
            if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync();

            await context.Entry(guildData).ReloadAsync();
            return guildData;
        }
    }
}
