using System;
using System.Threading;

namespace ProxyForge.Core
{
    /// <summary>
    /// Represents a sticky proxy session context for binding all HTTP requests in an async scope to a single proxy.
    /// </summary>
    public sealed class ProxySession : IDisposable
    {
        private static readonly AsyncLocal<ProxySession?> _currentSession = new AsyncLocal<ProxySession?>();

        /// <summary>
        /// Gets the current active sticky proxy session in the async execution context.
        /// </summary>
        public static ProxySession? Current => _currentSession.Value;

        /// <summary>
        /// Gets the target proxy bound to this sticky session.
        /// </summary>
        public ProxyInfo Proxy { get; }

        /// <summary>
        /// Gets the parent proxy manager.
        /// </summary>
        public ProxyManager Manager { get; }

        private readonly ProxySession? _parentSession;
        private bool _disposed;

        internal ProxySession(ProxyManager manager, ProxyInfo proxy)
        {
            Manager = manager;
            Proxy = proxy;
            _parentSession = _currentSession.Value;
            _currentSession.Value = this;
        }

        /// <summary>
        /// Marks the proxy in this session as failed (e.g. timeout or connection drop).
        /// </summary>
        public void MarkFailed(Exception? exception = null)
        {
            if (Proxy != null) Manager.MarkAsDead(Proxy);
        }

        /// <summary>
        /// Marks the proxy in this session as successful.
        /// </summary>
        public void MarkSuccess(double responseTimeMs = 0)
        {
            if (Proxy != null) Manager.MarkAsSuccess(Proxy);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _currentSession.Value = _parentSession;
            }
        }
    }
}
