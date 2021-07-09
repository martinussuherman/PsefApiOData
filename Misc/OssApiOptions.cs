namespace PsefApiOData.Misc
{
    /// <summary>
    /// OSS API configuration options.
    /// </summary>
    public class OssApiOptions
    {
        /// <summary>
        /// Configuration options name.
        /// </summary>
        public const string OptionsName = "OssApi";

        /// <summary>
        /// Gets or sets the OSS base uri.
        /// </summary>
        /// <value>The OSS base uri.</value>
        public string BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the OSS user.
        /// </summary>
        /// <value>The OSS user.</value>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the OSS password.
        /// </summary>
        /// <value>The OSS password.</value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the OSS cache time in hour.
        /// </summary>
        /// <value>The OSS cache time in hour.</value>
        public int CacheHour { get; set; }
    }
}