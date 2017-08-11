using MapAccounts.Comparers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Models.Maps.OSM
{
    [JsonConverter(typeof(OSMConverter))]
    public class OSMResult
    {
        public String version { get; set; }
        public String generator { get; set; }
        public osm3s osm3s { get; set; }
        public ICollection<node> nodes { get; set; }

        private Dictionary<long, node> NodesDictionary = null;
        public Dictionary<long, node> GetNodesAsDictionary
        {
            get
            {
                if (NodesDictionary == null) NodesDictionary = nodes.Distinct(new OSMNodeComparer()).ToDictionary<node, long>(p => p.id);
                return NodesDictionary;
            }
        }
        public ICollection<way> ways { get; set; }
        private Dictionary<long, way> WaysDictionary = null;
        public Dictionary<long, way> GetWaysAsDictionary
        {
            get
            {
                if (WaysDictionary == null) WaysDictionary = ways.ToDictionary<way, long>(p => p.id);
                return WaysDictionary;
            }
        }
    }
}