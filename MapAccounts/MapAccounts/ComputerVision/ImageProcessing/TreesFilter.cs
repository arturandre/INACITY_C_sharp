using Emgu.CV.Structure;
using System;
using System.Linq;
using Emgu.CV;
using System.IO;
using MapAccounts.Helpers;
using Emgu.CV.CvEnum;
using MapAccounts.Models.Primitives;
using MapAccounts.Models.Primitives.Converters;

namespace MapAccounts.ComputerVision.ImageProcessing
{
    public class TreesFilter : ImageFilter
    {
        public override FilterResultDTO.CaracteristicType FilterType
        {
            get
            {
                return FilterResultDTO.CaracteristicType.Trees;
            }
        }

        public override FilterResultDTO filterImage(Image<Bgr, byte> Image)
        {
            FilterResultDTO filterDTO = new FilterResultDTO();

            var imageMask = Mask(Image);
            

            filterDTO.Density = imageMask.CountNonzero()[0] / ((double)imageMask.Width * imageMask.Height);
            filterDTO.Type = FilterResultDTO.CaracteristicType.Trees;
            var mask = ImageHelper.MaskOverlay(Image, imageMask);
            var img = mask.ToBitmap();
            mask.Dispose();

            filterDTO.base64image = Base64Converter.BitmapToBase64(img);

            imageMask.Dispose();

            return filterDTO;
        }

        private Image<Gray, byte> Mask(Image<Bgr, byte> Image)
        {
            var hsvOriginalImage = Image.Convert<Hsv, byte>();
            var hsvSplit = hsvOriginalImage.Split();
            var hueChannel = hsvSplit[0];
            String LUTPath = PathMap.MapPath(@"~/Content/LUTTrees.txt");
            var LUT = new Image<Gray, byte>(256, 1, new Gray(0));
            using (StreamReader sr = new StreamReader(LUTPath))
            {
                var binaryString = sr.ReadToEnd().Split(',');
                for (int i = 0; i < binaryString.Count(); i++)
                {
                    LUT.Data[0, i, 0] = byte.Parse(binaryString[i]);
                }
            }
            CvInvoke.LUT(hueChannel, LUT, hueChannel);

            var cromaImage = ImageHelper.Bgr2Croma(Image);

            #region Morphology_mask
            var strel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle,
                new System.Drawing.Size(15, 3), new System.Drawing.Point(-1, -1));
            var gImage = cromaImage.Convert<Gray, byte>();
            CvInvoke.MorphologyEx(gImage, gImage,
                Emgu.CV.CvEnum.MorphOp.Blackhat,
                strel,
                new System.Drawing.Point(-1, -1),
                1, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0));
            var mean = CvInvoke.Mean(gImage);
            gImage._ThresholdBinary(new Gray(mean.V0 / 2), new Gray(255));
            strel.Dispose();
            #endregion Morphology_mask

            #region Color_mask

            var hsvImage = cromaImage.Convert<Hsv, byte>();
            var hsvMask = hsvImage.InRange(new Hsv(18, 0, 0), new Hsv(72, 255, 255));


            #endregion Color_mask

            var combinedMasks = gImage.And(hsvMask);

            /*Dilatação infinita*/
            var aux = hueChannel.Clone();
            while (CvInvoke.CountNonZero(aux) > 0)
            {
                hueChannel.CopyTo(aux);
                CvInvoke.Dilate(hueChannel, hueChannel, 
                    CvInvoke.GetStructuringElement(
                        ElementShape.Rectangle, 
                        new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1)), new System.Drawing.Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
                CvInvoke.BitwiseAnd(hueChannel, combinedMasks, hueChannel);
                CvInvoke.Subtract(aux, hueChannel, aux);
            }
            hueChannel.CopyTo(combinedMasks);

            //Noise treatment
            combinedMasks._MorphologyEx(
                Emgu.CV.CvEnum.MorphOp.Close,
                CvInvoke.GetStructuringElement(
                    Emgu.CV.CvEnum.ElementShape.Rectangle,
                    new System.Drawing.Size(5, 5),
                    new System.Drawing.Point(-1, -1)),
                new System.Drawing.Point(-1, -1),
                1,
                Emgu.CV.CvEnum.BorderType.Constant,
                new MCvScalar(0));

            combinedMasks._MorphologyEx(
                Emgu.CV.CvEnum.MorphOp.Open,
                CvInvoke.GetStructuringElement(
                    Emgu.CV.CvEnum.ElementShape.Rectangle,
                    new System.Drawing.Size(19, 19),
                    new System.Drawing.Point(-1, -1)),
                new System.Drawing.Point(-1, -1),
                1,
                Emgu.CV.CvEnum.BorderType.Constant,
                new MCvScalar(0));
            aux.Dispose();
            hsvImage.Dispose();
            hsvOriginalImage.Dispose();
            hsvSplit[0].Dispose();
            hsvSplit[1].Dispose();
            hsvSplit[2].Dispose();
            hsvMask.Dispose();
            gImage.Dispose();
            cromaImage.Dispose();
            return combinedMasks;
        }


    }
}