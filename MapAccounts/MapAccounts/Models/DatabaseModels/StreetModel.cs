using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MapAccounts.Models
{
    public class StreetModel
    {
        public StreetModel() { }
        [Key]
        public int ID { get; set; }
        public String Name { get; set; }
        public virtual ICollection<RegionModel> RegionModel { get; set; }
        //public virtual ICollection<SegmentModel> SegmentModel { get; set; }
        public virtual ICollection<ICollection<StreetPointModel>> StreetTrechosModel { get; set; }
    }
}