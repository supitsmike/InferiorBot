using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InferiorBot.Attributes;
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
        public class SetGuildSettings(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
        {
            private readonly InferiorBotContext _context = context;

            [Defer(true)]
            [SlashCommand("allow-url-convert", "Allow or disallow the bot to convert social media urls so they embed properly.")]
            public async Task AllowUrlConvert([Summary(description: "Do you want to allow the bot to convert urls?")] bool value)
            {
                if (GuildData.ConvertUrls == value)
                {
                    await FollowupAsync($"Automatic URL Convert is already set to `{value}`.", ephemeral: true);
                    return;
                }

                GuildData.ConvertUrls = value;
                if (!_context.ChangeTracker.HasChanges())
                {
                    await FollowupAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await FollowupAsync($"Automatic URL Convert has been set to `{value}`.", ephemeral: true);
            }

            [Defer(true)]
            [SlashCommand("bot-channel", "Set the channel you want to use as a bot commands channel.")]
            public async Task AddBotChannel([Summary(description: "The channel you want to use as a bot commands channel.")] SocketChannel channel)
            {
                var channelId = Convert.ToString(channel.Id);
                if (GuildData.BotChannels.Contains(channelId))
                {
                    await FollowupAsync($"Channel {DiscordFormatter.Mention(channel)} is already set as a bot commands channel.", ephemeral: true);
                    return;
                }

                GuildData.BotChannels.Add(channelId);
                if (!_context.ChangeTracker.HasChanges())
                {
                    await FollowupAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await FollowupAsync($"Channel {DiscordFormatter.Mention(channel)} has been added as a bot commands channel.", ephemeral: true);
            }

            [Defer(true)]
            [SlashCommand("dj-role", "Set the role you want to allow to use the music commands.")]
            public async Task AddDjRole([Summary(description: "The role you want to allow use of music commands.")] SocketRole role)
            {
                var roleId = Convert.ToString(role.Id);
                if (GuildData.DjRoles.Contains(roleId))
                {
                    await FollowupAsync($"Role {DiscordFormatter.Mention(role)} is already set a DJ role", ephemeral: true);
                    return;
                }

                GuildData.DjRoles.Add(roleId);
                if (!_context.ChangeTracker.HasChanges())
                {
                    await FollowupAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await FollowupAsync($"Role {DiscordFormatter.Mention(role)} has been given DJ permission.", ephemeral: true);
            }
        }

        [Group("remove", "Remove a guild settings within the bot.")]
        public class RemoveGuildSettings(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
        {
            private readonly InferiorBotContext _context = context;

            [Defer(true)]
            [SlashCommand("bot-channel", "Remove the channel you wanted to be used as a bot commands channel.")]
            public async Task RemoveBotChannel([Summary(description: "The channel you want to remove as a bot commands channel.")] SocketChannel? channel = null)
            {
                if (GuildData.BotChannels.Count == 0)
                {
                    await FollowupAsync("There are currently no bot commands channel set.", ephemeral: true);
                    return;
                }

                var channelId = Convert.ToString(channel?.Id);
                if (channel != null && GuildData.BotChannels.Find(x => x == channelId) == null)
                {
                    await FollowupAsync("This channel is not currently set as a bot commands channel.", ephemeral: true);
                    return;
                }

                if (channel == null) GuildData.BotChannels.Clear();
                else GuildData.BotChannels.Remove(channelId);

                if (!_context.ChangeTracker.HasChanges())
                {
                    await FollowupAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await FollowupAsync(
                    $"Removed {(channel == null ? "all channels" : DiscordFormatter.Mention(channel))} as a bot commands channel from guild. {(GuildData.BotChannels.Count == 0 ? "Now any channel can be used to do bot commands." : string.Empty)}",
                    ephemeral: true);
            }

            [Defer(true)]
            [SlashCommand("dj-role", "Remove the role you wanted to allow to use the music commands.")]
            public async Task RemoveDjRole([Summary(description: "The role you want to remove DJ permission from.")] SocketRole? role = null)
            {
                if (GuildData.DjRoles.Count == 0)
                {
                    await FollowupAsync("There are currently no DJ roles set.", ephemeral: true);
                    return;
                }

                var roleId = Convert.ToString(role?.Id);
                if (role != null && GuildData.DjRoles.Find(x => x == roleId) == null)
                {
                    await FollowupAsync("This role is not currently set to have DJ permissions.", ephemeral: true);
                    return;
                }

                if (role == null) GuildData.DjRoles.Clear();
                else GuildData.DjRoles.Remove(roleId);

                if (!_context.ChangeTracker.HasChanges())
                {
                    await FollowupAsync("Failed to update guild settings.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();
                
                await FollowupAsync(
                    $"Removed DJ permission form {(role == null ? "all roles" : DiscordFormatter.Mention(role))} in guild. {(GuildData.DjRoles.Count == 0 ? "Now anyone can use the music settings." : string.Empty)}",
                    ephemeral: true);
            }
        }
    }
}
