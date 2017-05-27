using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapAccounts.Models
{
    public class StreetPointModel
    {
        public StreetPointModel() { }

        [Key]
        public String ID { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public int StreetModelID { get; set; }
        [ForeignKey("StreetModelID")]
        public virtual StreetModel StreetModel { get; set; }
        public int? GSPanoramaID { get; set; }
        [ForeignKey("GSPanoramaID")]
        public virtual GSPanorama GSPanorama { get; set; }
    }
}