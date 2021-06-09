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
        public string OssBaseUri { get; set; }

        /// <summary>
        /// Gets or sets the OSS user.
        /// </summary>
        /// <value>The OSS user.</value>
        public string OssUser { get; set; }

        /// <summary>
        /// Gets or sets the OSS password.
        /// </summary>
        /// <value>The OSS password.</value>
        public string OssPassword { get; set; }

        /// <summary>
        /// Gets or sets the OSS cache time in hour.
        /// </summary>
        /// <value>The OSS cache time in hour.</value>
        public int OssCacheHour { get; set; }
    }
}