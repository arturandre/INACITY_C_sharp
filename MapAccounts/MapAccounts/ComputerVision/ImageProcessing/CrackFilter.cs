using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.Util;
using MapAccounts.Models.Primitives;
using MapAccounts.Models.Primitives.Converters;

namespace MapAccounts.ComputerVision.ImageProcessing
{
    public class CrackFilter : ImageFilter
    {
        public override FilterResultDTO.CaracteristicType FilterType
        {
            get
            {
                return FilterResultDTO.CaracteristicType.Cracks;
            }
        }

        public FilterResultDTO preFilter(Image<Bgr, byte> Image)
        {
            var cromaImg = ImageHelper.Bgr2Croma(Image);


            ///Removal of saturations greater than 80
            var hsvImg = cromaImg.Convert<Hsv, byte>();
            var grayMask = hsvImg.Split()[1].ThresholdBinaryInv(new Gray(80), new Gray(255));

            //Removal of half top of the image
            grayMask.ROI = new System.Drawing.Rectangle(0, 0, grayMask.Width - 1, (grayMask.Height / 2) - 1);
            grayMask.SetZero();
            grayMask.ROI = new System.Drawing.Rectangle();

            //Dilation for noise remotion
            grayMask._Dilate(2);

            //Trapezoidal area masking
            Image<Gray, byte> trapezoid = grayMask.CopyBlank();
            Point[] arrayOfPoints =
                {
                 new Point((int)((1.0/4.0)*trapezoid.Width), (int)((3.0/4.0)*trapezoid.Height)),
                 new Point((int)((3.0/4.0)*trapezoid.Width), (int)((3.0/4.0)*trapezoid.Height)),
                 new Point(trapezoid.Width-1, trapezoid.Height-1),
                 new Point(0, trapezoid.Height-1),
                 new Point((int)((1.0/4.0)*trapezoid.Width), (int)((3.0/4.0)*trapezoid.Height)),
            };
            VectorOfPoint vectorOfPoint = new VectorOfPoint(arrayOfPoints);
            CvInvoke.FillConvexPoly(trapezoid, vectorOfPoint, new MCvScalar(255));
            grayMask._And(trapezoid);

            //Gradient masking
            var gImage = Image.Convert<Gray, byte>();
            var gradImage = gradientByDilation(gImage);
            var mean = CvInvoke.Mean(gradImage);

            gradImage._ThresholdBinary(new Gray(mean.V0), new Gray(255));
            double nTotal = grayMask.CountNonzero()[0];
            grayMask._And(gradImage);
            double nCrack = grayMask.CountNonzero()[0];

            var outImage = ImageHelper.MaskOverlay(Image, grayMask);
            var bit = outImage.ToBitmap();

            cromaImg.Dispose();
            hsvImg.Dispose();
            outImage.Dispose();
            trapezoid.Dispose();
            gImage.Dispose();
            gradImage.Dispose();
            return new FilterResultDTO()
            {
                base64image = Base64Converter.BitmapToBase64(bit),
                Density = nCrack / nTotal,
                Type = FilterResultDTO.CaracteristicType.Cracks,
            };
        }

