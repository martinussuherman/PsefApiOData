namespace PsefApiOData.Misc
{
    /// <summary>
    /// Electronic signature configuration options.
    /// </summary>
    public class ElectronicSignatureOptions
    {
        /// <summary>
        /// Configuration options name.
        /// </summary>
        public const string OptionsName = "Signature";

        /// <summary>
        /// Gets or sets the Dirjen name.
        /// </summary>
        /// <value>The Dirjen name.</value>
        public string NamaDirjen { get; set; }
    }
}
