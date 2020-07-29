using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PsefApi.Models
{
    public partial class Pemohon
    {
        /// <summary>
        /// Gets or sets the Pemohon OSS information.
        /// </summary>
        /// <value>The Pemohon's OSS information.</value>
        [NotMapped]
        public OssInfo OssInfo { get; set; } = new OssInfo();
    }
}