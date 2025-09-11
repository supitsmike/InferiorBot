using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InferiorGame = Infrastructure.InferiorBot.Game;

namespace InferiorBot.Modules
{
    public class BaseUserModule(InferiorBotContext context, IServiceProvider services) : InteractionModuleBase<SocketInteractionContext>
    {
        protected IConfiguration Configuration = null!;
        protected IReadOnlyCollection<SocketApplicationCommand> Commands = null!;
        protected IReadOnlyCollection<Emote> Emotes = null!;

        protected string? AuthorName;
        protected string? AuthorIconUrl;

        protected Guild GuildData = null!;
        protected User UserData = null!;
        protected List<InferiorGame> ActiveGames = null!;
        protected List<ConvertedUrl> ConvertedUrls = null!;

        public override async Task BeforeExecuteAsync(ICommandInfo command)
        {
            Configuration = services.GetRequiredService<IConfiguration>();
            Commands = await Context.Client.GetGlobalApplicationCommandsAsync();
            Emotes = await Context.Client.GetApplicationEmotesAsync();

            AuthorName = $"{Context.User.Username}{(Context.User.Discriminator != "0000" ? $"#{Context.User.Discriminator}" : string.Empty)}";
            AuthorIconUrl = Context.User.GetDisplayAvatarUrl() ?? Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl();

            var userId = Convert.ToString(Context.User.Id);
            var guildId = Convert.ToString(Context.Guild.Id);
            var channelId = Convert.ToString(Context.Channel.Id);

            GuildData = await Context.Guild.GetGuildDataAsync(context);
            UserData = await Context.User.GetUserDataAsync(context, services);
            ActiveGames = await context.Games.Include(x => x.GameUsers).Where(x => x.UserId == userId || x.GuildId == guildId).ToListAsync();
            ConvertedUrls = await context.ConvertedUrls.Where(x => x.Guild.GuildId == guildId && x.ChannelId == channelId).ToListAsync();

            await base.BeforeExecuteAsync(command);
        }
    }
}
