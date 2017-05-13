using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Primitives
{
    public class AmenityDTO : PointDTO
    {
        public string name { get; set; }
        public string address { get; set; }
    }
}