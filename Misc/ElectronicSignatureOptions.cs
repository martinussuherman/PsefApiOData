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
        /// Gets or sets the signer name.
        /// </summary>
        /// <value>The signer name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the signer position.
        /// </summary>
        /// <value>The signer position.</value>
        public string Position { get; set; }
    }
}
