using System;
using System.Text.Json.Serialization;

namespace ProxyForge.Core
{
    /// <summary>
    /// Represents detailed information for a single proxy server endpoint.
    /// </summary>
    public class ProxyInfo : ICloneable
    {
        /// <summary>
        /// Gets or sets the IP address or host name of the proxy server.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port number of the proxy server.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the username for proxy authentication (optional).
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for proxy authentication (optional).
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the protocol type (HTTP or SOCKS5).
        /// </summary>
        public ProxyType Type { get; set; } = ProxyType.HTTP;

        /// <summary>
        /// Gets or sets the latency in milliseconds measured during the last connectivity test.
        /// Returns -1 if untested or unreachable.
        /// </summary>
        public int LatencyMs { get; set; } = -1;

        /// <summary>
        /// Gets or sets whether the proxy is currently verified as live.
        /// null = Unchecked, true = Live, false = Dead.
        /// </summary>
        public bool? IsLive { get; set; } = null;

        /// <summary>
        /// Gets or sets the ISO country code of the proxy server location (optional).
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the anonymity level of the proxy (Elite, Anonymous, Transparent).
        /// </summary>
        public string AnonymityLevel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when this proxy was last checked.
        /// </summary>
        public DateTime? LastChecked { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this proxy was last used to fulfill a request.
        /// </summary>
        public DateTime LastUsed { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the consecutive failure count for this proxy.
        /// </summary>
        public int FailCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total number of times this proxy has been used.
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the count of successful requests fulfilled by this proxy since last rotation.
        /// </summary>
        public int SuccessfulRequestsCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether this proxy is explicitly banned.
        /// </summary>
        public bool IsBanned { get; set; } = false;

        /// <summary>
        /// Gets or sets the expiration timestamp of the current cooldown period.
        /// </summary>
        public DateTime CooldownUntil { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets whether this item represents a direct connection without a proxy server.
        /// </summary>
        public bool IsDirect { get; set; } = false;

        /// <summary>
        /// Singleton instance representing a direct connection without a proxy server.
        /// </summary>
        public static ProxyInfo Direct { get; } = new ProxyInfo("DIRECT", 0) { IsDirect = true, IsLive = true };

        /// <summary>
        /// Gets whether the proxy is currently in cooldown mode due to recent failures.
        /// </summary>
        public bool IsInCooldown => !IsDirect && DateTime.UtcNow < CooldownUntil;

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyInfo"/>.
        /// </summary>
        [JsonConstructor]
        public ProxyInfo() { }

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyInfo"/> with specified host and port.
        /// </summary>
        /// <param name="host">Host IP or domain name.</param>
        /// <param name="port">Port number.</param>
        /// <param name="type">Proxy protocol type.</param>
        public ProxyInfo(string host, int port, ProxyType type = ProxyType.HTTP)
        {
            Host = host;
            Port = port;
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyInfo"/> with authentication details.
        /// </summary>
        /// <param name="host">Host IP or domain name.</param>
        /// <param name="port">Port number.</param>
        /// <param name="username">Username for authentication.</param>
        /// <param name="password">Password for authentication.</param>
        /// <param name="type">Proxy protocol type.</param>
        public ProxyInfo(string host, int port, string username, string password, ProxyType type = ProxyType.HTTP)
            : this(host, port, type)
        {
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
        }

        /// <summary>
        /// Formats the proxy connection details into a standard URL string representation.
        /// </summary>
        /// <returns>Formatted proxy URL string (e.g. http://user:pass@host:port).</returns>
        public string ToUrl()
        {
            if (IsDirect) return "direct://";
            string scheme = Type == ProxyType.SOCKS5 ? "socks5" : "http";
            if (!string.IsNullOrEmpty(Username))
            {
                return $"{scheme}://{Username}:{Password}@{Host}:{Port}";
            }
            return $"{scheme}://{Host}:{Port}";
        }

        /// <summary>
        /// Returns a compact string representation of the proxy in host:port or host:port:user:pass format.
        /// </summary>
        /// <returns>Compact proxy string.</returns>
        public override string ToString()
        {
            if (IsDirect) return "DIRECT";
            if (!string.IsNullOrEmpty(Username))
            {
                return $"{Host}:{Port}:{Username}:{Password}";
            }
            return $"{Host}:{Port}";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is ProxyInfo other)
            {
                if (IsDirect || other.IsDirect)
                {
                    return IsDirect == other.IsDirect;
                }

                return Type == other.Type &&
                       string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase) &&
                       Port == other.Port &&
                       string.Equals(Username, other.Username, StringComparison.Ordinal) &&
                       string.Equals(Password, other.Password, StringComparison.Ordinal);
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (IsDirect) return "DIRECT".GetHashCode();
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (Host != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Host) : 0);
                hash = hash * 31 + Port.GetHashCode();
                hash = hash * 31 + (Username != null ? Username.GetHashCode() : 0);
                hash = hash * 31 + (Password != null ? Password.GetHashCode() : 0);
                hash = hash * 31 + Type.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="ProxyInfo"/> object.
        /// </summary>
        /// <returns>A new cloned instance of <see cref="ProxyInfo"/>.</returns>
        public ProxyInfo Clone()
        {
            return (ProxyInfo)MemberwiseClone();
        }

        object ICloneable.Clone() => Clone();
    }
}
