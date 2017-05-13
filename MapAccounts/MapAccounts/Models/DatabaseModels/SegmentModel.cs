using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.DatabaseModels
{
    public class SegmentModel
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("StreetModelID")]
        public virtual StreetModel StreetModel { get; set; }
        public virtual int StreetModelID { get; set; }
        public virtual ICollection<StreetPointModel> StreetPointModel { get; set; }
    }
}