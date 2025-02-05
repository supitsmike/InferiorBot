using InferiorBot.Extensions;
using System.Security.Cryptography;

namespace InferiorBot.Classes
{
    public class Methods
    {
        public static string ConvertUrl(string url, string? type, out string website)
        {
            website = string.Empty;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return string.Empty;

            var subdomain = uri.GetSubdomain();
            var host = uri.Host.StartsWith(subdomain, StringComparison.OrdinalIgnoreCase)
                ? uri.Host[(subdomain.Length > 0 ? subdomain.Length + 1 : 0)..]
                : uri.Host;

            var instagramUrls = new Dictionary<string, string>
            {
                { "instagram", "instagram.com" },
                { "InstaFix", "ddinstagram.com" },
                { "EmbedEZ", "instagramez.com" }
            };
            var twitterUrls = new Dictionary<string, string>
            {
                { "x", "x.com" },
                { "twitter", "twitter.com" },
                { "TwitFix", "vxtwitter.com" },
                { "EmbedEZ", "twitterez.com" }
            };
            var tiktokUrls = new Dictionary<string, string>
            {
                { "tiktok", "tiktok.com" },
                { "vxTiktok", "vxtiktok.com" },
                { "EmbedEZ", "tiktokez.com" }
            };
            var redditUrls = new Dictionary<string, string>
            {
                { "reddit", "reddit.com" },
                { "FixReddit", "rxddit.com" },
                { "EmbedEZ", "redditez.com" }
            };

            if (string.IsNullOrEmpty(type) && host.Equals(instagramUrls["instagram"], StringComparison.OrdinalIgnoreCase))
            {
                website = "instagram";
                type = "EmbedEZ";
            }
            if (string.IsNullOrEmpty(type) && (host.Equals(twitterUrls["x"], StringComparison.OrdinalIgnoreCase) ||
                                               host.Equals(twitterUrls["twitter"], StringComparison.OrdinalIgnoreCase)))
            {
                website = "twitter";
                type = "TwitFix";
            }
            if (string.IsNullOrEmpty(type) && host.Equals(tiktokUrls["tiktok"], StringComparison.OrdinalIgnoreCase))
            {
                website = "tiktok";
                type = "vxTiktok";
            }
            if (string.IsNullOrEmpty(type) && host.Equals(redditUrls["reddit"], StringComparison.OrdinalIgnoreCase))
            {
                website = "reddit";
                type = "FixReddit";
            }

            switch (type)
            {
                case "instagram":
                    if (instagramUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "instagram.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;
                case "InstaFix":
                    if (instagramUrls.DoesValueExist(host, type))
                    {
                        subdomain = "g";
                        host = "ddinstagram.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;

                case "x":
                    if (twitterUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "x.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;
                case "twitter":
                    if (twitterUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "twitter.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;
                case "TwitFix":
                    if (twitterUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "vxtwitter.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;

                case "tiktok":
                    if (tiktokUrls.DoesValueExist(host, type))
                    {
                        subdomain = subdomain.Length > 0 ? subdomain : "www";
                        host = "tiktok.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;
                case "vxTiktok":
                    if (tiktokUrls.DoesValueExist(host, type))
                    {
                        subdomain = subdomain.Length > 0 ? subdomain : "www";
                        host = "vxtiktok.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;

                case "reddit":
                    if (redditUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "reddit.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;
                case "FixReddit":
                    if (redditUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "rxddit.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;

                case "EmbedEZ":
                    if (instagramUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "instagramez.com";
                        break;
                    }
                    if (twitterUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "twitterez.com";
                        break;
                    }
                    if (tiktokUrls.DoesValueExist(host, type))
                    {
                        subdomain = subdomain.Length > 0 ? subdomain : "www";
                        host = "tiktokez.com";
                        break;
                    }
                    if (redditUrls.DoesValueExist(host, type))
                    {
                        subdomain = "www";
                        host = "redditez.com";
                        break;
                    }
                    subdomain = string.Empty;
                    host = string.Empty;
                    break;

                default: return string.Empty;
            }
            if (string.IsNullOrEmpty(subdomain) && string.IsNullOrEmpty(host)) return string.Empty;

            return $"{new UriBuilder(uri) { Host = $"{subdomain}{(subdomain.Length > 0 ? "." : string.Empty)}{host}", Query = string.Empty, Port = -1}}";
        }

        public static int GenerateRandomNumber(int minValue, int maxValue = -1)
        {
            var randomBytes = new byte[sizeof(int)];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var seed = BitConverter.ToInt32(randomBytes, 0);
            return maxValue != -1 ? new Random(seed).Next(minValue, maxValue + 1) : new Random(seed).Next(minValue);
        }
    }
}
