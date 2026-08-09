using System;
using System.Net.Http;

namespace ProxyForge.Core
{
    /// <summary>
    /// Detects IP bans, captchas, and rate-limiting responses from web targets.
    /// </summary>
    public class BanDetector
    {
        private static readonly string[] BanKeywords = new[]
        {
            "captcha",
            "cloudflare",
            "access denied",
            "blocked",
            "unusual traffic",
            "cf-challenge"
        };

        /// <summary>
        /// Evaluates whether an HTTP response or body indicates that the client IP / proxy has been banned or rate-limited.
        /// </summary>
        /// <param name="response">The HTTP response message (optional).</param>
        /// <param name="body">The HTTP response body text (optional).</param>
        /// <returns>True if a ban or challenge is detected; otherwise, false.</returns>
        public bool IsBanned(HttpResponseMessage? response, string? body)
        {
            if (response != null)
            {
                int statusCode = (int)response.StatusCode;
                if (statusCode == 403 || statusCode == 429 || statusCode == 503)
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(body))
            {
                string lowerBody = body!.ToLowerInvariant();
                foreach (var keyword in BanKeywords)
                {
                    if (lowerBody.Contains(keyword))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluates whether a response status code alone indicates a ban.
        /// </summary>
        public bool IsBanned(HttpResponseMessage? response)
        {
            return IsBanned(response, null);
        }

        /// <summary>
        /// Evaluates whether a body payload alone contains ban/challenge indicators.
        /// </summary>
        public bool IsBanned(string? body)
        {
            return IsBanned(null, body);
        }
    }
}
