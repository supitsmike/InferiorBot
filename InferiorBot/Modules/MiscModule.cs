using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InferiorBot.Classes;
using InferiorBot.Extensions;

namespace InferiorBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class MiscModule : InteractionModuleBase<SocketInteractionContext>
    {
        public enum AvatarSizeEnum
        {
            [ChoiceDisplay("16x16")] Size16 = 16,
            [ChoiceDisplay("32x32")] Size32 = 32,
            [ChoiceDisplay("64x64")] Size64 = 64,
            [ChoiceDisplay("128x128")] Size128 = 128,
            [ChoiceDisplay("256x256")] Size256 = 256,
            [ChoiceDisplay("512x512")] Size512 = 512,
            [ChoiceDisplay("1024x1024")] Size1024 = 1024,
            [ChoiceDisplay("2048x2048")] Size2048 = 2048,
            [ChoiceDisplay("4096x4096")] Size4096 = 4096
        }
        [SlashCommand("avatar", "Get the avatar URL of the selected user, or your own avatar.")]
        public async Task Avatar([Summary(description: "The member's avatar to show.")] SocketUser? user = null,
            [Summary(description: "The size of the avatar image.")] AvatarSizeEnum size = AvatarSizeEnum.Size256)
        {
            user ??= Context.User;

            var avatarUrl = user.GetAvatarUrl(size: (ushort)size);
            var displayAvatarUrl = user.GetDisplayAvatarUrl(size: (ushort)size);

            var embeds = new List<Embed>
            {
                new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = user.GetUserName(),
                        IconUrl = avatarUrl
                    },
                    Title = "Avatar:",
                    ImageUrl = avatarUrl
                }.Build()
            };

            if (!string.IsNullOrEmpty(displayAvatarUrl) && displayAvatarUrl != avatarUrl)
            {
                embeds.Add(new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = user.GetUserName(),
                        IconUrl = displayAvatarUrl
                    },
                    Title = "Server Avatar:",
                    ImageUrl = displayAvatarUrl
                }.Build());
            }

            await RespondAsync(embeds: embeds.ToArray());
        }

        [SlashCommand("8ball", "Ask the magic 8 ball a question.")]
        public async Task Magic8Ball([Summary(description: "What question would you like to ask?")] string question)
        {
            string[] answers =
            [
                "Yes", "No", "Maybe", "Count on it", "Ask again", "No doubt", "Absolutely", "Go for it", "Wait for it",
                "Not now", "Very likely", "Not likely"
            ];

            var answer = answers[Methods.GenerateRandomNumber(answers.Length)];
            await RespondAsync(embed: new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = Context.User.GetUserName(),
                    IconUrl = Context.User.GetDisplayAvatarUrl()
                },
                Color = new Color(0),
                Title = "Magic 8 Ball",
                Description = $"{question}",
                Footer = new EmbedFooterBuilder
                {
                    Text = answer
                }
            }.Build());
        }
    }
}
