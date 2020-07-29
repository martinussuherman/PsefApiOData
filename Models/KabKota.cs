using System;
using System.Collections.Generic;

namespace PsefApi.Models
{
    public partial class Kabkota
    {
        public ushort Id { get; set; }
        public string Name { get; set; }
        public byte? ProvinsiId { get; set; }

        public virtual Provinsi Provinsi { get; set; }
    }
}
