using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;

namespace InferiorBot.Modules
{
    public class MessageComponentModule : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction("convert_url:*")]
        public async Task ConvertUrl(string type)
        {
            if (Context.Interaction is not IComponentInteraction componentInteraction) return;

            var user = componentInteraction.Message.MentionedUserIds.FirstOrDefault(id => id == Context.User.Id);
            if (user == 0)
            {
                await RespondAsync("You did not post this message!", ephemeral: true);
                return;
            }

            var convertSplit = componentInteraction.Message.Content.Split(": ");
            if (convertSplit.Length != 2) return;
            var url = convertSplit[1];
            if (!url.IsValidUrl()) return;

            url = Methods.ConvertUrl(url, type, out _);
            if (string.IsNullOrEmpty(url)) return;

            await componentInteraction.UpdateAsync(x => x.Content = $"{DiscordFormatter.Mention(Context.User)}: {url}");
        }

        [ComponentInteraction("delete_message")]
        public async Task DeleteMessage()
        {
            if (Context.Interaction is not IComponentInteraction componentInteraction) return;

            var user = componentInteraction.Message.MentionedUserIds.FirstOrDefault(id => id == Context.User.Id);
            if (user == 0)
            {
                await RespondAsync("You did not post this message!", ephemeral: true);
                return;
            }

            await componentInteraction.Message.DeleteAsync();
        }
    }
}
