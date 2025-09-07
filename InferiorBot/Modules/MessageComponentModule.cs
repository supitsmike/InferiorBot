using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;

namespace InferiorBot.Modules
{
    public class MessageComponentModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;

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

            var messageId = Convert.ToString(interaction.Message.Id);
            var previousPost = ConvertedUrls.FirstOrDefault(x => x.MessageId == messageId);
            if (previousPost != null)
            {
                _context.ConvertedUrls.Remove(previousPost);
                if (_context.ChangeTracker.HasChanges()) await _context.SaveChangesAsync();
            }

            await interaction.Message.DeleteAsync();
        }
    }
}