        public override FilterResultDTO filterImage(Image<Bgr, byte> input)
        {

            var prefiltered = preFilter(input);
            if (prefiltered.Density < 0.1)
            {
                prefiltered.base64image = Base64Converter.BitmapToBase64(input.ToBitmap());
                return prefiltered;
            }

            var segmented = segStreetBySatHist(input);
            segmented._Not();
            var mask = fitTrapeziumToMask(segmented);

            if (mask.CountNonzero()[0] < 30000)
            {
                prefiltered.base64image = Base64Converter.BitmapToBase64(input.ToBitmap());
                return prefiltered;
            }



            var xm = input.Convert<Gray, byte>();
            mask._Not();
            xm._Max(mask);
            mask._Not();


            int maxr = 0;
            int idx = 0;
            Image<Gray, byte> cracks = new Image<Gray, byte>(input.Size);
            Image<Gray, byte> aux = new Image<Gray, byte>(input.Size);
            Image<Gray, byte> maxCracks = new Image<Gray, byte>(input.Size);
            Random r = new Random();
            for (int t = 0; t <= 255; t++)
            {
                var xmCC = xm.ThresholdBinaryInv(new Gray(t), new Gray(255));
                int numberCC = 0;
                var contours = new VectorOfVectorOfPoint();
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(xmCC, contours, hierarchy, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                for (int i = 0; i < contours.Size; i++)
                {
                    if (contours[i].Size < 5) continue;
                    var rect = CvInvoke.MinAreaRect(contours[i]);
                    //var ellipse = CvInvoke.FitEllipse(contours[i]);
                    var ellipse = new Ellipse(rect);
                    var w = ellipse.RotatedRect.Size.Width;
                    var h = ellipse.RotatedRect.Size.Height;
                    var major = (w > h) ? w : h;
                    var minor = (w < h) ? w : h;
                    var eccentricity = Math.Sqrt(1 - ((minor * minor) / (major * major)));
                    if (eccentricity >= 0.90 && major >= 25 && major < 60)
                    {
                        using (VectorOfVectorOfPoint vvp = new VectorOfVectorOfPoint(contours[i]))
                        {
                            //Check skeleton
                            CvInvoke.FillPoly(aux, vvp, new MCvScalar(255));
                            var auxSkel = aux.Copy();

                            var skel = Skeletonization(auxSkel);
                            var ratio = aux.CountNonzero()[0] / skel.CountNonzero()[0];
                            if (ratio < 4)
                            {
                                numberCC++;
                                cracks._Or(aux);
                            }
                            auxSkel.Dispose();
                            skel.Dispose();
                        }
                    }
                    if (numberCC > maxr)
                    {
                        cracks.CopyTo(maxCracks);
                        cracks.SetZero();
                        maxr = numberCC;
                        idx = t;
                    }
                }
                contours.Dispose();
                hierarchy.Dispose();
                xmCC.Dispose();
            }

            var ret = ImageHelper.MaskOverlay(input, maxCracks);
            var nTotal = mask.CountNonzero()[0];
            var nCrack = nTotal;
            mask.Dispose();
            segmented.Dispose();
            xm.Dispose();
            segmented.Dispose();
            aux.Dispose();
            cracks.Dispose();
            maxCracks.Dispose();
            return new FilterResultDTO()
            {
                base64image = Base64Converter.BitmapToBase64(ret.Bitmap),
                Density = nCrack / nTotal,
                ProcessedArea = nTotal,
                Type = FilterResultDTO.CaracteristicType.Cracks,
            };

        }

        public static Image<Gray, byte> Skeletonization(Image<Gray, byte> img)
        {
            var skel = img.CopyBlank();
            var sqrEl =
                CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle,
                    new System.Drawing.Size(3, 3),
                    new System.Drawing.Point(-1, -1));
            while (img.CountNonzero()[0] > 0)
            {
                var temp = img.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Open, sqrEl,
                    new System.Drawing.Point(-1, -1),
                    1, Emgu.CV.CvEnum.BorderType.Constant,
                    new MCvScalar(255));
                temp._Not();
                temp._And(img);
                skel._Or(temp);
                img._Erode(1);


                temp.Dispose();
            }
            return skel;
        }

