using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Helpers
{
    public static class MatHelper
    {
        public static void CopyTo(ref Image<Gray, float> original,
            int origin_x,
            int origin_y,
            int origin_width,
            int origin_height,
            ref Image<Gray, float> destination,
            int destin_x,
            int destin_y,
            int destin_width,
            int destin_height)
        {
            CvInvoke.Transpose(original, original);
            int p = 0;
            for (int i = origin_y; i < origin_y + origin_height; i++)
            {
                for (int j = origin_x; j < origin_x + origin_width; j++)
                {
                    //int dx = (int)Math.Truncate(p / (float)destin_height);
                    int dx = (int)p % destin_width;
                    int dy = (int)Math.Truncate(p / (float)destin_width);
                    //int dy = p % destin_height;
                    //int dy = p % destin_height;
                    destination.Data[destin_y + dy, destin_x + dx, 0] = original.Data[i, j, 0];
                    p++;
                }
            }
            CvInvoke.Transpose(original, original);
        }

        public static Image<Gray, float> maxInLines(Image<Gray, float> bgrimage)
        {
            var channels = bgrimage.Split();
            var mx = new Image<Gray, float>(1, bgrimage.Rows);
            for (int k = 0; k < bgrimage.NumberOfChannels; k++)
            {
                for (int i = 0; i < bgrimage.Rows; i++)
                {
                    mx.Data[i, 0, k] = float.NaN;
                    for (int j = 0; j < bgrimage.Cols; j++)
                    {
                        var val = channels[k].Data[i, j, 0];
                        if (float.IsNaN(mx.Data[i, 0, k]) || val > mx.Data[i, 0, k]) mx.Data[i, 0, k] = val;
                    }
                }
            }
            return mx;
        }
        public static Image<Gray, float> minInLines(Image<Gray, float> bgrimage)
        {
            var channels = bgrimage.Split();
            var mn = new Image<Gray, float>(1, bgrimage.Rows);
            for (int k = 0; k < bgrimage.NumberOfChannels; k++)
            {
                for (int i = 0; i < bgrimage.Rows; i++)
                {
                    mn.Data[i, 0, k] = float.NaN;
                    for (int j = 0; j < bgrimage.Cols; j++)
                    {
                        var val = channels[k].Data[i, j, 0];
                        if (float.IsNaN(mn.Data[i, 0, k]) || val < mn.Data[i, 0, k]) mn.Data[i, 0, k] = val;
                    }
                }
            }
            return mn;
        }

        internal static void CopyTo(ref Image<Bgr, float> original,
            int origin_x,
            int origin_y,
            int origin_width,
            int origin_height,
            ref Image<Bgr, float> destination,
            int destin_x,
            int destin_y,
            int destin_width,
            int destin_height)
        {
            for (int k = 0; k < 3; k++)
            {
            int p = 0;
                for (int i = origin_y; i < origin_y + origin_height; i++)
                {
                    for (int j = origin_x; j < origin_x + origin_width; j++)
                    {
                        //int dx = (int)Math.Truncate(p / (float)destin_height);
                        int dx = (int)p % destin_width;
                        int dy = (int)Math.Truncate(p / (float)destin_width);
                        //int dy = p % destin_height;
                        //int dy = p % destin_height;
                        destination.Data[destin_y + dy, destin_x + dx, k] = original.Data[i, j, k];
                        p++;
                    }
                }
            }
        }
    }
}