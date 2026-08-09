using System;
using System.Windows.Forms;
using SmartProxyManager.Core;

namespace SmartProxyManager.WinForms
{
    /// <summary>
    /// Standalone Form container wrapping <see cref="ProxyManagerControl"/> for easy modal or modeless display.
    /// </summary>
    public partial class ProxyManagerForm : Form
    {
        /// <summary>
        /// Gets the embedded <see cref="ProxyManagerControl"/> instance.
        /// </summary>
        public ProxyManagerControl ProxyControl => proxyControl;

        /// <summary>
        /// Gets the underlying <see cref="ProxyManager"/>.
        /// </summary>
        public ProxyManager Manager => proxyControl.Manager;

        /// <summary>
        /// Initializes a new instance of <see cref="ProxyManagerForm"/>.
        /// </summary>
        public ProxyManagerForm()
        {
            InitializeComponent();
        }
    }
}
