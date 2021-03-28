using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DrAvail.Models
{
    public class Location
    {
        public int LocationID { get; set; }
        public string Locality { get; set; }

        public int Pincode { get; set; }

        public string District { get; set; }

        public string State { get; set; }
    }
}
