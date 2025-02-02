using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InferiorBot.Classes;
using Infrastructure.InferiorBot;

namespace InferiorBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("setting", "Changes some settings within the bot.")]
    public class GuildSettingsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("set", "Set a guild settings within the bot.")]
        public class SetGuildSettings(InferiorBotContext context) : BaseUserModule(context)
        {
            private readonly InferiorBotContext _context = context;

            [SlashCommand("allow-url-convert", "Allow or disallow the bot to convert social media urls so they embed properly.")]
            public async Task AllowUrlConvertCommand([Summary(description: "Do you want to allow the bot to convert urls?")] bool value)
            {
                if (GuildData.ConvertUrls == value)
                {
                    await RespondAsync($"Automatic URL Convert is already set to `{value}`.", ephemeral: true);
                    return;
                }

                GuildData.ConvertUrls = value;
                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await RespondAsync($"Automatic URL Convert has been set to `{value}`.", ephemeral: true);
            }

            [SlashCommand("bot-channel", "Set the channel you want to use as a bot commands channel.")]
            public async Task AddBotChannelCommand([Summary(description: "The channel you want to use as a bot commands channel.")] SocketChannel channel)
            {
                var botChannels = GuildData.BotChannels.ToList();
                if (botChannels.Contains(channel.Id))
                {
                    await RespondAsync($"Channel {DiscordFormatter.Mention(channel)} is already set as a bot commands channel.", ephemeral: true);
                    return;
                }

                botChannels.Add(channel.Id);
                GuildData.BotChannels = botChannels.ToArray();
                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await RespondAsync($"Channel {DiscordFormatter.Mention(channel)} has been added as a bot commands channel.", ephemeral: true);
            }

            [SlashCommand("dj-role", "Set the role you want to allow to use the music commands.")]
            public async Task AddDjRoleCommand([Summary(description: "The role you want to allow use of music commands.")] SocketRole role)
            {
                var djRoles = GuildData.DjRoles.ToList();
                if (djRoles.Contains(role.Id))
                {
                    await RespondAsync($"Role {DiscordFormatter.Mention(role)} is already set a DJ role", ephemeral: true);
                    return;
                }

                djRoles.Add(role.Id);
                GuildData.DjRoles = djRoles.ToArray();
                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await RespondAsync($"Role {DiscordFormatter.Mention(role)} has been given DJ permission.", ephemeral: true);
            }
        }

        [Group("remove", "Remove a guild settings within the bot.")]
        public class RemoveGuildSettings(InferiorBotContext context) : BaseUserModule(context)
        {
            private readonly InferiorBotContext _context = context;

            [SlashCommand("bot-channel", "Remove the channel you wanted to be used as a bot commands channel.")]
            public async Task RemoveBotChannelCommand([Summary(description: "The channel you want to remove as a bot commands channel.")] SocketChannel? channel = null)
            {
                var botChannels = GuildData.BotChannels.ToList();
                if (botChannels.Count == 0)
                {
                    await RespondAsync("There are currently no bot commands channel set.", ephemeral: true);
                    return;
                }
                if (channel != null && botChannels.Find(x => x == channel.Id) == default)
                {
                    await RespondAsync("This channel is not currently set as a bot commands channel.", ephemeral: true);
                    return;
                }

                if (channel == null) botChannels.Clear();
                else botChannels.Remove(channel.Id);

                GuildData.BotChannels = botChannels.ToArray();
                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await RespondAsync(
                    $"Removed {(channel == null ? "all channels" : DiscordFormatter.Mention(channel))} as a bot commands channel from guild. {(GuildData.BotChannels.Length == 0 ? "Now any channel can be used to do bot commands." : string.Empty)}",
                    ephemeral: true);
            }

            [SlashCommand("dj-role", "Remove the role you wanted to allow to use the music commands.")]
            public async Task RemoveDjRoleCommand([Summary(description: "The role you want to remove DJ permission from.")] SocketRole? role = null)
            {
                var djRoles = GuildData.DjRoles.ToList();
                if (djRoles.Count == 0)
                {
                    await RespondAsync("There are currently no DJ roles set.", ephemeral: true);
                    return;
                }
                if (role != null && djRoles.Find(x => x == role.Id) == default)
                {
                    await RespondAsync("This role is not currently set to have DJ permissions.", ephemeral: true);
                    return;
                }

                if (role == null) djRoles.Clear();
                else djRoles.Remove(role.Id);

                GuildData.DjRoles = djRoles.ToArray();
                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();
                
                await RespondAsync(
                    $"Removed DJ permission form {(role == null ? "all roles" : DiscordFormatter.Mention(role))} in guild. {(GuildData.DjRoles.Length == 0 ? "Now anyone can use the music settings." : string.Empty)}",
                    ephemeral: true);
            }
        }
    }
}
