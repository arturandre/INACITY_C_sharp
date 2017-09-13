using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Helpers
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

        /// <summary>
        /// function rgb=hsl2rgb(hsl_in)
        /// </summary>
        /// <param name="bgrimage"></param>
        /// <returns></returns>
        public static Image<Bgr, byte> Hsl2Bgr(Image<Bgr, byte> bgrimage)
        {

            var doubleBgr = bgrimage.Convert<Bgr, float>() / 255;
            var numPoints = bgrimage.Rows * bgrimage.Cols;
            var H = new Image<Gray, float>(1, numPoints);
            var S = new Image<Gray, float>(1, numPoints);
            var L = new Image<Gray, float>(1, numPoints);
            var channels = doubleBgr.Split();

            MatHelper.CopyTo(ref channels[2], 0, 0, channels[2].Cols, channels[0].Rows,
                ref H, 0, 0, 1, numPoints);
            MatHelper.CopyTo(ref channels[1], 0, 0, channels[1].Cols, channels[1].Rows,
                ref S, 0, 0, 1, numPoints);
            MatHelper.CopyTo(ref channels[0], 0, 0, channels[0].Cols, channels[2].Rows,
                ref L, 0, 0, 1, numPoints);

            Image<Gray, float> auxImage = new Image<Gray, float>(L.Size);
            Image<Gray, Byte> auxImageByte = new Image<Gray, Byte>(L.Size);
            auxImage.SetValue(new Gray(0.5));
            Mat lowlidx = new Mat(L.Rows, L.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, L.NumberOfChannels);
            Mat nonlowlidx = new Mat(L.Rows, L.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, L.NumberOfChannels);
            CvInvoke.Compare(L, auxImage, lowlidx, Emgu.CV.CvEnum.CmpType.LessEqual);
            CvInvoke.BitwiseNot(lowlidx, nonlowlidx);
            

            //var lowLidx = L.ThresholdBinaryInv(new Gray(1.0 / 2.0), new Gray(1.0));
            //Mat q = new Mat(L.Rows, L.Cols, , L.NumberOfChannels);
            //Mat p = new Mat(L.Rows, L.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, L.NumberOfChannels);
            
            var partial1 = 1 + S;
            partial1._Mul(L);


            auxImageByte.SetValue(new Gray(255));
            CvInvoke.Divide(lowlidx, auxImageByte, lowlidx);
            Mat lowlidxFloat = new Mat();
            lowlidx.ConvertTo(lowlidxFloat, Emgu.CV.CvEnum.DepthType.Cv32F);
            CvInvoke.Multiply(partial1.Mat, lowlidxFloat, partial1.Mat);

            CvInvoke.Divide(nonlowlidx, auxImageByte, nonlowlidx);
            var partial2 = L.Mul(S);
            Mat nonlowlidxFloat = new Mat();
            nonlowlidx.ConvertTo(nonlowlidxFloat, Emgu.CV.CvEnum.DepthType.Cv32F);
            CvInvoke.Multiply(partial2.Mat, nonlowlidxFloat, partial2.Mat);
            partial2 = L + S - partial2;

            
            var q = partial1 + partial2;
            var p = 2 * L - q;

            var hk = H;

            var t1 = hk + 1 / 3.0;
            var t2 = hk;
            var t3 = hk - 1 / 3.0;

            var underidx1 = t1.ThresholdBinaryInv(new Gray(0), new Gray(1));
            var underidx2 = t2.ThresholdBinaryInv(new Gray(0), new Gray(1));
            var underidx3 = t3.ThresholdBinaryInv(new Gray(0), new Gray(1));

            var overidx1 = t1.ThresholdBinary(new Gray(1), new Gray(1));
            var overidx2 = t2.ThresholdBinary(new Gray(1), new Gray(1));
            var overidx3 = t3.ThresholdBinary(new Gray(1), new Gray(1));

            var t = new Image<Bgr, float>(new Image<Gray, float>[] { t1, t2, t3 });
            var underidx = new Image<Bgr, float>(new Image<Gray, float>[] { underidx2, underidx2, underidx3 });
            var overidx = new Image<Bgr, float>(new Image<Gray, float>[] { overidx1, overidx2, overidx3 });
            t = t + underidx - overidx;

            t1.Dispose();
            t2.Dispose();
            t3.Dispose();
            underidx1.Dispose();
            underidx2.Dispose();
            underidx3.Dispose();
            overidx1.Dispose();
            overidx2.Dispose();
            overidx3.Dispose();
            var tSplit = t.Split();
            t1 = tSplit[0];
            t2 = tSplit[1];
            t3 = tSplit[2];

            var range1_1 = t1.ThresholdBinaryInv(new Gray(1.0 / 6), new Gray(1.0));
            var range1_2 = t2.ThresholdBinaryInv(new Gray(1.0 / 6), new Gray(1.0));
            var range1_3 = t3.ThresholdBinaryInv(new Gray(1.0 / 6), new Gray(1.0));

            var range2_1 = t1.InRange(new Gray(1.0 / 6), new Gray(1.0 / 2)).Convert<Gray, float>() / 255.0;
            var range2_2 = t2.InRange(new Gray(1.0 / 6), new Gray(1.0 / 2)).Convert<Gray, float>() / 255.0;
            var range2_3 = t3.InRange(new Gray(1.0 / 6), new Gray(1.0 / 2)).Convert<Gray, float>() / 255.0;

            var range3_1 = t1.InRange(new Gray(1.0 / 2), new Gray(2.0 / 3)).Convert<Gray, float>() / 255.0;
            var range3_2 = t2.InRange(new Gray(1.0 / 2), new Gray(2.0 / 3)).Convert<Gray, float>() / 255.0;
            var range3_3 = t3.InRange(new Gray(1.0 / 2), new Gray(2.0 / 3)).Convert<Gray, float>() / 255.0;

            var range4_1 = t1.ThresholdBinary(new Gray(2.0 / 3), new Gray(1.0));
            var range4_2 = t2.ThresholdBinary(new Gray(2.0 / 3), new Gray(1.0));
            var range4_3 = t3.ThresholdBinary(new Gray(2.0 / 3), new Gray(1.0));

            var range1 = new Image<Bgr, float>(new Image<Gray, float>[] { range1_1, range1_2, range1_3 });
            var range2 = new Image<Bgr, float>(new Image<Gray, float>[] { range2_1, range2_2, range2_3 });
            var range3 = new Image<Bgr, float>(new Image<Gray, float>[] { range3_1, range3_2, range3_3 });
            var range4 = new Image<Bgr, float>(new Image<Gray, float>[] { range4_1, range4_2, range4_3 });

            var P = new Image<Bgr, float>(new Image<Gray, float>[] { p, p, p });
            var Q = new Image<Bgr, float>(new Image<Gray, float>[] { q, q, q });

            var rgb_c = (P + ((6 * (Q - P)).Mul(t))).Mul(range1) +
                Q.Mul(range2) +
                (P + (((Q - P) * 6).Mul(2 / 3 - t))).Mul(range3) +
                P.Mul(range4);
            rgb_c *= 255.0;

            var rgb_cShape = new Image<Bgr, float>(bgrimage.Cols, bgrimage.Rows);
            MatHelper.CopyTo(ref rgb_c, 0, 0, rgb_c.Cols, rgb_c.Rows, ref rgb_cShape, 0, 0, rgb_cShape.Cols, rgb_cShape.Rows);
            //var H = hsv.Split()[0];
            ////MatHelper.CopyTo(ref hsplit0, 0, 0, hsplit0.Cols, hsplit0.Rows, ref H, 0, 0, H.Cols, H.Rows);
            //var L_shape = new Image<Gray, float>(bgrimage.Cols, bgrimage.Rows);
            //var S_shape = new Image<Gray, float>(bgrimage.Cols, bgrimage.Rows);
            //MatHelper.CopyTo(ref S, 0, 0, S.Cols, S.Rows, ref S_shape, 0, 0, S_shape.Cols, S_shape.Rows);
            //MatHelper.CopyTo(ref L, 0, 0, L.Cols, L.Rows, ref L_shape, 0, 0, L_shape.Cols, L_shape.Rows);

            return rgb_cShape.Convert<Bgr, byte>();
        }

        public static Image<Bgr, byte> Bgr2Hsv(Image<Bgr, byte> bgrimage)
        {
            var aux = bgrimage.Convert<Bgr, float>() / 255;
            return (Bgr2Hsv(aux)*255).Convert<Bgr, Byte>();
        }

        public static Image<Bgr, float> Bgr2Hsv(Image<Bgr, float> bgrimage)
        {

            var bgrimageSplit = bgrimage.Split();
            var b = bgrimage[0];
            var g = bgrimage[1];
            var r = bgrimage[2];

            return Bgr2Hsv(b, g, r);
        }

        public static Image<Bgr, float> Bgr2Hsv(Image<Gray, float> grayimage)
        {
            var numPoints = grayimage.Rows;
            var b = new Image<Gray, float>(1, numPoints);
            var g = new Image<Gray, float>(1, numPoints);
            var r = new Image<Gray, float>(1, numPoints);
            MatHelper.CopyTo(ref grayimage, 0, 0, 1, numPoints,
                ref b, 0, 0, 1, numPoints);
            MatHelper.CopyTo(ref grayimage, 1, 0, 1, numPoints,
                ref g, 0, 0, 1, numPoints);
            MatHelper.CopyTo(ref grayimage, 2, 0, 1, numPoints,
                ref r, 0, 0, 1, numPoints);
            return Bgr2Hsv(b, g, r);
        }
        public static Image<Bgr, float> Bgr2Hsv(
            Image<Gray, float> b,
            Image<Gray, float> g,
            Image<Gray, float> r
            )
        {
            var v = r.Max(g).Max(b);
            var h = new Image<Gray, float>(v.Size);
            var s = v - r.Min(g).Min(b);
            Mat z = new Mat();
            var zeroAux = new Image<Gray, float>(s.Size);
            zeroAux.SetZero();
            //z = ~s;
            CvInvoke.Compare(s, zeroAux, z, Emgu.CV.CvEnum.CmpType.Equal);
            //s(z) = 1;
            s.Mat.SetTo(new MCvScalar(1), z);
            //k = (r == v);
            var k = new Mat();
            CvInvoke.Compare(r, v, k, Emgu.CV.CvEnum.CmpType.Equal);
            //h(k) = (g(k) - b(k))./ s(k);
            (g - b).Mul(1.0 / s).Mat.CopyTo(h, k);
            //k = (g == v);
            CvInvoke.Compare(g, v, k, Emgu.CV.CvEnum.CmpType.Equal);
            //h(k) = 2 + (b(k) - r(k))./ s(k);
            (2 + (b - r).Mul(1.0 / s)).Mat.CopyTo(h, k);
            //k = (b == v);
            CvInvoke.Compare(b, v, k, Emgu.CV.CvEnum.CmpType.Equal);
            //h(k) = 4 + (r(k) - g(k))./ s(k);
            (4 + (r - g).Mul(1.0 / s)).Mat.CopyTo(h, k);
            //h = h / 6;
            h._Mul(1.0 / 6.0);
            //k = (h < 0);
            CvInvoke.Compare(h, zeroAux, k, Emgu.CV.CvEnum.CmpType.Equal);
            //h(k) = h(k) + 1;
            (h + 1).Mat.CopyTo(h, k);
            //h(z) = 0;
            h.Mat.SetTo(new MCvScalar(0), z);

            //tmp = s./ v;
            var tmp = s.Mul(1.0 / v);
            //tmp(z) = 0;
            tmp.Mat.SetTo(new MCvScalar(0), z);
            //k = (v~= 0);
            //CvInvoke.FindNonZero(v, k);
            CvInvoke.Compare(v, zeroAux, k, Emgu.CV.CvEnum.CmpType.NotEqual);
            //s(k) = tmp(k);
            tmp.Mat.CopyTo(s, k);
            //s(~v) = 0;
            CvInvoke.BitwiseNot(k, k);
            s.Mat.SetTo(new MCvScalar(0), k);
            return new Image<Bgr, float>(new Image<Gray, float>[] { v, s, h });
        }

        public static Image<Bgr, byte> Bgr2Hsl(Image<Bgr, byte> bgrimage)
        {

            var doubleBgr = bgrimage.Convert<Bgr, float>() / 255;
            var numPoints = bgrimage.Rows * bgrimage.Cols;
            var bgrDoubleReshaped = new Image<Gray, float>(3, numPoints);
            var channels = doubleBgr.Split();
            MatHelper.CopyTo(ref channels[0], 0, 0, channels[0].Cols, channels[0].Rows,
                ref bgrDoubleReshaped, 0, 0, 1, numPoints);
            MatHelper.CopyTo(ref channels[1], 0, 0, channels[1].Cols, channels[1].Rows,
                ref bgrDoubleReshaped, 1, 0, 1, numPoints);
            MatHelper.CopyTo(ref channels[2], 0, 0, channels[2].Cols, channels[2].Rows,
                ref bgrDoubleReshaped, 2, 0, 1, numPoints);

            var mx = MatHelper.maxInLines(bgrDoubleReshaped);
            var mn = MatHelper.minInLines(bgrDoubleReshaped);

            Mat nonzeroidx = new Mat(mx.Rows, mx.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, mx.NumberOfChannels);
            Mat zeroidx = new Mat(mx.Rows, mx.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, mx.NumberOfChannels);

            var L = (mx + mn) / 2;
            var S = new Image<Gray, float>(L.Size);

            CvInvoke.Compare(mx, mn, zeroidx, Emgu.CV.CvEnum.CmpType.Equal);
            CvInvoke.BitwiseNot(zeroidx, nonzeroidx);

            Mat lowlidx = new Mat(mx.Rows, mx.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, mx.NumberOfChannels);
            var calc = new Image<Gray, float>(mx.Size);

            var mmsum = mx + mn;
            var mmsub = mx - mn;
            var auxImage = new Image<Gray, float>(L.Size);
            auxImage.SetValue(new Gray(0.5));
            ////CvInvoke.Threshold(L, lowlidx, 0.5, 1.0, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            CvInvoke.Compare(L, auxImage, lowlidx, Emgu.CV.CvEnum.CmpType.LessEqual);

            //CvInvoke.Divide(mmsub, mmsum, calc);
            calc = mmsub.Mul(1.0 / mmsum);
            Mat idx = new Mat();
            CvInvoke.BitwiseAnd(lowlidx, nonzeroidx, idx);
            calc.Mat.CopyTo(S, idx);

            Mat hilidx = new Mat(mx.Rows, mx.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, mx.NumberOfChannels);
            CvInvoke.Compare(L, auxImage, hilidx, Emgu.CV.CvEnum.CmpType.GreaterThan);
            var mmsum2 = (2 - (mx + mn));
            calc = mmsub.Mul(1.0 / mmsum2);
            CvInvoke.BitwiseAnd(hilidx, nonzeroidx, idx);
            calc.Mat.CopyTo(S, idx);
            //var hsv = doubleBgr.Convert<Hsv, float>() / 255;
            var hsv = Bgr2Hsv(bgrDoubleReshaped);
            var H = hsv.Split()[2];
            //MatHelper.CopyTo(ref hsplit0, 0, 0, hsplit0.Cols, hsplit0.Rows, ref H, 0, 0, H.Cols, H.Rows);
            var L_shape = new Image<Gray, float>(bgrimage.Cols, bgrimage.Rows);
            var S_shape = new Image<Gray, float>(bgrimage.Cols, bgrimage.Rows);
            var H_shape = new Image<Gray, float>(bgrimage.Cols, bgrimage.Rows);
            MatHelper.CopyTo(ref S, 0, 0, S.Cols, S.Rows, ref S_shape, 0, 0, S_shape.Cols, S_shape.Rows);
            MatHelper.CopyTo(ref L, 0, 0, L.Cols, L.Rows, ref L_shape, 0, 0, L_shape.Cols, L_shape.Rows);
            MatHelper.CopyTo(ref H, 0, 0, H.Cols, H.Rows, ref H_shape, 0, 0, H_shape.Cols, H_shape.Rows);
            CvInvoke.Transpose(H_shape, H_shape);
            CvInvoke.Transpose(S_shape, S_shape);
            CvInvoke.Transpose(L_shape, L_shape);
            var hsl = new Image<Bgr, float>(new Image<Gray, float>[] { L_shape, S_shape, H_shape });
            hsl *= 255;
            return hsl.Convert<Bgr, byte>();
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