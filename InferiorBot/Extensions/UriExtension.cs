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

        public static string RemoveQuery(this Uri uri)
        {
            return $"{new UriBuilder(uri) { Host = uri.Host, Query = string.Empty, Port = -1 }}";
        }

        public static string RemoveQueryAndNormalize(this Uri uri, bool normalizePath)
        {
            return $"{new UriBuilder(uri) { Scheme = uri.Scheme.ToLower(), Host = uri.Host.ToLower(), Path = normalizePath ? uri.AbsolutePath.ToLower() : uri.AbsolutePath, Query = string.Empty, Port = -1 }}";
        }

        public static async Task<Uri?> ResolveRedirectAsync(this Uri uri, CancellationToken cancellationToken = default)
        {
            try
            {
                using var httpClient = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = false
                });
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                var response = await httpClient.GetAsync(uri, cancellationToken);

                if (response.StatusCode != System.Net.HttpStatusCode.MovedPermanently &&
                    response.StatusCode != System.Net.HttpStatusCode.Found &&
                    response.StatusCode != System.Net.HttpStatusCode.SeeOther &&
                    response.StatusCode != System.Net.HttpStatusCode.TemporaryRedirect &&
                    response.StatusCode != System.Net.HttpStatusCode.PermanentRedirect) return uri;

                var location = response.Headers.Location;
                if (location == null) return uri;

                if (location.IsAbsoluteUri == false)
                {
                    location = new Uri(uri, location);
                }
                return location;
            }
            catch
            {
                return uri;
            }
        }
    }
}
