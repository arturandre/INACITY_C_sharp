using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapAccountsTests
{
    public class Street
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Trecho[][] Trechos { get; set; }
    }

    public class Trecho
    {
        public string ID { get; set; }
        public float lat { get; set; }
        public float lng { get; set; }
        public object PanoramaDTO { get; set; }
    }


}
