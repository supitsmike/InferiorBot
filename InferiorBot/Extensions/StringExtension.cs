using Newtonsoft.Json;
using System.Globalization;

namespace InferiorBot.Extensions
{
    public static class StringExtension
    {
        public static bool IsValidUrl(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (Uri.IsWellFormedUriString(s, UriKind.Absolute) == false) return false;

            if (!Uri.TryCreate(s, UriKind.Absolute, out var uri)) return false;
            if (uri.Scheme != "http" && uri.Scheme != "https") return false;

            return true;
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
