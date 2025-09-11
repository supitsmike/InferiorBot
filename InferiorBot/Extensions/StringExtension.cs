using Newtonsoft.Json;

namespace InferiorBot.Extensions
{
    public static class StringExtension
    {
        public static bool IsValidUrl(this string s)
        {
            return Uri.IsWellFormedUriString(s, UriKind.Absolute);
        }

        public static T? ToObject<T>(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? default : JsonConvert.DeserializeObject<T>(s);
        }
    }
}
