using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProxyForge.Core
{
    /// <summary>
    /// Intelligently parses proxy strings in any common format into <see cref="ProxyInfo"/> instances.
    /// </summary>
    public static class ProxyParser
    {
        private static readonly Regex AuthAtHostRegex = new Regex(
            @"^(?:(?<scheme>http|https|socks5|socks4)://)?(?:(?<user>[^:@\s]+):(?<pass>[^@\s]+)@)?(?<host>[^:@\s]+):(?<port>\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Parses raw text input containing single or multiple proxy representations into a list of <see cref="ProxyInfo"/>.
        /// </summary>
        /// <param name="input">Raw text input (separated by newlines, commas, or semicolons).</param>
        /// <param name="defaultType">Default protocol type when not explicitly present in proxy string.</param>
        /// <returns>A list of successfully parsed <see cref="ProxyInfo"/> instances.</returns>
        public static List<ProxyInfo> Parse(string input, ProxyType defaultType = ProxyType.HTTP)
        {
            var result = new List<ProxyInfo>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            string[] lines = input.Split(new[] { '\r', '\n', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Ignore full line comments
                if (line.StartsWith("#") || line.StartsWith("//")) continue;

                // Handle inline comments
                int hashIdx = line.IndexOf('#');
                if (hashIdx >= 0) line = line.Substring(0, hashIdx).Trim();

                int doubleSlashIdx = line.IndexOf("//", StringComparison.Ordinal);
                if (doubleSlashIdx >= 0 && !line.Contains("://")) line = line.Substring(0, doubleSlashIdx).Trim();

                if (string.IsNullOrWhiteSpace(line)) continue;

                ProxyInfo? info = ParseSingleLine(line, defaultType);
                if (info != null)
                {
                    result.Add(info);
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts to parse a single line into a <see cref="ProxyInfo"/> object.
        /// </summary>
        /// <param name="line">Single cleaned input line.</param>
        /// <param name="defaultType">Default protocol type.</param>
        /// <returns>A valid <see cref="ProxyInfo"/> or null if parsing fails.</returns>
        public static ProxyInfo? ParseSingleLine(string line, ProxyType defaultType = ProxyType.HTTP)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            ProxyType detectedType = defaultType;

            // Scheme check
            if (line.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
            {
                detectedType = ProxyType.SOCKS5;
            }
            else if (line.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || line.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                detectedType = ProxyType.HTTP;
            }

            // 1. Try standard Uri parsing if scheme exists
            if (line.Contains("://"))
            {
                if (Uri.TryCreate(line, UriKind.Absolute, out Uri? uri) && uri != null)
                {
                    int port = uri.Port > 0 ? uri.Port : 80;
                    string user = string.Empty;
                    string pass = string.Empty;

                    if (!string.IsNullOrEmpty(uri.UserInfo))
                    {
                        string[] userInfoParts = uri.UserInfo.Split(new[] { ':' }, 2);
                        user = Uri.UnescapeDataString(userInfoParts[0]);
                        if (userInfoParts.Length > 1)
                        {
                            pass = Uri.UnescapeDataString(userInfoParts[1]);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(uri.Host) && port > 0 && port <= 65535)
                    {
                        return new ProxyInfo(uri.Host, port, user, pass, detectedType);
                    }
                }
            }

            // 2. Try auth@host:port or scheme://user:pass@host:port Regex
            Match match = AuthAtHostRegex.Match(line);
            if (match.Success)
            {
                string host = match.Groups["host"].Value;
                if (int.TryParse(match.Groups["port"].Value, out int port) && port > 0 && port <= 65535)
                {
                    string user = match.Groups["user"].Value;
                    string pass = match.Groups["pass"].Value;
                    string scheme = match.Groups["scheme"].Value;

                    if (!string.IsNullOrEmpty(scheme))
                    {
                        detectedType = scheme.Equals("socks5", StringComparison.OrdinalIgnoreCase) ? ProxyType.SOCKS5 : ProxyType.HTTP;
                    }

                    return new ProxyInfo(host, port, user, pass, detectedType);
                }
            }

            // 3. Try colon-separated formats: host:port or host:port:user:pass
            string cleaned = line;
            if (cleaned.Contains("://"))
            {
                cleaned = cleaned.Substring(cleaned.IndexOf("://", StringComparison.Ordinal) + 3);
            }

            string[] parts = cleaned.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                string host = parts[0].Trim();
                if (int.TryParse(parts[1].Trim(), out int port) && port > 0 && port <= 65535)
                {
                    return new ProxyInfo(host, port, detectedType);
                }
            }
            else if (parts.Length == 4)
            {
                string host = parts[0].Trim();
                if (int.TryParse(parts[1].Trim(), out int port) && port > 0 && port <= 65535)
                {
                    string user = parts[2].Trim();
                    string pass = parts[3].Trim();
                    return new ProxyInfo(host, port, user, pass, detectedType);
                }
            }

            return null;
        }
    }
}
