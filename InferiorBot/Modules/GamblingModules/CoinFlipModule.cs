using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;

namespace InferiorBot.Modules.GamblingModules
{
    [RequireContext(ContextType.Guild)]
    public class CoinFlipModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;

        public enum CoinFlipEnum
        {
            [ChoiceDisplay("heads")] Heads,
            [ChoiceDisplay("tails")] Tails
        }

        [SlashCommand("coinflip", "Flip a coin for a chance to double your money.")]
        public async Task HandleSlashCommand([Summary(description: "heads or tails?")] CoinFlipEnum selection, [Summary(description: "How much money would you like to bet? (all is also valid)")] string? amount = null)
        {
            var sides = new[] { nameof(CoinFlipEnum.Heads), nameof(CoinFlipEnum.Tails) };
            var index = NumericRandomizer.GenerateRandomNumber(sides.Length);
            var result = sides[index] == selection.ToString();

            var embedBuilder = new EmbedBuilder
            {
                Title = result ? "WINNER" : "LOSER",
                Color = result ? Color.DarkGreen : Color.DarkRed,
                ThumbnailUrl = $"attachment://{sides[index]}.png",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = AuthorIconUrl,
                    Name = AuthorName
                }
            };

            var (betAmount, error) = UserData.GetBetAmount(amount);
            if (!string.IsNullOrWhiteSpace(error))
            {
                await RespondAsync(error, ephemeral: true);
                return;
            }

            if (betAmount > 0m)
            {
                if (result)
                {
                    UserData.WonMoney(betAmount);
                    UserData.UserStat.CoinFlipWins++;
                }
                else
                {
                    UserData.LostMoney(betAmount);
                    UserData.UserStat.CoinFlipLosses++;
                }

                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to update user.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                embedBuilder.Description = $"""
                                            You bet {Format.Bold($"{betAmount:C}")} on {selection}.
                                            It was {sides[index]}.
                                            """;
                embedBuilder.Footer = new EmbedFooterBuilder
                {
                    Text = $"New balance is {UserData.Balance:C}"
                };
            }
            else
            {
                embedBuilder.Description = $"""
                                            You guessed {selection}.
                                            It was {sides[index]}.
                                            """;
            }

            var image = sides[index] == nameof(CoinFlipEnum.Heads)
                ? Properties.Resources.penny_heads
                : Properties.Resources.penny_tails;
            var file = new FileAttachment(new MemoryStream(image), $"{sides[index]}.png");

            var embed = embedBuilder.Build();
            await RespondWithFileAsync(file, embed: embed);
        }
    }
}
