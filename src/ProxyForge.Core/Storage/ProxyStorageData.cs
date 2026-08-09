using System.Collections.Generic;

namespace ProxyForge.Core
{
    /// <summary>
    /// Represents the complete persistence payload container including configuration settings and proxy pool items.
    /// </summary>
    public class ProxyStorageData
    {
        /// <summary>
        /// Gets or sets whether proxy routing is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the default proxy protocol type.
        /// </summary>
        public ProxyType DefaultType { get; set; } = ProxyType.HTTP;

        /// <summary>
        /// Gets or sets the rotation mode.
        /// </summary>
        public RotationMode RotationMode { get; set; } = RotationMode.RoundRobin;

        /// <summary>
        /// Gets or sets request rotation threshold interval.
        /// </summary>
        public int RotateAfter { get; set; } = 10;

        /// <summary>
        /// Gets or sets the list of proxy server endpoints.
        /// </summary>
        public List<ProxyInfo> Proxies { get; set; } = new List<ProxyInfo>();
    }
}
