using Newtonsoft.Json;
using System.Globalization;

namespace InferiorBot.Extensions
{
    public static class StringExtension
    {
        public static bool IsValidUrl(this string s)
        {
            return !string.IsNullOrWhiteSpace(s) && Uri.IsWellFormedUriString(s, UriKind.Absolute);
        }

        public static T? ToObject<T>(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? default : JsonConvert.DeserializeObject<T>(s);
        }

        public static string ToTitleCase(this string s, bool everyWord = false)
        {
            return string.IsNullOrWhiteSpace(s) ? s : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
        }
    }
}
