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
    public class BlackjackModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;

        [SlashCommand("blackjack", "Blackjack.")]
        public async Task HandleSlashCommand([Summary(description: "How much money would you like to bet? (all is also valid)")] string? amount = null)
        {
            var gameType = await _context.GameTypes.FirstOrDefaultAsync(x => x.Name == "blackjack");
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
                Title = "Blackjack",
                Color = Color.Gold,
                ImageUrl = "attachment://blackjack_cards.png",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = AuthorIconUrl,
                    Name = AuthorName
                }
            };

            var cards = DeckUtility.CreateDeck();
            var deckCards = DeckUtility.ShuffleCards(cards);

            var gameData = new Blackjack(deckCards);

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

                embedBuilder.Footer = new EmbedFooterBuilder
                {
                    Text = $"New balance is {UserData.Balance:C}"
                };
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

            var cardImageBytes = DeckUtility.GenerateBlackjackGameImage(gameData.DealersCards, gameData.PlayersCards, gameData.RevealedDealersCards);
            var cardImageFile = new FileAttachment(new MemoryStream(cardImageBytes), "blackjack_cards.png");

            var embed = embedBuilder.Build();
            var components = new ComponentBuilder()
                .WithButton("Hit", "blackjack:hit", ButtonStyle.Secondary)
                .WithButton("Stand", "blackjack:stand", ButtonStyle.Secondary)
                .WithButton("Split", "blackjack:split", ButtonStyle.Secondary)
                .WithButton("Double", "blackjack:double", ButtonStyle.Secondary)
                .Build();
            await RespondWithFileAsync(cardImageFile, embed: embed, components: components);
        }
    }
}
