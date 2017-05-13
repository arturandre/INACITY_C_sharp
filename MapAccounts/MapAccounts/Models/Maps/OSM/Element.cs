using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Maps.OSM
{
    public class Element
    {
        public Int64 id { get; set; }
        public String type { get; set; }
        public Dictionary<String, String> tags { get; set; }
    }
}