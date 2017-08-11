using MapAccounts.Models.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapAccounts.Models.Maps.OSM
{
    public class OverpassQueryBuilder
    {

        private const String _defaultOutFormat = "json";
        private String _out_format = "json";
        public String outFormat
        {
            get { return "[out:" + _out_format + "]"; }
            set { _out_format = value; }
        }

        private const int _defaultTimeout = 900;
        private int _timeout = 900;
        private String Timeout
        {
            get { return "[timeout:" + _timeout + "]"; }
            set { _timeout = int.Parse(value); }
        }

        private const String _defaultOut = "body";
        private String _out = "body";
        private String Out
        {
            get { return _out; }
            set { _out = value; }
        }

        private Bound Bounds { get; set; }
        private String currentConfig = "";
        private List<String> currentStatements = new List<string>();
        
        public OverpassQueryBuilder(Bound _Bounds,
            String _outFormat = _defaultOutFormat,
            int _Timeout = _defaultTimeout,
            String _out = _defaultOut)
        {
            Bounds = _Bounds;
            outFormat = _outFormat;
            Timeout = _Timeout + "";
            Out = _out;
            currentConfig = getConfig();
        }

        public enum Element
        {
            node,
            way,
            relation
        }
        public enum FilterTag
        {
            surface,
            highway,
            amenity
        }
        public enum Surface
        {
            asphalt
        }
        public enum Highway
        {
            living_street,
            primary,
            secondary,
            tertiary,
            residential,
            trunk,
            service,
            bus_stop,
            traffic_signals,
            footway
        }

        private string getConfig(string _outFormat = null, String _timeout = null)
        {
            if (!String.IsNullOrEmpty(_outFormat)) outFormat = _outFormat;
            if (!String.IsNullOrEmpty(_timeout)) Timeout = _timeout;
            return outFormat + Timeout + ";";
        }

        private string getOut(String _out = null)
        {
            if (_out != null) Out = _out;
            StringBuilder sb = new StringBuilder();
            sb.Append("(._;>;);");
            sb.Append("out " + Out + ";");
            return sb.ToString();

        }

        private String getNode(Bound Bounds, FilterTag filterTag, String filterValue)
        {
            return "node(" + Bounds.ToString() + ")[" + filterTag + " = " + filterValue + "];";
        }
        private String getWay(Bound Bounds, FilterTag filterTag, String filterValue)
        {
            return "way(" + Bounds.ToString() + ")[" + filterTag + " = " + filterValue + "];";
        }

        #region amenityOverpassQuery
        public String amenityOverpassQuery(AmenityType amenity)
        {
            return amenityOverpassQuery(amenity.ToString());
        }

        public String busStopsOverpassQuery()
        {
            resetToDefault();

            StringBuilder sb = new StringBuilder();
            sb.Append(getConfig());
            sb.Append(
                "node(" + Bounds + ")[highway=bus_stop]->.all;" +
                "rel(bn.all);" +
                "node(r);" +
                "out meta;"
                );
            sb.Append(getOut());
            return sb.ToString();

        }
        public String amenityOverpassQuery(String amenity)
        {
            resetToDefault();
            pushStatement(Element.node, FilterTag.amenity, amenity);
            return assembleQuery();
        }
        #endregion amenityOverpassQuery

        private void pushStatement(Element element, FilterTag tag, String value)
        {
            currentStatements.Add(element.ToString() +
                "(" + Bounds + ")" +
                "[" + tag.ToString() + " = " + value + "];");
        }

        private String unionStatements()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            foreach (var statement in currentStatements)
            {
                sb.Append(statement);
            }
            sb.Append(");");
            return sb.ToString();
        }

        private void resetToDefault()
        {
            Timeout = _defaultTimeout + "";
            outFormat = _defaultOutFormat;
            Out = _defaultOut;
            clearStatements();
        }

        private void clearStatements()
        {
            currentStatements.Clear();
        }

        private String assembleQuery()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(getConfig());
            sb.Append(unionStatements());
            sb.Append(getOut());
            return sb.ToString();
        }

        public String highwayOverpassQuery()
        {
            resetToDefault();
            pushStatement(Element.way, FilterTag.surface, Surface.asphalt.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.living_street.ToString());
            pushStatement(Element.way, FilterTag.surface, Surface.asphalt.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.living_street.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.primary.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.secondary.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.tertiary.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.residential.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.trunk.ToString());
            pushStatement(Element.way, FilterTag.highway, Highway.service.ToString());
            return assembleQuery();
        }

    }
}