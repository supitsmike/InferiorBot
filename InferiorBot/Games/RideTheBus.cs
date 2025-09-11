using InferiorBot.Classes;

namespace InferiorBot.Games
{
    public class RideTheBus(List<Card> cards) : BaseGame
    {
        public List<Card> Cards { get; set; } = cards;
        public bool[] RevealedCards { get; set; } = [false, false, false, false];
        public decimal BetAmount { get; set; }
    }
}
