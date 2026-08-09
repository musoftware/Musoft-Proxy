using System;

namespace ProxyForge.Core
{
    /// <summary>
    /// Represents the result of an asynchronous proxy test operation.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Gets or sets whether the proxy connectivity test was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the latency in milliseconds measured during the test.
        /// </summary>
        public int LatencyMs { get; set; } = -1;

        /// <summary>
        /// Gets or sets the external IP address returned by the test endpoint.
        /// </summary>
        public string IP { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message if the proxy test failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the proxy object tested.
        /// </summary>
        public ProxyInfo Proxy { get; set; } = null!;
    }
}
