using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using InferiorBot.Games;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;
using InferiorGame = Infrastructure.InferiorBot.Game;

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

        [Group("start", "Start a game.")]
        public class StartGameModules(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
        {
            private readonly InferiorBotContext _context = context;

            [SlashCommand("guess-the-number", "Who ever guesses the number closest wins.")]
            public async Task StartGuessTheNumberCommand([Summary(description: "How much money is required to play?")] string? amount = null)
            {
                var gameType = await _context.GameTypes.FirstOrDefaultAsync(x => x.Name == "guess the number");
                if (gameType == null)
                {
                    await RespondAsync("Failed to find game type.", ephemeral: true);
                    return;
                }

                var guess = Commands.FirstOrDefault(x => x.Name == "guess");
                var activeGame = ActiveGames.FirstOrDefault(x => x.GameTypeId == gameType.GameTypeId);
                if (activeGame != null)
                {
                    var activeGameData = activeGame.GetGuessTheNumberData();
                    if (activeGameData != null && DateTime.Now < activeGameData.ExpireDate)
                    {
                        await RespondAsync(
                            $"There is already a `Guess the Number` in progress. Do {(guess != null ? DiscordFormatter.Mention(guess) : "/guess")} to play.",
                            ephemeral: true);
                        return;
                    }

                    _context.GameUsers.RemoveRange(activeGame.GameUsers);
                    _context.Games.Remove(activeGame);

                    if (!_context.ChangeTracker.HasChanges())
                    {
                        await RespondAsync("Failed to remove existing game.", ephemeral: true);
                        return;
                    }
                    await _context.SaveChangesAsync();
                }

                var gameData = new GuessTheNumber
                {
                    Answer = NumericRandomizer.GenerateRandomNumber(0, 100),
                    ExpireDate = DateTime.Now.AddSeconds(10)
                };
                if (!string.IsNullOrEmpty(amount))
                {
                    switch (UserData.Balance)
                    {
                        case 0m:
                            await RespondAsync(":x: You have no money!", ephemeral: true);
                            return;
                        case < 0m:
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
                    if (betAmount <= 0m)
                    {
                        await RespondAsync(":x: You can't bet nothing!", ephemeral: true);
                        return;
                    }

                    gameData.BetAmount = betAmount;
                }

                var game = new InferiorGame
                {
                    GameTypeId = gameType.GameTypeId,
                    GuildId = Convert.ToString(Context.Guild.Id),
                    GameData = gameData.ToJson()
                };
                await _context.Games.AddAsync(game);
                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to create game.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();
                await _context.Entry(game).ReloadAsync();

                var description = $"""
                                   Guess a number between 0 and 100.
                                   Who ever guesses the closest wins.

                                   [GAME_PRICE]

                                   Number will be revealed {Format.Bold(DiscordFormatter.Timestamp(gameData.ExpireDate))}
                                   Use {(guess != null ? DiscordFormatter.Mention(guess) : "/guess")} to make a guess.
                                   """;
                description = gameData.BetAmount != 0m
                    ? description.Replace("[GAME_PRICE]", $"Current price to play {Format.Bold($"{gameData.BetAmount:C}")}.")
                    : description.Replace("""

                                          [GAME_PRICE]

                                          """, string.Empty);

                await RespondAsync(embed: new EmbedBuilder
                {
                    Title = "Guess the Number",
                    Description = description,
                    Color = Color.DarkGreen
                }.Build());

                await Task.Delay(gameData.ExpireDate - DateTime.Now);
                await DeleteOriginalResponseAsync();
                await _context.Entry(game).ReloadAsync();

                var gameUsers = await _context.GetGameUsers(game);
                gameData = game.GetGuessTheNumberData();
                if (gameData == null)
                {
                    await RespondAsync("Something went wrong with the game.", ephemeral: true);
                    return;
                }

                description = "There were no winners.";
                var footer = string.Empty;

                var sortedUsers = gameUsers.OrderBy(x => Math.Abs(x.GetGuessTheNumberAnswer() - gameData.Answer)).ToList();
                var winners = sortedUsers.Where(x =>
                    Math.Abs(x.GetGuessTheNumberAnswer() - gameData.Answer) ==
                    Math.Abs(sortedUsers[0].GetGuessTheNumberAnswer() - gameData.Answer)).ToList();
                if (winners.Count > 0)
                {
                    var prizeMoney = 0m;
                    var guessedCorrectly = winners.Any(x => Math.Abs(x.GetGuessTheNumberAnswer() - gameData.Answer) == 0);
                    if (gameData.BetAmount != 0m)
                    {
                        prizeMoney = guessedCorrectly
                            ? gameData.PrizeMoney * 1.5m / winners.Count
                            : gameData.PrizeMoney / winners.Count;
                    }

                    description = $"{Format.Bold($"Winner{(winners.Count > 1 ? 's' : string.Empty)}:")}";
                    foreach (var gameUser in winners)
                    {
                        var userId = Convert.ToUInt64(gameUser.UserId);
                        var guildUser = Context.Guild.Users.FirstOrDefault(x => x.Id == userId);
                        if (guildUser == null) continue;

                        description += $"{Environment.NewLine}{DiscordFormatter.Mention(guildUser)}";

                        var difference = Math.Abs(gameUser.GetGuessTheNumberAnswer() - gameData.Answer);
                        footer = difference == 0 ? "Guessed the number!" : $"{difference} off!";

                        await _context.Entry(gameUser.User).ReloadAsync();
                        gameUser.User.UserStat.GuessWins++;
                        if (prizeMoney > 0m) gameUser.User.WonMoney(prizeMoney);
                    }

                    if (gameData.BetAmount > 0m)
                    {
                        description += $"{Environment.NewLine}{Environment.NewLine}";
                        if (guessedCorrectly)
                        {
                            description += $"Exact number guessed! Prize gets 1.5x multiplier!{Environment.NewLine}";
                        }
                        description += winners.Count > 1
                            ? $"Each winner gets {Format.Bold($"{prizeMoney:C}")}!"
                            : $"You won {Format.Bold($"{prizeMoney:C}")}!";
                    }
                }

                var losers = sortedUsers
                    .Where(x => winners.All(winner => winner.UserId != x.UserId))
                    .ToList();
                if (losers.Count > 0)
                {
                    foreach (var gameUser in losers)
                    {
                        await _context.Entry(gameUser.User).ReloadAsync();
                        gameUser.User.UserStat.GuessLosses++;
                        if (gameData.BetAmount > 0m) gameUser.User.LostMoney(gameData.BetAmount, false);
                    }
                }

                _context.GameUsers.RemoveRange(gameUsers);
                _context.Games.Remove(game);

                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to delete game.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await Context.Channel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = $"The number was {gameData.Answer}",
                    Description = description,
                    Color = Color.Gold,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = footer
                    }
                }.Build());
            }
        }

        [SlashCommand("guess", "Guess a number between 0 and 100.")]
        public async Task GuessCommand([Summary(description: "Enter a number between 0 and 100."), MinValue(0), MaxValue(100)] int number)
        {
            var gameType = await _context.GameTypes.FirstOrDefaultAsync(x => x.Name == "guess the number");
            if (gameType == null)
            {
                await RespondAsync("Failed to find game type.", ephemeral: true);
                return;
            }

            var activeGame = ActiveGames.FirstOrDefault(x => x.GameTypeId == gameType.GameTypeId);
            if (activeGame == null)
            {
                await RespondAsync("There is no guess the number games in progress.", ephemeral: true);
                return;
            }

            var activeGameData = activeGame.GetGuessTheNumberData();
            if (activeGameData == null || DateTime.Now > activeGameData.ExpireDate)
            {
                _context.GameUsers.RemoveRange(activeGame.GameUsers);
                _context.Games.Remove(activeGame);

                if (!_context.ChangeTracker.HasChanges())
                {
                    await RespondAsync("Failed to remove existing guess the number game.", ephemeral: true);
                    return;
                }
                await _context.SaveChangesAsync();

                await RespondAsync("There is no guess the number games in progress.", ephemeral: true);
                return;
            }

            if (activeGame.GameUsers.FirstOrDefault(x => x.UserId == UserData.UserId) != null)
            {
                await RespondAsync("You have already made a guess for this game.", ephemeral: true);
                return;
            }

            if (activeGameData.BetAmount > 0m)
            {
                if (activeGameData.BetAmount > UserData.Balance)
                {
                    await RespondAsync(":x: You can't bet more than you have!", ephemeral: true);
                    return;
                }

                UserData.Balance -= activeGameData.BetAmount;
                activeGameData.PrizeMoney += activeGameData.BetAmount;
                activeGame.GameData = activeGameData.ToJson();
            }

            var gameUser = new GameUser
            {
                GameId = activeGame.GameId,
                UserId = UserData.UserId,
                UserData = new GuessTheNumberUser
                {
                    Number = number
                }.ToJson()
            };
            await _context.GameUsers.AddAsync(gameUser);
            if (!_context.ChangeTracker.HasChanges())
            {
                await RespondAsync("Failed to join guess the number game.", ephemeral: true);
                return;
            }
            await _context.SaveChangesAsync();
            await _context.Entry(gameUser).ReloadAsync();
            await _context.Entry(activeGame).ReloadAsync();

            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Guess the Number",
                Description = $"You bet the number {Format.Bold(Convert.ToString(number))}.",
                Color = Color.DarkGreen,
                Footer = new EmbedFooterBuilder
                {
                    Text = activeGameData.BetAmount > 0m ? $"New balance is {UserData.Balance:C}" : null
                }
            }.Build(), ephemeral: true);
        }
    }
}
