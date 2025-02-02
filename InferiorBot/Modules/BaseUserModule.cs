using Discord;
using Discord.Commands;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;

namespace InferiorBot.Modules
{
    public class BaseUserModule(InferiorBotContext context) : InteractionModuleBase<SocketInteractionContext>
    {
        protected Guild GuildData = null!;
        protected User UserData = null!;

        protected string? AuthorName;
        protected string? AuthorIconUrl;

        public override async Task BeforeExecuteAsync(ICommandInfo command)
        {
            AuthorName = $"{Context.User.Username}{(Context.User.Discriminator != "0000" ? $"#{Context.User.Discriminator}" : string.Empty)}";
            AuthorIconUrl = Context.User.GetDisplayAvatarUrl() ?? Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl();

            GuildData = await Context.Guild.GetGuildDataAsync(context);
            UserData = await Context.User.GetUserDataAsync(context);
            
            await base.BeforeExecuteAsync(command);
        }
    }
}