        /// <summary>
        /// Guardado para caso seja necessário apresentar casos que não funcionam
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        public FilterResultDTO OldfilterImageAnisoDiff(Image<Bgr, byte> Image)
        {

            var prefiltered = preFilter(Image);
            if (prefiltered.Density < 0.1)
            {
                return prefiltered;
            }

            var segmented = segStreetBySatHist(Image);
            segmented._Not();
            var mask = fitTrapeziumToMask(segmented);

            if (mask.CountNonzero()[0] < 30000)
            {
                prefiltered.base64image = Base64Converter.BitmapToBase64(Image.Copy(mask).ToBitmap());
                return prefiltered;
            }


            var gImg2 = Image.Convert<Gray, byte>();
            var gImg = gImg2.Convert<Gray, double>();

            //Anisotropic diffusion filter
            var auxImage = anisotropicDiffusion(gImg, 4, 60, .15, 2);
            gImg.Dispose();



            mask._Not();
            auxImage.SetValue(new Gray(255), mask);
            mask._Not();

            //Histogram diffused image
            var hist = getHistogram(auxImage);
            hist[255] = 0;
            //Histogram normalized (i.e. sum = 1)
            //hist = normalizeHistogram(hist);

            //Find otsu threshold for the histogram
            int otsuLevel = otsuThresholdFromHistrogram(hist);

            double[] otsuHist = new double[otsuLevel];

            for (int i = 0; i < otsuLevel; i++) otsuHist[i] = hist[i];

            double mean = getMeanFromHistogram(otsuHist);
            double std = getStdFromHistogram(otsuHist, mean);

            var TL = mean - 2 * std;

            var byteAuxImage = auxImage.Convert<Gray, byte>();
            auxImage.Dispose();
            byteAuxImage._ThresholdBinary(new Gray((int)TL), new Gray(255));

            var maskComp = mask.Not();
            mask._And(byteAuxImage);
            mask._Or(maskComp);
            maskComp.Dispose();

            var contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();

            CvInvoke.FindContours(mask, contours, hierarchy, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            List<RotatedRect> minEllipses = new List<RotatedRect>();
            for (int i = 0; i < contours.Size; i++)
            {
                if (contours[i].Size < 5) continue;
                var ellipse = CvInvoke.FitEllipse(contours[i]);
                var w = ellipse.Size.Width;
                var h = ellipse.Size.Height;
                var major = (w > h) ? w : h;
                var minor = (w < h) ? w : h;
                var eccentricity = Math.Sqrt(1 - ((minor * minor) / (major * major)));
                if (eccentricity >= 0.9 && major >= 25)
                {

                }

            }

            //CvInvoke.FitEllipse()

            var ret = ImageHelper.MaskOverlay(Image, mask);
            var nTotal = mask.CountNonzero()[0];
            var nCrack = nTotal;
            mask.Dispose();
            segmented.Dispose();
            return new FilterResultDTO()
            {
                base64image = Base64Converter.BitmapToBase64(ret.Bitmap),
                Density = nCrack / nTotal,
                ProcessedArea = nTotal,
                Type = FilterResultDTO.CaracteristicType.Cracks,
            };

        }

        #region Pre-filtering functions
        private Image<Gray, byte> gradientByDilation(Image<Gray, byte> input, int size = 1)
        {
            var d = input.Dilate(size);
            var delta = d.Sub(input);
            d.Dispose();
            return delta;
        }
        private Image<Gray, byte> gradientByErosion(Image<Gray, byte> input, int size = 1)
        {
            var e = input.Dilate(1);
            var delta = input.Sub(e);
            e.Dispose();
            return delta;
        }
        private Image<Gray, byte> gradientByTophatDual(Image<Gray, byte> input, int size = 1)
        {
            var thd = input.MorphologyEx(
                Emgu.CV.CvEnum.MorphOp.Blackhat,
                CvInvoke.GetStructuringElement(
                    Emgu.CV.CvEnum.ElementShape.Rectangle,
                    new Size(3, 3),
                    new Point(-1, -1)),
                new Point(-1, -1), size,
                Emgu.CV.CvEnum.BorderType.Constant,
                new MCvScalar(0));
            return thd;
        }
        #endregion Pre-filtering functions
        #region Mathematical helper functions

        public static double thresholdByChebchev(Image<Gray, byte> gImg, double percentile)
        {
            MCvScalar mean = new MCvScalar();
            MCvScalar std = new MCvScalar();
            CvInvoke.MeanStdDev(gImg, ref mean, ref std);
            return std.V0 * Math.Pow((1.0 - percentile), -0.5);
        }
        public static double[] getHistogram(Image<Gray, byte> gImg)
        {
            return getHistogram(gImg.Convert<Gray, double>());
        }
        public static double[] getHistogram(Image<Gray, double> gImg)
        {
            double[] ret = new double[256];
            for (int row = 0; row < gImg.Rows; row++)
            {
                for (int col = 0; col < gImg.Cols; col++)
                {
                    ret[(int)gImg.Data[row, col, 0]]++;
                }
            }
            return ret;
        }
        public static double getMeanFromHistogram(double[] hist)
        {
            double hsum = 0;
            for (int i = 0; i < hist.Length; i++)
            {
                hsum += hist[i] * i;
            }
            double total = sum(hist);
            return hsum / total;

        }
        public static double getStdFromHistogram(double[] hist, double mean)
        {
            double variance = 0;
            for (int i = 0; i < hist.Length; i++)
            {
                var diff = (i - mean);
                variance += diff * diff * hist[i];
            }
            variance /= sum(hist);
            return Math.Sqrt(variance);
        }
        public static int otsuThresholdFromHistrogram(double[] hist)
        {
            int level = 0;
            double hsum = sum(hist);
            int n = hist.Length;

            double sumB = 0;
            double wB = 0;
            double sum1 = 0;
            double maximum = 0.0;
            for (int i = 0; i < n; i++) sum1 += i * hist[i];
            for (int i = 1; i < n - 1; i++)
            {
                wB += hist[i];
                if (wB == 0) continue;
                double wF = hsum - wB;
                if (wF == 0) break;
                sumB += i * hist[i];
                double mB = sumB / wB;
                double mF = (sum1 - sumB) / wF;
                double between = wB * wF * (mB - mF) * (mB - mF);
                if (between >= maximum)
                {
                    level = i;
                    maximum = between;
                }
            }
            return level;
        }
        public static double getStdFromHistogram(double[] hist)
        {
            return getStdFromHistogram(hist, getMeanFromHistogram(hist));

        }

        /// <summary>
        /// Normalize the histogram so that the sum over all values is 1
        /// </summary>
        /// <param name="histogram">Input array with the values</param>
        /// <returns>Histogram normalized</returns>
        public static double[] normalizeHistogram(double[] histogram)
        {
            var hsum = sum(histogram);
            return prodArrayScalar(histogram, (1.0 / hsum));
        }
        public static double[] accumulatedSum(double[] inputArray)
        {
            if (inputArray.Length == 0) return new double[0];
            double[] ret = new double[inputArray.Length];
            ret[0] = inputArray[0];
            for (int i = 1; i < inputArray.Length; i++)
            {
                ret[i] = ret[i - 1] + inputArray[i];
            }
            return ret;
        }
        public static double sum(double[] inputArray)
        {
            if (inputArray.Length == 0) return 0;
            double ret = 0;
            for (int i = 0; i < inputArray.Length; i++)
            {
                ret += inputArray[i];
            }
            return ret;
        }
        public static double[] prodArrayScalar(double[] inputArray, double scalar)
        {
            if (inputArray.Length == 0) return new double[0];
            double[] ret = new double[inputArray.Length];
            for (int i = 0; i < inputArray.Length; i++)
            {
                ret[i] = inputArray[i] * scalar;
            }
            return ret;
        }

        /// <summary>
        /// Peter Kovesi - anisodiff
        /// </summary>
        /// <param name="gImg">grayscale image</param>
        /// <param name="niter">number of iterations</param>
        /// <param name="kappa">conduction coefficient 20-100 ?</param>
        /// <param name="lambda">max value of .25 for stability</param>
        /// <param name="option">1 Perona Malik diffusion equation No 1
        /// 2 Perona Malik diffusion equation No 2</param>
        /// <returns>diffused image</returns>
        public static Image<Gray, double> anisotropicDiffusion(
            Image<Gray, double> gImg,
            int niter,
            double kappa,
            double lambda,
            int option)
        {
            var rows = gImg.Rows;
            var cols = gImg.Cols;
            var diff = gImg.Clone();

            for (int i = 1; i <= niter; i++)
            {
                var diffl = new Image<Gray, double>(cols + 2, rows + 2);
                diffl.ROI = new Rectangle(1, 1, cols, rows);
                CvInvoke.cvCopy(diff, diffl, IntPtr.Zero);

                diffl.ROI = new Rectangle(1, 0, cols, rows);
                var deltaN = diffl.Sub(diff);
                diffl.ROI = new Rectangle(1, 2, cols, rows);
                var deltaS = diffl.Sub(diff);
                diffl.ROI = new Rectangle(2, 1, cols, rows);
                var deltaE = diffl.Sub(diff);
                diffl.ROI = new Rectangle(0, 1, cols, rows);
                var deltaW = diffl.Sub(diff);
                Image<Gray, double> cN, cS, cE, cW;
                cN = cS = cE = cW = null;

                cN = deltaN.Mul(1.0 / kappa);
                cN._Mul(deltaN);
                cS = deltaS.Mul(1.0 / kappa);
                cS._Mul(deltaS);
                cE = deltaE.Mul(1.0 / kappa);
                cE._Mul(deltaE);
                cW = deltaW.Mul(1.0 / kappa);
                cW._Mul(deltaW);

                if (option == 1)
                {
                    cN._Mul(-1);
                    CvInvoke.Exp(cN, cN);
                    cS._Mul(-1);
                    CvInvoke.Exp(cS, cS);
                    cE._Mul(-1);
                    CvInvoke.Exp(cE, cE);
                    cW._Mul(-1);
                    CvInvoke.Exp(cW, cW);
                }
                else if (option == 2)
                {
                    var sN = cN.Add(new Gray(1));
                    var sS = cS.Add(new Gray(1));
                    var sE = cE.Add(new Gray(1));
                    var sW = cW.Add(new Gray(1));

                    CvInvoke.Pow(sN, -1, cN);
                    CvInvoke.Pow(sS, -1, cS);
                    CvInvoke.Pow(sE, -1, cE);
                    CvInvoke.Pow(sW, -1, cW);
                    sN.Dispose();
                    sS.Dispose();
                    sE.Dispose();
                    sW.Dispose();
                }
                cN._Mul(deltaN);
                cS._Mul(deltaS);
                cE._Mul(deltaE);
                cW._Mul(deltaW);

                var sum = cN.CopyBlank();
                CvInvoke.Add(cN, cS, sum);
                CvInvoke.Add(sum, cE, sum);
                CvInvoke.Add(sum, cW, sum);
                sum._Mul(lambda);
                CvInvoke.Add(diff, sum, diff);
                cN.Dispose();
                cS.Dispose();
                cE.Dispose();
                cW.Dispose();
                sum.Dispose();
                diffl.Dispose();
            }

            return diff;
        }
        #endregion Mathematical helper functions


        #region Street segmentation functions
        public class Trapezium
        {
            private Point center;

            public Trapezium(Point center) { this.center = center; base_width = left_triangle = right_triangle = height = 1; }
            public Trapezium(Point center, int b, int t1, int t2, int h)
            {
                base_width = b;
                left_triangle = t1;
                right_triangle = t2;
                height = h;
            }
            private int base_width;

            public int BaseWidth
            {
                get { return base_width; }
                set { base_width = value; }
            }
            private int left_triangle;

            public int LeftTriangle
            {
                get { return left_triangle; }
                set { left_triangle = value; }
            }
            private int right_triangle;

            public int RightTriangle
            {
                get { return right_triangle; }
                set { right_triangle = value; }
            }
            private int height;

            public int Height
            {
                get { return height; }
                set { height = value; }
            }

            public Point[] getVertexCoordinates()
            {
                List<Point> ret = new List<Point>();
                ret.Add(new Point(center.X - base_width / 2 - left_triangle, center.Y));
                ret.Add(new Point(center.X - base_width / 2, center.Y - height));
                ret.Add(new Point(center.X + base_width / 2, center.Y - height));
                ret.Add(new Point(center.X + base_width / 2 + right_triangle, center.Y));
                ret.Add(new Point(center.X - base_width / 2 - left_triangle, center.Y));
                return ret.ToArray();
            }
        }
        private Image<Gray, byte> segStreetBySatHist(Image<Bgr, byte> bgrImage)
        {
            var hsvImage = bgrImage.Convert<Hsv, byte>();
            var split = hsvImage.Split();
            var sHist = getHistogram(split[1]);
            var accumulatedSum_sHist = accumulatedSum(sHist);

            //Vetor de soma aaccumulatedulada sobre o histograma normalizado (Soma total = 1) de saturações
            var accumulatedSumNormalized_sHist = accumulatedSum(normalizeHistogram(accumulatedSum_sHist));
            int idx = 0;
            for (; accumulatedSumNormalized_sHist[idx + 1] <= 0.1; idx++) ;
            split[1]._ThresholdBinaryInv(new Gray(idx), new Gray(255));
            split[2]._ThresholdBinaryInv(new Gray((int)(255 * 0.7)), new Gray(255));
            var mask = split[1].And(split[2]);

            //Only for noise removing
            CvInvoke.MorphologyEx(mask, mask, Emgu.CV.CvEnum.MorphOp.Close, CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(7, 7), new Point(-1, -1)), new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Replicate, new MCvScalar(0));
            Image<Gray, byte> ret = mask.Convert<Gray, byte>();
            hsvImage.Dispose();
            split[0].Dispose();
            split[1].Dispose();
            split[2].Dispose();
            mask.Dispose();
            return ret;
        }

