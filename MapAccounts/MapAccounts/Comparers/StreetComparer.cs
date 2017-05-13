using System.Collections.Generic;
using MapAccounts.Models.Primitives;

namespace MapAccounts.Comparers
{
    internal class StreetComparer : IEqualityComparer<StreetDTO>
    {
        public bool Equals(StreetDTO x, StreetDTO y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(StreetDTO obj)
        {
            return obj.GetHashCode();
        }
    }
}