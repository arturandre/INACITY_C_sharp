using System.Collections.Generic;

namespace MapAccounts.Models.Primitives
{
    public class PanoramaDTO
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public PanoramaDTO()
        {
            Pictures = new List<PictureDTO>();
        }
        public PanoramaDTO(GSPanorama modelPanorama)
        {
            frontAngle = modelPanorama.frontAngle;
            pitch = modelPanorama.pitch;
            pano = modelPanorama.panoID;
            Pictures = new List<PictureDTO>();
            foreach(var picture in modelPanorama.GSPicture)
            {
                Pictures.Add(new PictureDTO(picture));
            }
        }

        public double frontAngle { get; set; }
        public double pitch { get; set; }
        public string pano { get; set; }
        public ICollection<PictureDTO> Pictures { get; set; }
    }
}