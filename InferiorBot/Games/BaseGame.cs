using Newtonsoft.Json;

namespace InferiorBot.Games
{
    public class BaseGame
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
