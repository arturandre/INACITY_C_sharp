using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapAccounts.Models
{
    public class RegionModel
    {
        public RegionModel() { }

        [Key]
        public int ID { get; set; }
        public String ApplicationUserID { get; set; }
        [ForeignKey("ApplicationUserID")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public Bound Bounds { get; set; }
        public PointDTO getCenter()
        {
            return (new PointDTO()
            {
                lat = (Bounds.North + Bounds.South) / 2.0,
                lng = (Bounds.East + Bounds.West) / 2.0
            });
        }

        public virtual ICollection<StreetModel> StreetModel { get; set; }
    }
}