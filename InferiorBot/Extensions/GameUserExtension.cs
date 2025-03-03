using InferiorBot.Games;
using Infrastructure.InferiorBot;
using Newtonsoft.Json;

namespace InferiorBot.Extensions
{
    public static class GameUserExtension
    {
        public static int GetGuessTheNumberAnswer(this GameUser gamesUser)
        {
            var userData = JsonConvert.DeserializeObject<GuessTheNumberUser>(gamesUser.UserData);
            return userData?.Number ?? int.MaxValue;
        }
    }
}
