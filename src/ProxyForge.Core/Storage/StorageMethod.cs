namespace ProxyForge.Core
{
    /// <summary>
    /// Specifies the storage mechanism used by ProxyManager for proxy persistence.
    /// </summary>
    public enum StorageMethod
    {
        /// <summary>
        /// In-memory storage with no external file or database persistence.
        /// </summary>
        InMemory,

        /// <summary>
        /// JSON file storage on local disk.
        /// </summary>
        JsonFile,

        /// <summary>
        /// Custom user-provided API, database, or delegate storage implementation.
        /// </summary>
        CustomApi
    }
}
