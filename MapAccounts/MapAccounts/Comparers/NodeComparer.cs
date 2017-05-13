using MapAccounts.Models.Maps.OSM;
using System.Collections.Generic;
using System;

namespace MapAccounts.Comparers
{
    internal class NodeComparer : IEqualityComparer<node>
    {
        public bool Equals(node x, node y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(node obj)
        {
            return obj.id.GetHashCode();
        }
    }
}