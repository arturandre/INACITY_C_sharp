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

        public Image<Gray, byte> Mask(Image<Bgr, byte> Image)
        {
            var lowH = 1.0 / 8.0;
            var highH = 1.0 / 2.0;
            var lowS = 32.0 / 100.0;
            var highS = 1.0;

            lowH *= 255.0;
            highH *= 255.0;
            lowS *= 255.0;
            highS *= 255.0;

            var hsl = ImageHelper.Bgr2Hsl(Image);
            var hslSplit = hsl.Split();
            hslSplit[1]._Mul(2.0);
            hsl.Dispose();
            hsl = new Image<Bgr, byte>(hslSplit);
            Image = ImageHelper.Hsl2Bgr(Image);

            var hsvOriginalImage = Image.Convert<Hsv, byte>();
            var hsvSplit = hsvOriginalImage.Split();
            var hueChannel = hsvSplit[0];
            var hueChannelUINT16 = hueChannel.Convert<Gray, UInt16>();
            String LUTPath = PathMap.MapPath(@"~/Content/LUTTrees.txt");
            var LUT = new Image<Gray, UInt16>(65536, 1, new Gray(0));
            using (StreamReader sr = new StreamReader(LUTPath))
            {
                var binaryString = sr.ReadToEnd().Split(',');
                for (int i = 0; i < binaryString.Count(); i++)
                {
                    LUT.Data[0, i, 0] = byte.Parse(binaryString[i]);
                }
            }
            for (int i = 0; i < hueChannelUINT16.Rows; i++)
            {
                for (int j = 0; j < hueChannelUINT16.Cols; j++)
                {
                    hueChannelUINT16.Data[i, j, 0] = LUT.Data[0, hueChannelUINT16.Data[i, j, 0], 0];
                }
            }
            
            var maskLUT = hueChannelUINT16.Convert<Gray, byte>();

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
            var hsvMask = hsvImage.InRange(new Hsv(lowH, lowS, 0), new Hsv(highH, highS, 255));


            #endregion Color_mask

            var combinedMasks = gImage.And(hsvMask).Or(maskLUT);
            /*Dilatação infinita*/
            /*
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
            hueChannel.Convert<Gray, byte>().CopyTo(combinedMasks);
            */

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


        public Image<Bgr, byte> MaskTest(Image<Bgr, byte> Image)
        {
            var floatImg = Image.Convert<Bgr, float>();
            floatImg._Mul(1.0 / 256.0);

            var lowH = 0.0001*180;
            var highH = 1.0250*180;
            var lowS = 0.0001;
            var highS = 0.9500;

            var str_elem_close = 6;
            var str_elem_open = 24;
            var str_elem_gradient_width = 15;
            var str_elem_gradient_height = 9;
            var gradient_factor = 0.4654;
            

            var cromaImage = ImageHelper.Bgr2Croma(floatImg);

            #region Morphology_mask
            var strel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle,
                new System.Drawing.Size(
                    str_elem_gradient_width , 
                    str_elem_gradient_height), new System.Drawing.Point(-1, -1));
            var gImage = cromaImage.Convert<Gray, float>();
            CvInvoke.MorphologyEx(gImage, gImage,
                Emgu.CV.CvEnum.MorphOp.Blackhat,
                strel,
                new System.Drawing.Point(-1, -1),
                1, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0));
            var mean = CvInvoke.Mean(gImage);
            gImage._ThresholdBinary(new Gray(mean.V0*gradient_factor), new Gray(1));
            strel.Dispose();
            #endregion Morphology_mask

            #region Color_mask

            var hsvImage = cromaImage.Convert<Hsv, float>();
            var hsvMask = hsvImage.InRange (new Hsv(lowH, lowS, 0), new Hsv(highH, highS, 1));


            #endregion Color_mask

            var combinedMasks = gImage.Mul(hsvMask.Convert<Gray, float>());
            

            //Noise treatment
            combinedMasks._MorphologyEx(
                Emgu.CV.CvEnum.MorphOp.Close,
                CvInvoke.GetStructuringElement(
                    Emgu.CV.CvEnum.ElementShape.Rectangle,
                    new System.Drawing.Size(str_elem_close, str_elem_close),
                    new System.Drawing.Point(-1, -1)),
                new System.Drawing.Point(-1, -1),
                1,
                Emgu.CV.CvEnum.BorderType.Constant,
                new MCvScalar(0));

            combinedMasks._MorphologyEx(
                Emgu.CV.CvEnum.MorphOp.Open,
                CvInvoke.GetStructuringElement(
                    Emgu.CV.CvEnum.ElementShape.Rectangle,
                    new System.Drawing.Size(str_elem_open, str_elem_open),
                    new System.Drawing.Point(-1, -1)),
                new System.Drawing.Point(-1, -1),
                1,
                Emgu.CV.CvEnum.BorderType.Constant,
                new MCvScalar(0));

            hsvImage.Dispose();
            //hsvOriginalImage.Dispose();
            //hsvSplit[0].Dispose();
            //hsvSplit[1].Dispose();
            //hsvSplit[2].Dispose();
            hsvMask.Dispose();
            gImage.Dispose();
            cromaImage.Dispose();
            return Image.Copy(combinedMasks.Convert<Gray, byte>());
        }

    }
}