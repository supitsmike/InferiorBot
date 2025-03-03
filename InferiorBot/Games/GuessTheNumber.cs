namespace InferiorBot.Games
{
    public class GuessTheNumber : BaseGame
    {
        public int Answer { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PrizeMoney { get; set; }
        public DateTime ExpireDate { get; set; }
        //public GuessTheNumberUser[]? Users { get; set; }
    }

    public class GuessTheNumberUser : BaseGame
    {
        //public decimal UserId { get; set; }
        public int Number { get; set; }
    }
}
