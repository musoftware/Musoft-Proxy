using System;

namespace SmartProxyManager.WinForms.Resources
{
    /// <summary>
    /// Resource strings container for SmartProxyManager UI elements.
    /// </summary>
    public static class Strings
    {
        public const string EnableProxy = "Enable Proxy";
        public const string Type = "Type:";
        public const string Rotation = "Rotation:";
        public const string ModeLabel = "Mode:";
        public const string RotateAfterLabel = "Rotate After:";
        public const string StrategyLabel = "Strategy: {0}";
        public const string SingleProxyHeader = " Add Single Proxy ";

        public const string HostLabel = "Host / IP:";
        public const string PortLabel = "Port:";
        public const string UsernameLabel = "Username:";
        public const string PasswordLabel = "Password:";
        public const string AddProxy = "Add Proxy";

        public const string PasteFromClipboard = "Paste Clipboard";
        public const string ImportTxt = "Import .txt";
        public const string RemoveSelected = "Remove Selected";
        public const string ClearAll = "Clear All";
        public const string TestSelected = "Test Selected";
        public const string TestAll = "Test All";
        public const string SaveList = "Save List...";
        public const string LoadList = "Load List...";
        public const string Cancel = "Cancel";

        public const string ColHost = "Host";
        public const string ColPort = "Port";
        public const string ColUsername = "Username";
        public const string ColType = "Type";
        public const string ColStatus = "Status";
        public const string ColLatency = "Latency";

        public const string StatusReady = "Status: Ready";
        public const string StatusTesting = "Status: Testing proxies ({0}/{1})...";
        public const string StatusTestingDone = "Status: Testing completed ({0} Live, {1} Dead)";
        public const string StatusUnchecked = "Unchecked";
        public const string StatusLive = "Live";
        public const string StatusDead = "Dead";

        public const string OpenFileDialogFilter = "Text Files (*.txt)|*.txt|JSON Files (*.json)|*.json|All Files (*.*)|*.*";
        public const string SaveFileDialogFilter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

        public const string FormTitle = "Smart Proxy Manager";
    }
}
