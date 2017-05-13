using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Maps.OSM
{
    public class node : Element
    {
        public decimal lat { get; set; }
        public decimal lon { get; set; }

    }
}