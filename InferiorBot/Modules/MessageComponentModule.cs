using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Modules
{
    public class MessageComponentModule(InferiorBotContext context) : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction("convert_url:*")]
        public async Task ConvertUrl(string type)
        {
            if (Context.Interaction is not IComponentInteraction interaction) return;

            var user = interaction.Message.MentionedUserIds.FirstOrDefault(id => id == Context.User.Id);
            if (user == 0)
            {
                await RespondAsync("You did not post this message!", ephemeral: true);
                return;
            }

            var convertSplit = interaction.Message.Content.Split(": ");
            if (convertSplit.Length != 2) return;
            var url = convertSplit[1];
            if (!url.IsValidUrl()) return;

            url = Methods.ConvertUrl(url, type, out _);
            if (string.IsNullOrEmpty(url)) return;

            await interaction.UpdateAsync(x => x.Content = $"{DiscordFormatter.Mention(Context.User)}: {url}");
        }

        [ComponentInteraction("delete_message")]
        public async Task DeleteMessage()
        {
            if (Context.Interaction is not IComponentInteraction interaction) return;

            var user = interaction.Message.MentionedUserIds.FirstOrDefault(id => id == Context.User.Id);
            if (user == 0)
            {
                await RespondAsync("You did not post this message!", ephemeral: true);
                return;
            }

            var previousPost = await context.ConvertedUrls.FirstOrDefaultAsync(x => x.GuildId == interaction.GuildId && x.ChannelId == interaction.ChannelId && x.MessageId == interaction.Message.Id);
            if (previousPost != null)
            {
                context.ConvertedUrls.Remove(previousPost);
                if (context.ChangeTracker.HasChanges()) await context.SaveChangesAsync();
            }

            await interaction.Message.DeleteAsync();
        }
    }
}
