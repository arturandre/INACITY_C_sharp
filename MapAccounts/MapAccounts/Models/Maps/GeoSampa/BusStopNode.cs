using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Maps.GeoSampa
{
    //X,Y,pt_nome,pt_enderec,pt_descric
    public class BusStopNode
    {
        //Longitude
        public double X { get; set; }
        //Latitude
        public double Y { get; set; }
        public String pt_nome { get; set; }
        public String pt_enderec { get; set; }
        public String pt_descric { get; set; }
    }
}