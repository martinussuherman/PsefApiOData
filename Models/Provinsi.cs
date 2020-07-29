using System;
using System.Collections.Generic;

namespace PsefApi.Models
{
    public partial class Provinsi
    {
        public Provinsi()
        {
            Kabkota = new HashSet<Kabkota>();
        }

        public byte Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Kabkota> Kabkota { get; set; }
    }
}
