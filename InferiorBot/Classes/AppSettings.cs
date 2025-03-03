namespace InferiorBot.Classes
{
    public class BotSettings
    {
        public required decimal StartingBalance { get; set; }
        public required decimal DailyBonus { get; set; }
    }

    public class AppSettings
    {
        public required string DiscordToken { get; set; }
        public required BotSettings Settings { get; set; }
    }
}
