using System;
using System.Collections.Generic;

namespace PsefApi.Models
{
    public partial class Pemohon
    {
        public uint Id { get; set; }
        public string UserId { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Nib { get; set; }
        public string ApotekerName { get; set; }
        public string ApotekerEmail { get; set; }
        public string ApotekerPhone { get; set; }
        public string StraNumber { get; set; }
        public string StraUrl { get; set; }
    }
}