        /// <summary>
        /// Increasing by dividing - trapezium
        /// </summary>
        /// <param name="bwImg"></param>
        /// <returns></returns>
        public static Image<Gray, byte> fitTrapeziumToMask(Image<Gray, byte> bwImg)
        {
            Point c = new Point((int)(bwImg.Width / 2.0), (int)(bwImg.Height - 1));
            Trapezium t = new Trapezium(c);
            int inistep = 16;
            int step = inistep;
            var black = bwImg.CopyBlank();

            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            var aux = black.And(bwImg);

            if (CvInvoke.CountNonZero(aux) != 0)
            {
                bwImg._Not();
                black.SetZero();
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                aux = black.And(bwImg);
            }

            while (CvInvoke.CountNonZero(aux) == 0
                && (c.X - t.BaseWidth / 2 - t.LeftTriangle > 0)
                && (c.X + t.BaseWidth / 2 + t.RightTriangle < black.Width))
            {
                t.BaseWidth += step;
                t.Height += step;
                t.LeftTriangle += step;
                t.RightTriangle += step;
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                CvInvoke.BitwiseAnd(black, bwImg, aux);
            }
            t.BaseWidth -= step;
            t.Height -= step;
            t.LeftTriangle -= step;
            t.RightTriangle -= step;

            t.BaseWidth = Math.Max(t.BaseWidth, 1);
            t.Height = Math.Max(t.BaseWidth, 1);
            t.LeftTriangle = Math.Max(t.BaseWidth, 1);
            t.RightTriangle = Math.Max(t.BaseWidth, 1);

            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            while (step > 0)
            {
                if (CvInvoke.CountNonZero(aux) == 0
                && (c.X - t.BaseWidth / 2 - t.LeftTriangle > 0)
                && (c.X + t.BaseWidth / 2 + t.RightTriangle < black.Width))
                {
                    t.BaseWidth += step;
                }
                else
                {
                    t.BaseWidth -= step;
                    step /= 2;
                }
                black.SetZero();
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                CvInvoke.BitwiseAnd(black, bwImg, aux);
            }
            if (t.BaseWidth < 0)
            {
                black.SetZero();
                return black;
            }
            while (CvInvoke.CountNonZero(aux) > 0)
            {
                t.BaseWidth--;
                black.SetZero();
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                CvInvoke.BitwiseAnd(black, bwImg, aux);
            }
            step = inistep;

            while (step > 0)
            {
                if (CvInvoke.CountNonZero(aux) == 0 && (c.X - t.BaseWidth / 2 - t.LeftTriangle > 0))
                {
                    t.LeftTriangle += step;
                }
                else
                {
                    t.LeftTriangle -= step;
                    step /= 2;
                }
                black.SetZero();
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                CvInvoke.BitwiseAnd(black, bwImg, aux);
            }
            t.LeftTriangle--;
            step = inistep;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            while (step > 0)
            {
                if (CvInvoke.CountNonZero(aux) == 0 && (c.X + t.BaseWidth / 2 + t.RightTriangle < black.Width))
                {
                    t.RightTriangle += step;
                }
                else
                {
                    t.RightTriangle -= step;
                    step /= 2;
                }
                black.SetZero();
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                CvInvoke.BitwiseAnd(black, bwImg, aux);
            }
            t.RightTriangle--;
            step = inistep;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            while (step > 0)
            {
                if (CvInvoke.CountNonZero(aux) == 0 && t.Height < black.Height)
                {
                    t.Height += step;
                }
                else
                {
                    t.Height -= step;
                    step /= 2;
                }
                black.SetZero();
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                CvInvoke.BitwiseAnd(black, bwImg, aux);
            }
            t.Height--;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            return black;
        }

