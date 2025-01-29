namespace InferiorBot.Extensions
{
    public static class DictionaryExtension
    {
        public static bool DoesValueExist(this Dictionary<string, string> dictionary, string value, string? keyToIgnore = null)
        {
            return dictionary.Where(x => keyToIgnore == null || x.Key != keyToIgnore).Any(x => x.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
