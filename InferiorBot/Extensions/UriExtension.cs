namespace InferiorBot.Extensions
{
    public static class UriExtension
    {
        public static string GetSubdomain(this Uri uri)
        {
            var host = uri.Host;
            var parts = host.Split('.');

            if (parts.Length <= 2) return string.Empty;
            if (parts[^2].Length == 2 && parts[^1].Length == 2)
            {
                if (parts.Length > 3) return string.Join('.', parts[..^3]);
            }
            else
            {
                return string.Join('.', parts[..^2]);
            }

            return string.Empty;
        }
    }
}
