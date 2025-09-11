using InferiorBot.Properties;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace InferiorBot.Classes
{
    public class CardConfig
    {
        public const int Width = 100;
        public const int Height = 140;
        public const int Spacing = 10;
    }

    public enum CardSuit
    {
        Spades,
        Diamonds,
        Clubs,
        Hearts
    }

    public class CardRank(string value)
    {
        public string Value { get; } = value;
        public int Number
        {
            get
            {
                return Value switch
                {
                    "Ace" => 1,
                    "2" => 2,
                    "3" => 3,
                    "4" => 4,
                    "5" => 5,
                    "6" => 6,
                    "7" => 7,
                    "8" => 8,
                    "9" => 9,
                    "10" or "Jack" or "Queen" or "King" => 10,
                    _ => 0
                };
            }
        }

        public int ComparisonValue
        {
            get
            {
                return Value switch
                {
                    "Ace" => 1,
                    "2" => 2,
                    "3" => 3,
                    "4" => 4,
                    "5" => 5,
                    "6" => 6,
                    "7" => 7,
                    "8" => 8,
                    "9" => 9,
                    "10" => 10,
                    "Jack" => 11,
                    "Queen" => 12,
                    "King" => 13,
                    _ => 0
                };
            }
        }

        public bool IsAce => Value == "Ace";
    }

    public class Card(CardRank rank, CardSuit suit)
    {
        public CardSuit Suit { get; } = suit;
        public CardRank Rank { get; } = rank;
        public Image<Rgba32>? GetCardImage(bool shouldReveal = true)
        {
            try
            {
                var resourceName = shouldReveal
                    ? $"{Rank.Value.ToLower()}_of_{Suit.ToString().ToLower()}"
                    : "back_of_card";
                var resource = Resources.ResourceManager.GetObject(resourceName);
                if (resource is not byte[] resourceBytes) return null;

                var image = Image.Load<Rgba32>(resourceBytes);
                if (image.Width != CardConfig.Width || image.Height != CardConfig.Height)
                {
                    image.Mutate(x => x.Resize(CardConfig.Width, CardConfig.Height));
                }
                return image;
            }
            catch (Exception) { /* ignored */ }

            return null;
        }
    }

    public class DeckUtility
    {
        private static readonly CardSuit[] Suits = [CardSuit.Spades, CardSuit.Diamonds, CardSuit.Clubs, CardSuit.Hearts];

        private static readonly CardRank[] Ranks =
        [
            new("Ace"),
            new("2"),
            new("3"),
            new("4"),
            new("5"),
            new("6"),
            new("7"),
            new("8"),
            new("9"),
            new("10"),
            new("Jack"),
            new("Queen"),
            new("King")
        ];

        public static List<Card> CreateDeck()
        {
            var deck = new List<Card>();

            foreach (var suit in Suits)
            {
                // Use reverse order for Clubs and Hearts
                var suitRanks = suit is CardSuit.Clubs or CardSuit.Hearts
                    ? Ranks.Reverse()
                    : Ranks;

                deck.AddRange(suitRanks.Select(rank => new Card(rank, suit)));
            }

            return deck;
        }

        public static List<Card> ShuffleCards(List<Card> cards)
        {
            var shuffledArray = new List<Card>(cards);

            // Fisher-Yates Shuffle
            for (var i = shuffledArray.Count - 1; i > 0; i--)
            {
                var randomIndex = NumericRandomizer.GenerateRandomNumber(i + 1);
                (shuffledArray[i], shuffledArray[randomIndex]) = (shuffledArray[randomIndex], shuffledArray[i]);
            }

            return shuffledArray;
        }

        public static byte[] GenerateCardImage(List<Card> cards, bool[]? revealedCards = null)
        {
            if (cards == null || cards.Count == 0) throw new ArgumentException("Cards list cannot be null or empty");

            var totalWidth = (cards.Count * CardConfig.Width) + ((cards.Count - 1) * CardConfig.Spacing);
            using var image = new Image<Rgba32>(totalWidth, CardConfig.Height);
            image.Mutate(x => x.BackgroundColor(Color.Transparent));

            for (var i = 0; i < cards.Count; i++)
            {
                var x = i * (CardConfig.Width + CardConfig.Spacing);
                var shouldReveal = revealedCards == null || (i < revealedCards.Length && revealedCards[i]);

                var cardImage = cards[i].GetCardImage(shouldReveal);
                if (cardImage != null)
                {
                    image.Mutate(ctx => ctx.DrawImage(cardImage, new Point(x, 0), 1.0f));
                }
                else
                {
                    image.Mutate(ctx => ctx.Fill(Color.Gray, new RectangleF(x, 0, CardConfig.Width, CardConfig.Height)));
                }
            }

            using var outputStream = new MemoryStream();
            image.Save(outputStream, new PngEncoder());
            return outputStream.ToArray();
        }
    }
}
