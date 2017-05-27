using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Primitives
{
    public class HeatMapPointDTO
    {
        public PointDTO location { get; set; }
        public double? CracksDensity { get; set; }
        public double? TreesDensity { get; set; }
    }
}



/*
 {
    "ID": "_azoL1rUfXvg_ECdVk-KMA",
    "coordinates": {
      "Geography": {
        "CoordinateSystemId": 4326,
        "WellKnownText": "POINT (-46.7531242370605 -23.5638427734375)"
      }
    },
    "TreesDensity": 0.00848877,
    "CracksDensity": null
  },
     
     */
