using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.ComputerVision.ImageProcessing
{
    public static class ImageHelper
    {
        public static Image<Bgr, byte> MaskOverlay(Image<Bgr, byte> Image, Image<Gray, Byte> Mask)
        {
            Image<Bgr, byte> greenImage = Image.CopyBlank();
            greenImage.SetValue(new Bgr(0, 255, 0), Mask);
            Image<Bgr, byte> redImage = Image.CopyBlank();
            Mask._Not();
            redImage.SetValue(new Bgr(0, 0, 255), Mask);
            var colorMask = redImage.Add(greenImage);
            var blendedImage = Image.CopyBlank();
            CvInvoke.AddWeighted(Image, 0.8, colorMask, 0.2, 0, blendedImage);

            redImage.Dispose();
            greenImage.Dispose();
            colorMask.Dispose();

            return blendedImage;
        }
        public static Image<Bgr, byte> Bgr2Croma(Image<Bgr, byte> Image)
        {
            Mat ImageSquared = new Mat();
            CvInvoke.Multiply(Image, Image, ImageSquared, 1, Emgu.CV.CvEnum.DepthType.Cv32F);
            var channels = ImageSquared.Split();
            var sumChannels = new Mat();
            //sumChannels.SetTo(new MCvScalar(1, 1, 1));
            CvInvoke.Add(channels[0], channels[1], sumChannels);
            CvInvoke.Add(sumChannels, channels[2], sumChannels);
            Mat sqrtChannels = new Mat();
            CvInvoke.Sqrt(sumChannels, sqrtChannels);
            var zeroMask = new Mat(sqrtChannels.Size, sqrtChannels.Depth, sqrtChannels.NumberOfChannels);
            CvInvoke.Threshold(sqrtChannels, zeroMask, 0, 1, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            var onesMatrix = new Mat(sqrtChannels.Size, sqrtChannels.Depth, sqrtChannels.NumberOfChannels);
            onesMatrix.SetTo(new MCvScalar(1));
            try
            {
                CvInvoke.Add(sqrtChannels, onesMatrix, sqrtChannels, zeroMask.ToImage<Gray, byte>());
            }
            catch (Exception)
            {

                throw;
            }
            
            var mergedSum = new Mat();
            using (VectorOfMat vm = new VectorOfMat(sqrtChannels, sqrtChannels, sqrtChannels))
            { 
                CvInvoke.Merge(vm, mergedSum);
            }
            var cromaMat = new Mat();
            CvInvoke.Divide(Image, mergedSum, cromaMat, 1, Emgu.CV.CvEnum.DepthType.Cv32F);
            var cromaImage = cromaMat.ToImage<Bgr, float>();
            cromaImage._Mul(255);
            sumChannels.Dispose();
            mergedSum.Dispose();
            onesMatrix.Dispose();
            zeroMask.Dispose();
            return cromaImage.Convert<Bgr, byte>();
        }
    }
}