using Discord.Interactions;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Modules
{
    public class BaseUserModule(InferiorBotContext context) : InteractionModuleBase<SocketInteractionContext>
    {
        protected Guild GuildData = null!;
        protected User UserData = null!;
        protected List<ConvertedUrl> ConvertedUrls = null!;

        protected string? AuthorName;
        protected string? AuthorIconUrl;

        public override async Task BeforeExecuteAsync(ICommandInfo command)
        {
            AuthorName = $"{Context.User.Username}{(Context.User.Discriminator != "0000" ? $"#{Context.User.Discriminator}" : string.Empty)}";
            AuthorIconUrl = Context.User.GetDisplayAvatarUrl() ?? Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl();

            GuildData = await Context.Guild.GetGuildDataAsync(context);
            UserData = await Context.User.GetUserDataAsync(context);
            ConvertedUrls = await context.ConvertedUrls.Where(x => x.Guild.GuildId == Context.Guild.Id && x.ChannelId == Context.Channel.Id).ToListAsync();

            await base.BeforeExecuteAsync(command);
        }
    }
}
