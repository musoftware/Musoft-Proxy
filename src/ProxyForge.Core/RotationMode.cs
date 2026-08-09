using System;

namespace ProxyForge.Core
{
    /// <summary>
    /// Specifies proxy rotation triggers and strategies.
    /// </summary>
    public enum RotationMode
    {
        /// <summary>
        /// Rotate to a new proxy on every request.
        /// </summary>
        EveryRequest,

        /// <summary>
        /// Rotate to a new proxy after N requests.
        /// </summary>
        EveryNRequests,

        /// <summary>
        /// Rotate to a new proxy after N seconds elapsed.
        /// </summary>
        EveryNSeconds,

        /// <summary>
        /// Sticky session proxy binding.
        /// </summary>
        Sticky,

        /// <summary>
        /// Sequential round-robin selection.
        /// </summary>
        RoundRobin,

        /// <summary>
        /// Random selection from active pool.
        /// </summary>
        Random
    }
}
