using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Primitives.Converters
{
    public static class Base64Converter
    {
        public static String BgrImageToBase64(Image<Bgr, byte> bgrImg)
        {
            return BitmapToBase64(bgrImg.ToBitmap());
        }

        public static String BitmapToBase64(Bitmap img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }
}