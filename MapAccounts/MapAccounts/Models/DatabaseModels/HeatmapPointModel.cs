using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace MapAccounts.Models
{
    public class HeatmapPointModel
    {
        [Key]
        public String ID { get; set; }
        public DbGeography coordinates { get; set; }
        public float? TreesDensity { get; set; }
        public float? CracksDensity { get; set; }
    }
}