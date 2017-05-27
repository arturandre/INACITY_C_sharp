using MapAccounts.Models.Primitives;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapAccounts.Models
{
    public class GSPicture
    {
        public GSPicture() { }
        public GSPicture(PictureDTO picture)
        {
            this.ID = picture.panoID;
            this.heading = picture.heading;
            this.imageURI = picture.base64image;
        }

        [Key]
        public String ID { get; set; }
        public virtual int GSPanoramaID { get; set; }
        [ForeignKey("GSPanoramaID")]
        public virtual GSPanorama Panorama { get; set; }
        public double heading { get; set; }
        public string imageURI { get; set; }
    }
}