        /// <summary>
        /// Increasing by unit - trapezium
        /// </summary>
        /// <param name="bwImg"></param>
        /// <returns></returns>
        public static Image<Gray, byte> fitTrapeziumToMask1(Image<Gray, byte> bwImg)
        {
            Point c = new Point((int)(bwImg.Width / 2.0), (int)(bwImg.Height - 1));
            Trapezium t = new Trapezium(c);

            var black = bwImg.CopyBlank();

            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            var aux = black.And(bwImg);
            while (CvInvoke.CountNonZero(aux) == 0)
            {
                t.BaseWidth++;
                t.Height++;
                t.LeftTriangle++;
                t.RightTriangle++;
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
                CvInvoke.BitwiseAnd(black, aux, aux);
            }
            t.BaseWidth--;
            t.Height--;
            t.LeftTriangle--;
            t.RightTriangle--;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            while (CvInvoke.CountNonZero(black.And(bwImg)) == 0
                && (c.X - t.BaseWidth / 2 - t.LeftTriangle > 0)
                && (c.X + t.BaseWidth / 2 + t.RightTriangle < black.Width))
            {
                t.BaseWidth++;
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            }
            t.BaseWidth--;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            while (CvInvoke.CountNonZero(black.And(bwImg)) == 0 && (c.X - t.BaseWidth / 2 - t.LeftTriangle > 0))
            {
                t.LeftTriangle++;
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            }
            t.LeftTriangle--;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            while (CvInvoke.CountNonZero(black.And(bwImg)) == 0 && (c.X + t.BaseWidth / 2 + t.RightTriangle < black.Width))
            {
                t.RightTriangle++;
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            }
            t.RightTriangle--;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            while (CvInvoke.CountNonZero(black.And(bwImg)) == 0 && t.Height < black.Height)
            {
                t.Height++;
                black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            }
            t.Height--;
            black.SetZero();
            black.FillConvexPoly(t.getVertexCoordinates(), new Gray(255));
            return black;
        }
        #endregion Street segmentation functions
    }
}