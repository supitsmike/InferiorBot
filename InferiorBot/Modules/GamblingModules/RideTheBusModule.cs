using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using InferiorBot.Games;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;
using InferiorGame = Infrastructure.InferiorBot.Game;

namespace InferiorBot.Modules.GamblingModules
{
    [RequireContext(ContextType.Guild)]
    public class RideTheBusModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;

        [SlashCommand("ridethebus", "Ride the bus.")]
        public async Task HandleSlashCommand([Summary(description: "How much money would you like to bet? (all is also valid)")] string? amount = null)
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
        public async Task HandleComponentInteraction(string answer)
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
                    var firstCard = gameData.Cards[0].Rank.IsAce ? 14 : gameData.Cards[0].Rank.ComparisonValue;
                    var secondCard = gameData.Cards[1].Rank.IsAce ? 14 : gameData.Cards[1].Rank.ComparisonValue;
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
                    var firstCard = gameData.Cards[0].Rank.IsAce ? 14 : gameData.Cards[0].Rank.ComparisonValue;
                    var secondCard = gameData.Cards[1].Rank.IsAce ? 14 : gameData.Cards[1].Rank.ComparisonValue;
                    var thirdCard = gameData.Cards[2].Rank.IsAce ? 14 : gameData.Cards[2].Rank.ComparisonValue;

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
