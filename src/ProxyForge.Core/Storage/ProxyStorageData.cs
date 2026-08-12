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
        /// Gets or sets whether direct connection is included in pool rotation and allowed as fallback.
        /// </summary>
        public bool AllowDirectFallback { get; set; } = true;

        /// <summary>
        /// Gets or sets maximum successful requests per proxy before automatic rotation.
        /// </summary>
        public int MaxSuccessfulRequestsPerProxy { get; set; } = 8;

        /// <summary>
        /// Gets or sets whether to rotate immediately to the next proxy when a request fails or returns an empty response.
        /// </summary>
        public bool RotateOnFailure { get; set; } = true;

        /// <summary>
        /// Gets or sets whether automatic background proxy fetching is enabled.
        /// </summary>
        public bool EnableAutoFetch { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of proxy server endpoints.
        /// </summary>
        public List<ProxyInfo> Proxies { get; set; } = new List<ProxyInfo>();
    }
}
