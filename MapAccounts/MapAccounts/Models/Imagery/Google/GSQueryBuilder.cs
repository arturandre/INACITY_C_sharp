using MapAccounts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MapAccounts.Models.Imagery.Google
{
    public class GSQueryBuilder
    {
        private const String _default_width = "640";
        private int _width = 640;
        private const String _default_height = "640";
        private int _height = 640;
        private const String _defaultSize = _default_width + "x" + _default_height;
        private String _size = _defaultSize;
        private double _lat = 0;
        private double _lng = 0;
        private double _heading = 0;
        private double _pitch = 0;
        private String _key = "";
        private string _panoid = "";

        public void setLocation(double lat, double lng)
        {
            _lat = lat;
            _lng = lng;
        }
        private String getLocation()
        {
            return _lat + "," + _lng;
        }
        

        public void setSize(int width, int height)
        {
            _width = width;
            _height = height;
            _size = width + "x" + height;
        }

        public GSQueryBuilder(double lat, double lng, String key)
        {
            setLocation(lat, lng);
            _key = key;
        }
        public GSQueryBuilder(double lat, double lng, int width, int height, double heading, double pitch, String key)
        {
            setLocation(lat, lng);
            setSize(width, height);
            _heading = heading;
            _pitch = pitch;
            _key = key;
        }
        public GSQueryBuilder(string panoid, int width, int height, double heading, double pitch, String key)
        {
            _panoid = panoid;
            setSize(width, height);
            _heading = heading;
            _pitch = pitch;
            _key = key;
        }
        public String getQueryLocation()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("?");
            sb.Append("size=" + _size);
            sb.Append("&");
            sb.Append("location=" + getLocation());
            sb.Append("&");
            sb.Append("heading=" + _heading.ToString(true));
            sb.Append("&");
            sb.Append("pitch=" + _pitch.ToString(true));
            sb.Append("&");
            sb.Append("key=" + _key);
            return sb.ToString();
        }
        public String getQueryPanoId()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("?");
            sb.Append("size=" + _size);
            sb.Append("&");
            sb.Append("pano=" + _panoid);
            sb.Append("&");
            sb.Append("heading=" + _heading.ToString(true));
            sb.Append("&");
            sb.Append("pitch=" + _pitch.ToString(true));
            sb.Append("&");
            sb.Append("key=" + _key);
            return sb.ToString();
        }

    }
}