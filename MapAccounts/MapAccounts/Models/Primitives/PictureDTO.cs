using System;
using System.Collections.Generic;
using System.Drawing;

namespace MapAccounts.Models.Primitives
{
    public class PictureDTO
    {
        public PictureDTO()
        {
            filterResults = new List<FilterResultDTO>();
        }
        public static PictureDTO initializer(GSPicture picture)
        {
            return new PictureDTO(picture);
        }
        public PictureDTO(GSPicture picture)
        {
            filterResults = new List<FilterResultDTO>();
//            this.imageID = picture.ID;
            this.panoID = picture.ID;
            this.heading = picture.heading;
            this.base64image = picture.imageURI;
        }

        public int imageID { get; set; }
        public String panoID { get; set; }
        public double heading { get; set; }
        public string base64image { get; set; }
        public String imageURI { get; set; }
        public PointDTO location { get; set; }
        public Bitmap getImage()
        {
            Byte[] bitmapData = Convert.FromBase64String(FixBase64ForImage(base64image));
            System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
            return new Bitmap((Bitmap)Image.FromStream(streamBitmap));
        }

        private string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }
        public ICollection<FilterResultDTO> filterResults { get; set; }
    }
}