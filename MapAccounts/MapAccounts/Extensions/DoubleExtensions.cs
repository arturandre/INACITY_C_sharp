using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MapAccounts.Extensions
{
    public static class DoubleExtensions
    {
        private static NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = "." };
        public static string ToString(this double some, bool useDotAsDecimalSeparator)
        {
            if (useDotAsDecimalSeparator)
                return some.ToString(nfi);
            else
                return some.ToString();
        }
    }
}