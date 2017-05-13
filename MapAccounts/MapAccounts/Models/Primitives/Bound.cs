using MapAccounts.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace MapAccounts.Models.Primitives
{
    public class Bound
    {
        public Bound() { }
        public enum ReturnFormat
        { ClockwiseSouth };

        //Minimum Latitude
        public double South { get; set; }
        //Minimum Longitude
        public double West { get; set; }
        //Maximum Latitude
        public double North { get; set; }
        //Maximum Longitude
        public double East { get; set; }

        public override string ToString()
        {
            return ToString(ReturnFormat.ClockwiseSouth);
        }
        public string ToString(ReturnFormat format)
        {
            if (format.Equals(ReturnFormat.ClockwiseSouth))
            {
                return South.ToString(true) + ", " +
                    West.ToString(true) + ", " +
                    North.ToString(true) + ", " +
                    East.ToString(true);
            }
            else
            {
                throw new Exception("ReturnFormat not recognized. Try ReturnFormat.ClockwiseSouth.");
            }
        }
        public String ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ Bounds:");
            sb.Append("{");
            sb.Append("North:");
            sb.Append(North);
            sb.Append(",South:");
            sb.Append(South);
            sb.Append(",East:");
            sb.Append(East);
            sb.Append(",West:");
            sb.Append(West);

            sb.Append("}");
            sb.Append("}");
            return sb.ToString();
        }
    }
}