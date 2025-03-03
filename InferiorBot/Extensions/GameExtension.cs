using InferiorBot.Games;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace InferiorBot.Extensions
{
    public static class GameExtension
    {
        public static GuessTheNumber? GetGuessTheNumberData(this Game game)
        {
            return JsonConvert.DeserializeObject<GuessTheNumber>(game.GameData);
        }
    }
}
