namespace PsefApiOData.Models
{
    /// <summary>
    /// Represents a OSS API Response.
    /// </summary>
    public class OssSendLicenseResponse : OssResponse
    {
        /// <summary>
        /// Gets or sets the response license number.
        /// </summary>
        /// <value>The response license number.</value>
        public string LicenseNumber { get; set; }
    }
}
