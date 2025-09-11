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

        [SlashCommand("ridethebus", "Ride the bus.")]
        public async Task RideTheBusCommand([Summary(description: "How much money would you like to bet? (all is also valid)")] string? amount = null)
        {
            var gameType = await _context.GameTypes.FirstOrDefaultAsync(x => x.Name == "ride the bus");
            if (gameType == null)
            {
                await RespondAsync("Failed to find game type.", ephemeral: true);
                return;
            }

            if (ActiveGames.Any(x => x.GameTypeId == gameType.GameTypeId && x.UserId == UserData.UserId))
            {
                await RespondAsync("You already have a game in progress.", ephemeral: true);
                return;
            }

            var embedBuilder = new EmbedBuilder
            {
                Title = "Ride The Bus",
                Color = Color.Gold,
                ImageUrl = "attachment://ride_the_bus_cards.png",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = AuthorIconUrl,
                    Name = AuthorName
                }
            };

            var cards = DeckUtility.CreateDeck();
            var shuffledCards = DeckUtility.ShuffleCards(cards);
            var deckCards = shuffledCards.Take(4).ToList();

            var gameData = new RideTheBus(deckCards);

            var (betAmount, error) = UserData.GetBetAmount(amount);
            if (!string.IsNullOrWhiteSpace(error))
            {
                await RespondAsync(error, ephemeral: true);
                return;
            }

            if (betAmount > 0m)
            {
                UserData.TakeMoney(betAmount);
                gameData.BetAmount = betAmount;

                embedBuilder.Description = $"""
                                            Is the first card {Format.Bold("Black")} or a {Format.Bold("Red")}?
                                            
                                            Correct guess wins {Format.Bold($"{betAmount * 2:C}")}!
                                            """;
                embedBuilder.Footer = new EmbedFooterBuilder
                {
                    Text = $"New balance is {UserData.Balance:C}"
                };
            }
            else
            {
                embedBuilder.Description = $"Is the first card {Format.Bold("Black")} or a {Format.Bold("Red")}?";
            }

            var game = new InferiorGame
            {
                GameTypeId = gameType.GameTypeId,
                UserId = UserData.UserId,
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

            var cardImageBytes = DeckUtility.GenerateCardImage(gameData.Cards, gameData.RevealedCards);
            var cardImageFile = new FileAttachment(new MemoryStream(cardImageBytes), "ride_the_bus_cards.png");

            var embed = embedBuilder.Build();
            var components = new ComponentBuilder()
                .WithButton("Black", "ridethebus:black", ButtonStyle.Secondary)
                .WithButton("Red", "ridethebus:red", ButtonStyle.Secondary)
                .Build();
            await RespondWithFileAsync(cardImageFile, embed: embed, components: components);
        }

        [ComponentInteraction("ridethebus:*")]
        public async Task RideTheBusFollowupCommand(string answer)
        {
            if (Context.Interaction is not IComponentInteraction interaction) return;
            if (interaction.User.Id != interaction.Message.InteractionMetadata.UserId)
            {
                await RespondAsync("You are not the owner of this game!", ephemeral: true);
                return;
            }

            var gameType = await _context.GameTypes.FirstOrDefaultAsync(x => x.Name == "ride the bus");
            if (gameType == null)
            {
                await RespondAsync("Failed to find game type.", ephemeral: true);
                return;
            }

            var game = ActiveGames.FirstOrDefault(x => x.GameTypeId == gameType.GameTypeId && x.UserId == UserData.UserId);
            if (game == null)
            {
                await RespondAsync("Failed to find active game.", ephemeral: true);
                return;
            }

            var gameData = game.GameData.ToObject<RideTheBus>();
            if (gameData == null)
            {
                await RespondAsync("Failed to parse game object.", ephemeral: true);
                return;
            }

            var embedBuilder = new EmbedBuilder
            {
                Title = "Ride The Bus",
                ImageUrl = "attachment://ride_the_bus_cards.png",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = AuthorIconUrl,
                    Name = AuthorName
                }
            };
            var componentBuilder = new ComponentBuilder();

            var roundMultipliers = new[] { 2m, 3m, 4m, 20m };
            var currentRound = Array.FindIndex(gameData.RevealedCards, x => !x);
            
            var wonRound = false;
            var wonGame = false;
            var cashOut = false;
            var previousMultiplier = currentRound > 0 && currentRound < roundMultipliers.Length
                ? roundMultipliers[currentRound - 1]
                : 1m;
            var currentMultiplier = currentRound >= 0 && currentRound < roundMultipliers.Length
                ? roundMultipliers[currentRound]
                : previousMultiplier;
            var nextMultiplier = currentRound + 1 < roundMultipliers.Length 
                ? roundMultipliers[currentRound + 1] 
                : currentMultiplier;

            var previousReward = gameData.BetAmount * previousMultiplier;
            var currentReward = gameData.BetAmount * currentMultiplier;
            var nextReward = gameData.BetAmount * nextMultiplier;

            switch (currentRound)
            {
                case 0 when answer is "black" or "red":
                {
                    var isBlack = gameData.Cards[0].Suit is CardSuit.Spades or CardSuit.Clubs;
                    var isRed = gameData.Cards[0].Suit is CardSuit.Diamonds or CardSuit.Hearts;

                    gameData.RevealedCards[0] = true;
                    if ((isBlack && answer == "black") || (isRed && answer == "red"))
                    {
                        embedBuilder.Description = $"Is the second card a {Format.Bold("Higher")} or a {Format.Bold("Lower")}?";
                        componentBuilder
                            .WithButton("Higher", "ridethebus:higher", ButtonStyle.Secondary)
                            .WithButton("Lower", "ridethebus:lower", ButtonStyle.Secondary);

                        wonRound = true;
                    }
                    else
                    {
                        embedBuilder.Description = $"You selected {Format.Bold(answer.ToTitleCase())} but the answer was {Format.Bold(isBlack ? "Black" : "Red")}.";
                        embedBuilder.Color = Color.DarkRed;
                    }

                    break;
                }
                case 1 when answer is "higher" or "lower":
                {
                    var firstCard = gameData.Cards[0].Rank.IsAce ? 11 : gameData.Cards[0].Rank.ComparisonValue;
                    var secondCard = gameData.Cards[1].Rank.IsAce ? 11 : gameData.Cards[1].Rank.ComparisonValue;
                    var isHigher = secondCard > firstCard;
                    var isLower = secondCard < firstCard;
                    var isEqual = secondCard == firstCard;

                    gameData.RevealedCards[1] = true;
                    if ((isHigher && answer == "higher") || (isLower && answer == "lower") || isEqual)
                    {
                        embedBuilder.Description = $"Is the third card a {Format.Bold("In-Between")} or a {Format.Bold("Outside")}?";
                        componentBuilder
                            .WithButton("In-Between", "ridethebus:inbetween", ButtonStyle.Secondary)
                            .WithButton("Outside", "ridethebus:outside", ButtonStyle.Secondary);

                        wonRound = true;
                    }
                    else
                    {
                        embedBuilder.Description = $"You selected {Format.Bold(answer.ToTitleCase())} but the answer was {Format.Bold(isHigher ? "Higher" : "Lower")}.";
                        embedBuilder.Color = Color.DarkRed;
                    }

                    break;
                }
                case 2 when answer is "inbetween" or "outside":
                {
                    var firstCard = gameData.Cards[0].Rank.IsAce ? 11 : gameData.Cards[0].Rank.ComparisonValue;
                    var secondCard = gameData.Cards[1].Rank.IsAce ? 11 : gameData.Cards[1].Rank.ComparisonValue;
                    var thirdCard = gameData.Cards[2].Rank.IsAce ? 11 : gameData.Cards[2].Rank.ComparisonValue;

                    var lowCard = Math.Min(firstCard, secondCard);
                    var highCard = Math.Max(firstCard, secondCard);

                    var isInbetween = thirdCard > lowCard && thirdCard < highCard;
                    var isOutside = thirdCard < lowCard || thirdCard > highCard;
                    var isEqual = thirdCard == lowCard || thirdCard == highCard;

                    gameData.RevealedCards[2] = true;
                    if ((isInbetween && answer == "inbetween") || (isOutside && answer == "outside") || isEqual)
                    {
                        embedBuilder.Description = $"What is the {Format.Bold("Suit")} of the fourth card?";
                        componentBuilder
                            .WithButton("Spades", "ridethebus:spades", ButtonStyle.Secondary)
                            .WithButton("Diamonds", "ridethebus:diamonds", ButtonStyle.Secondary)
                            .WithButton("Clubs", "ridethebus:clubs", ButtonStyle.Secondary)
                            .WithButton("Hearts", "ridethebus:hearts", ButtonStyle.Secondary);

                        wonRound = true;
                    }
                    else
                    {
                        if (answer == "inbetween") answer = "In-Between"; 
                        embedBuilder.Description = $"You selected {Format.Bold(answer.ToTitleCase())} but the answer was {Format.Bold(isInbetween ? "In-Between" : "Outside")}.";
                        embedBuilder.Color = Color.DarkRed;
                    }

                    break;
                }
                case 3 when answer is "spades" or "diamonds" or "clubs" or "hearts":
                {
                    var suit = gameData.Cards[3].Suit.ToString();

                    gameData.RevealedCards[3] = true;
                    if (string.Equals(suit, answer, StringComparison.CurrentCultureIgnoreCase))
                    {
                        wonRound = true;
                        wonGame = true;
                    }
                    else
                    {
                        embedBuilder.Description = $"You selected {Format.Bold(answer.ToTitleCase())} but the answer was {Format.Bold(suit)}.";
                        embedBuilder.Color = Color.DarkRed;
                    }

                    break;
                }
                default:
                {
                    if (answer == "cashout")
                    {
                        if (previousReward > 0m)
                        {
                            UserData.WonMoney(previousReward);
                            embedBuilder.Description = $"You cashed out and won {Format.Bold($"{previousReward:C}")}!";
                            embedBuilder.Footer = new EmbedFooterBuilder
                            {
                                Text = $"New balance is {UserData.Balance:C}"
                            };
                        }

                        cashOut = true;
                        embedBuilder.Color = Color.Gold;
                        UserData.UserStat.RideTheBusWins++;
                        _context.Games.Remove(game);
                    }
                    else
                    {
                        await RespondAsync("Something went wrong with the game.", ephemeral: true);
                        return;
                    }

                    break;
                }
            }

            if (!cashOut)
            {
                if (wonRound)
                {
                    game.GameData = gameData.ToJson();

                    if (wonGame)
                    {
                        if (currentReward > 0m)
                        {
                            UserData.WonMoney(currentReward);

                            embedBuilder.Description = $"You won {Format.Bold($"{currentReward:C}")}!";
                            embedBuilder.Footer = new EmbedFooterBuilder
                            {
                                Text = $"New balance is {UserData.Balance:C}"
                            };
                        }
                        else
                        {
                            embedBuilder.Description = "You won!";
                        }

                        UserData.UserStat.RideTheBusWins++;
                        _context.Games.Remove(game);

                        embedBuilder.Color = Color.Gold;
                    }
                    else
                    {
                        if (nextReward > 0m)
                        {
                            embedBuilder.Description += $"{Environment.NewLine}{Environment.NewLine}Correct guess wins {Format.Bold($"{nextReward:C}")}!";
                            embedBuilder.Footer = new EmbedFooterBuilder
                            {
                                Text = $"Cash Out now and win {currentReward:C}"
                            };
                            componentBuilder.WithButton("Cash Out", "ridethebus:cashout", ButtonStyle.Danger);
                        }

                        embedBuilder.Color = Color.DarkGreen;
                    }
                }
                else
                {
                    if (currentReward > 0m)
                    {
                        UserData.LostMoney(gameData.BetAmount, false);
                        embedBuilder.Footer = new EmbedFooterBuilder
                        {
                            Text = $"New balance is {UserData.Balance:C}"
                        };
                    }
                    UserData.UserStat.RideTheBusLosses++;
                    _context.Games.Remove(game);

                    embedBuilder.Color = Color.DarkRed;
                }
            }

            if (!_context.ChangeTracker.HasChanges())
            {
                await RespondAsync("Failed to update game. Please try again.", ephemeral: true);
                return;
            }
            await _context.SaveChangesAsync();

            var cardImageBytes = DeckUtility.GenerateCardImage(gameData.Cards, gameData.RevealedCards);
            var cardImageFile = new FileAttachment(new MemoryStream(cardImageBytes), "ride_the_bus_cards.png");

            var embed = embedBuilder.Build();
            var components = componentBuilder.ActionRows != null ? componentBuilder.Build() : null;
            await interaction.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = components;
                x.Attachments = new List<FileAttachment>
                {
                    cardImageFile
                };
            });
        }
    }
}
