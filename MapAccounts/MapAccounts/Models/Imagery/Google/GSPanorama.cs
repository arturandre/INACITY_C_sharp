using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MapAccounts.Models
{
    public class GSPanorama
    {
        public GSPanorama() { }
        public GSPanorama(PanoramaDTO panorama)
        {
            this.panoID = panorama.pano;
            this.frontAngle = panorama.frontAngle;
            this.pitch = panorama.pitch;
            this.GSPicture = new List<GSPicture>();
            if (panorama.Pictures != null)
            foreach (var picture in panorama.Pictures)
            {
                GSPicture gspic = new Models.GSPicture(picture);
                this.GSPicture.Add(gspic);
            }
        }

        [Key]
        public int ID { get; set; }
        public String panoID { get; set; }
        public double frontAngle { get; set; }
        public double pitch { get; set; }
        //public int StreetPointModelID { get; set; }
        //[ForeignKey("StreetPointModelID")]
        //public virtual StreetPointModel StreetPointModel { get; set; }
        public virtual ICollection<GSPicture> GSPicture { get; set; }
    }
}