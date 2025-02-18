using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using Infrastructure.InferiorBot;

namespace InferiorBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class GamblingModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;

        public enum CoinFlipEnum
        {
            [ChoiceDisplay("heads")] Heads,
            [ChoiceDisplay("tails")] Tails
        }
        [SlashCommand("coinflip", "Flip a coin for a chance to double your money.")]
        public async Task CoinFlip([Summary(description: "heads or tails?")] CoinFlipEnum selection, [Summary(description: "How much money would you like to bet? (all is also valid)")] string? amount = null)
        {
            var sides = new[] { CoinFlipEnum.Heads.ToString(), CoinFlipEnum.Tails.ToString() };
            var index = NumericRandomizer.GenerateRandomNumber(sides.Length);
            var result = sides[index] == selection.ToString();

            Embed embed;
            if (!string.IsNullOrEmpty(amount))
            {
                switch (UserData.Balance)
                {
                    case 0:
                        await RespondAsync(":x: You have no money!", ephemeral: true);
                        return;
                    case < 0:
                        await RespondAsync(":x: You are already in dept!", ephemeral: true);
                        return;
                }

                amount = amount.ToLower();
                if (amount != "all" && !decimal.TryParse(amount, out _)) amount = "0";
                if (amount.Contains('.') && amount.Split('.')[1].Length > 2)
                {
                    await RespondAsync(":x: Invalid bet amount!", ephemeral: true);
                    return;
                }

                var betAmount = amount == "all" ? UserData.Balance : Convert.ToDecimal(amount);
                if (betAmount > UserData.Balance)
                {
                    await RespondAsync(":x: You can't bet more money than you have!", ephemeral: true);
                    return;
                }
                if (betAmount <= 0)
                {
                    await RespondAsync(":x: You can't bet nothing!", ephemeral: true);
                    return;
                }

                if (result)
                {
                    UserData.Balance += betAmount;
                    UserData.UserStat.CoinFlipWins++;
                    UserData.UserStat.AllTimeWon += betAmount;
                    UserData.UserStat.BiggestWin = UserData.UserStat.BiggestWin > betAmount ? UserData.UserStat.BiggestWin : betAmount;
                }
                else
                {
                    UserData.Balance -= betAmount;
                    UserData.UserStat.CoinFlipLosses++;
                    UserData.UserStat.AllTimeLost += betAmount;
                    UserData.UserStat.BiggestLoss = UserData.UserStat.BiggestLoss > betAmount ? UserData.UserStat.BiggestLoss : betAmount;
                }

                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to update user.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                embed = new EmbedBuilder
                {
                    Title = result ? "WINNER" : "LOSER",
                    Description = $"""
                                   You bet {Format.Bold($"{betAmount:C}")} on {selection}.
                                   It was {sides[index]}.
                                   """,
                    Color = result ? Color.DarkGreen : Color.DarkRed,
                    ThumbnailUrl = $"attachment://{sides[index]}.png",
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = AuthorIconUrl,
                        Name = AuthorName
                    },
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"New balance is {UserData.Balance:C}"
                    }
                }.Build();
            }
            else
            {
                embed = new EmbedBuilder
                {
                    Title = result ? "WINNER" : "LOSER",
                    Description = $"""
                                   You guessed {selection}.
                                   It was {sides[index]}.
                                   """,
                    Color = result ? Color.DarkGreen : Color.DarkRed,
                    ThumbnailUrl = $"attachment://{sides[index]}.png",
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = AuthorIconUrl,
                        Name = AuthorName
                    }
                }.Build();
            }
            
            var image = sides[index] == CoinFlipEnum.Heads.ToString()
                ? Properties.Resources.penny_heads
                : Properties.Resources.penny_tails;
            var file = new FileAttachment(new MemoryStream(image), $"{sides[index]}.png");
            await RespondWithFileAsync(file, embed: embed);
        }
    }
}
