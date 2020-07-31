using System;
using System.Collections.Generic;

namespace PsefApi.Models
{
    /// <summary>
    /// Represents a OSS Information.
    /// </summary>
    public class OssInfo
    {
        /// <summary>
        /// Gets or sets the OSS Information NIB.
        /// </summary>
        /// <value>The OSS Information's NIB.</value>
        public string Nib { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information name.
        /// </summary>
        /// <value>The OSS Information's name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information address.
        /// </summary>
        /// <value>The OSS Information's address.</value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information NPWP.
        /// </summary>
        /// <value>The OSS Information's NPWP.</value>
        public string Npwp { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information SIUP.
        /// </summary>
        /// <value>The OSS Information's SIUP.</value>
        public string Siup { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information director name.
        /// </summary>
        /// <value>The OSS Information's director name.</value>
        public string Director { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information capital source type.
        /// </summary>
        /// <value>The OSS Information's capital source type.</value>
        public int CapitalSourceType { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information company type.
        /// </summary>
        /// <value>The OSS Information's company type.</value>
        public int CompanyType { get; set; }

        /// <summary>
        /// Gets or sets the OSS Information legal entity type.
        /// </summary>
        /// <value>The OSS Information's legal entity type.</value>
        public int LegalEntityType { get; set; }
    }
}