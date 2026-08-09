using System;
using System.Windows.Forms;
using ProxyForge.Core;

namespace ProxyForge.WinForms
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
            FormClosing += ProxyManagerForm_FormClosing;
        }

        private void ProxyManagerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                Manager.SaveToStorage();
            }
        }
    }
}
