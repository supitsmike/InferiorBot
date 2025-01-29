namespace InferiorBot.Extensions
{
    public static class DictionaryExtension
    {
        public static bool DoesValueExist(this Dictionary<string, string> dictionary, string value, string? keyToIgnore = null)
        {
            return dictionary.Where(kvp => keyToIgnore == null || kvp.Key != keyToIgnore).Any(kvp => kvp.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
