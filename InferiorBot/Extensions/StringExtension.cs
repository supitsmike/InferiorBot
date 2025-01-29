namespace InferiorBot.Extensions
{
    public static class StringExtension
    {
        public static bool IsValidUrl(this string s)
        {
            return Uri.IsWellFormedUriString(s, UriKind.Absolute);
        }
    }
}
