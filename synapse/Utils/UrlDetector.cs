using System;
using System.Text.RegularExpressions;

namespace synapse.Utils
{
    public static class UrlDetector
    {
        private static readonly Regex UrlRegex = new Regex(
            @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SimpleUrlRegex = new Regex(
            @"^(https?:\/\/|www\.)[^\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            // Check if it's a single line (URLs shouldn't contain newlines)
            if (text.Contains('\n') || text.Contains('\r'))
                return false;

            // First try the more comprehensive regex
            if (UrlRegex.IsMatch(text))
                return true;

            // Then try the simpler regex for cases like www.example.com
            if (SimpleUrlRegex.IsMatch(text))
                return true;

            // Also check for domain-only patterns like "example.com"
            if (IsDomainOnly(text))
                return true;

            return false;
        }

        private static bool IsDomainOnly(string text)
        {
            // Pattern for domain names like "google.com", "github.io", etc.
            var domainRegex = new Regex(
                @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return domainRegex.IsMatch(text);
        }

        public static string ExtractDomain(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                    return string.Empty;

                // Add protocol if missing for Uri parsing
                string normalizedUrl = url.Trim();
                if (!normalizedUrl.StartsWith("http://") && !normalizedUrl.StartsWith("https://"))
                {
                    normalizedUrl = "https://" + normalizedUrl;
                }

                var uri = new Uri(normalizedUrl);
                return uri.Host;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}