using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;

namespace MapAccounts.Comparers
{
    public class PointComparer : IEqualityComparer<PointDTO>
    {
        public bool Equals(PointDTO x, PointDTO y)
        {
            return (Math.Abs(x.lat - y.lat) < 0.0000001) &&
                (Math.Abs(x.lng - y.lng) < 0.0000001);
        }

        public int GetHashCode(PointDTO obj)
        {
            return obj.lat.GetHashCode() + obj.lng.GetHashCode();
        }
    }
}