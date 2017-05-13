using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Maps.OSM
{
    public class way : Element
    {
        public ICollection<Int64> nodes { get; set; }
    }
}